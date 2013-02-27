using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PointGaming.Desktop.POCO;
using SocketIOClient;
using SocketIOClient.Messages;
using RestSharp;

namespace PointGaming.Desktop
{
    public delegate void LogInCompleted(SocketSession caller, bool isSuccess);

    public class SocketSession
    {
        public Client MyClient;
        private AuthEmit _authEmit;
        public UserDataManager Data = new UserDataManager();

        private AutoResetEvent socketWorker = new AutoResetEvent(false);
        public string AuthToken { get; set; }

        private class CallbackAction
        {
            public int ThreadId;
            public Action Action;
            public Action Callback;
        }

        private Dictionary<int, Action<Action>> _threadQueuers = new Dictionary<int, Action<Action>>();
        private readonly List<CallbackAction> _workQueue = new List<CallbackAction>();

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
            while (!App.IsShuttingDown)
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
            bool isSuccess = false;
            try
            {
                var baseUrl = Properties.Settings.Default.BaseUrl;
                var client = new RestClient(baseUrl);

                var request = new RestRequest("sessions", Method.POST);
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
                    Data.User.Username = username;
                    Data.User.Id = apiResponse.Data._id;
                    Properties.Settings.Default.Username = username;
                    Properties.Settings.Default.Save();

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
        }

        private void DestroySession()
        {
            try
            {
                var baseUrl = Properties.Settings.Default.BaseUrl + "sessions/destroy?auth_token=" + AuthToken;
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.DELETE);
                client.Execute<ApiResponse>(request);
            }
            catch (Exception e)
            {
                App.LogLine(e.Message);
            }

            AuthToken = "";
            Data.User.Username = "";
            Data.User.Id = "";

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
