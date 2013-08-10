using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PointGaming.POCO;
using SocketIOClient;
using SocketIOClient.Messages;
using RestSharp;

namespace PointGaming
{
    public delegate void LogInCompleted(SocketSession caller, bool isSuccess);

    public class SocketSession
    {
        public Client MyClient;
        private AuthEmit _authEmit;
        private bool _processWorkQueue = true;
        
        private AutoResetEvent socketWorker = new AutoResetEvent(false);
        public string AuthToken { get; set; }
        public byte[] AuthTokenBytes
        {
            get
            {
                var chars = AuthToken.Replace("-", "");
                return StringToByteArrayFastest(chars);
            }
        }

        public static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        private class CallbackAction
        {
            public int ThreadId;
            public Action Action;
            public Action Callback;
        }

        private Dictionary<int, Action<Action>> _threadQueuers = new Dictionary<int, Action<Action>>();
        private readonly List<CallbackAction> _workQueue = new List<CallbackAction>();

        public UserBase User;

        public string GetWebAppFunction(string apiPath, string function, params string[] arguments)
        {
            var result = Properties.Settings.Default.WebServerUrl + apiPath + function;

            var args = new List<string>(arguments);
            if (!string.IsNullOrEmpty(AuthToken))
                args.Add("auth_token=" + AuthToken);

            for (int i = 0; i < args.Count; i++)
            {
                var prefix = "&";
                if (i == 0)
                    prefix = "?";
                result = result + prefix + args[i];
            }
            return result;
        }

        public string GetWebApiV1Function(string function, params string[] arguments)
        {
            return GetWebAppFunction("/api/v1", function, arguments);
        }

        public SocketSession()
        {
            var t = new Thread(DoWork);
            t.IsBackground = true;
            t.Name = "socket.io worker";
            t.Start();
        }

        public void SetThreadQueuerForCurrentThread(Action<Action> queuer)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            lock (_threadQueuers)
            {
                _threadQueuers[threadId] = queuer;
            }
        }

        public void EmitLater(string eventName, dynamic parameter)
        {
            Begin(delegate { MyClient.Emit(eventName, parameter); });
        }

        public void Emit(string eventName, dynamic parameter)
        {
            MyClient.Emit(eventName, parameter);
        }

        public void Begin(Action action)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var ca = new CallbackAction { Action = action, Callback = null, ThreadId = threadId, };
            lock (_workQueue)
            {
                _workQueue.Add(ca);
            }
            socketWorker.Set();
        }

        public void BeginAndCallback(Action action, Action callback)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var ca = new CallbackAction { Action = action, Callback = callback, ThreadId = threadId,};
            lock (_workQueue)
            {
                _workQueue.Add(ca);
            }
            socketWorker.Set();
        }
        
        public static RestResponse<T> Rest<T>(string url, Method method, object body) where T : new()
        {
            var client = new RestClient(url);
            var request = new RestRequest(method) { RequestFormat = RestSharp.DataFormat.Json };
            if (body != null)
                request.AddBody(body);
            RestResponse<T> response = (RestResponse<T>)client.Execute<T>(request);
            return response;
        }

        public void OnThread(string eventName, Action<IMessage> action)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Action<Action> queuer;
            lock (_threadQueuers)
            {
                queuer = _threadQueuers[threadId];
            }

            MyClient.On(eventName, new QueueInvoker(queuer, action).Invoke);
        }

        public class QueueInvoker
        {
            private readonly Action<Action> _queuer;
            private readonly Action<IMessage> _action;

            public QueueInvoker(Action<Action> queuer, Action<IMessage> action)
            {
                _queuer = queuer;
                _action = action;
            }

            public void Invoke(IMessage message)
            {
                _queuer(() => _action(message));
            }
        }

        private void DoWork()
        {
            while (_processWorkQueue && !App.IsShuttingDown)
            {
                socketWorker.WaitOne();

                List<CallbackAction> newWork;
                lock (_workQueue)
                {
                    newWork = new List<CallbackAction>(_workQueue);
                    _workQueue.Clear();
                }

                foreach (var item in newWork)
                {
                    try
                    {
                        item.Action();
                        if (item.Callback != null)
                        {
                            Action<Action> queuer;
                            lock (_threadQueuers)
                            {
                                queuer = _threadQueuers[item.ThreadId];
                            }
                            queuer(item.Callback);
                        }
                    }
                    catch (Exception e)
                    {
                        App.LogLine(e.Message);
                    }
                }
            }
        }

        private class LoginInfo
        {
            public readonly string Username;
            public readonly string Password;
            public readonly LogInCompleted TryAction;

            public LoginInfo(string username, string password, LogInCompleted tryAction)
            {
                Username = username;
                Password = password;
                TryAction = tryAction;
            }
        }

        private bool _isAuthorized;
        private bool _isAuthResponded;

        public bool Login(string username, string password, DateTime timeout)
        {
            _isAuthorized = false;
            _isAuthResponded = false;
            bool isSuccess = false;
            try
            {
                var baseUrl = GetWebApiV1Function("/sessions");
                var client = new RestClient(baseUrl);

                var request = new RestRequest(Method.POST);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddBody(new UserLogin { username = username, password = password });

                var oldTimeout = client.Timeout;
                client.Timeout = (int)((DateTime.Now - timeout).TotalMilliseconds);
                var apiResponse = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);
                isSuccess = apiResponse.IsOk();
                client.Timeout = oldTimeout;

                if (isSuccess)
                {
                    isSuccess = false;

                    AuthToken = apiResponse.Data.auth_token;
                    User = new UserBase {_id = apiResponse.Data._id, username = username };
                    
                    ConnectSocket();
                    while (DateTime.Now < timeout && !_isAuthResponded)
                        Thread.Sleep(25);
                    isSuccess = _isAuthorized;

                    if (!isSuccess)
                        MyClient.Close();
                }
            }
            catch (Exception e)
            {
                App.LogLine(e.Message);
            }

            return isSuccess;
        }

        public void Logout()
        {
            Disconnect();
            DestroySession();
            _processWorkQueue = false;
        }

        private void DestroySession()
        {
            if (!string.IsNullOrEmpty(AuthToken))
            {
                try
                {
                    var baseUrl = GetWebApiV1Function("/sessions/destroy");
                    var client = new RestClient(baseUrl);
                    var request = new RestRequest(Method.DELETE);
                    client.Execute<ApiResponse>(request);
                }
                catch (Exception e)
                {
                    App.LogLine(e.Message);
                }
            }

            AuthToken = "";
            User.username = "";
            User._id = "";

            App.LogLine("Logged in session ended.");
        }

        public void ConnectSocket()
        {
            MyClient = new Client(Properties.Settings.Default.SocketIoUrl);
            MyClient.On("connect", OnConnect);
            MyClient.On("auth_resp", OnAuthResponse);

            App.LogLine("Client socket connecting...");
            MyClient.Connect();
        }

        private void Disconnect()
        {
            try
            {
                MyClient.Close();
                MyClient = null;
            }
            catch (Exception e)
            {
                App.LogLine(e.Message);
            }

            App.LogLine("Socket disconnected.");
        }

        private void OnConnect(IMessage message)
        {
            try
            {
                _authEmit = new AuthEmit { auth_token = AuthToken };
                MyClient.Emit("auth", _authEmit);
            }
            catch (Exception e)
            {
                App.LogLine(e.Message);
            }

            App.LogLine("Client socket connected.");
        }
        
        private void OnAuthResponse(IMessage data)
        {
            bool isSuccess = false;
            try
            {
                var response = data.Json.GetFirstArgAs<ApiResponse>();
                isSuccess = response.success;
            }
            catch (Exception ex)
            {
                App.LogLine(ex.Message);
            }

            if (isSuccess)
            {
                App.LogLine("Client socket authorized.");
                _isAuthorized = true;
            }
            else
            {
                App.LogLine("Error: client socket is not authorized.");
            }
            _isAuthResponded = true;
        }
    }
}
