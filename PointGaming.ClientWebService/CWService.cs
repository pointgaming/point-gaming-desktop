using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;

namespace PointGaming.ClientWebService
{
    // http://localhost:9779/joinchat/1111?username=dean&password=5555
    // http://localhost:9779/info
    //
    // to install/uninstall service:
    // open cmd as admin
    // g:
    // cd "whitepaperclip\point-gaming-desktop\output\Debug"
    // "C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe" PointGaming.ClientWebService.exe
    // "C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe" /u PointGaming.ClientWebService.exe


    public partial class CWService : System.ServiceProcess.ServiceBase
    {
        private WcfServerConnection _wcfHost;
        private SimpleHttpServer _httpServer;

        private readonly static List<string> _appendLines = new List<string>();

        public static void AppendConsoleLine(string s)
        {
            Console.WriteLine(s);
            lock (_appendLines)
            {
                _appendLines.Add(s);
            }
        }

        public static string GetAppendedLines()
        {
            string lines;
            lock (_appendLines)
            {
                lines = string.Join("\r\n", _appendLines.ToArray());
                _appendLines.Clear();
            }
            return lines;
        }

        public CWService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _wcfHost = new WcfServerConnection();
            _wcfHost.Start();

            var prefixes = new[]{
                "http://localhost:9779/",
                "http://local.pointgaming.com:9779/",
                "http://127.0.0.1:9779/",
            };
            
            _httpServer = new SimpleHttpServer();
            _httpServer.Start(prefixes, ListenerCallback);
        }

        protected override void OnStop()
        {
            if (_wcfHost == null)
                return;
            _httpServer.Dispose();
            _wcfHost.Dispose();
            _wcfHost = null;
            _httpServer = null;
        }

        static void Main(string[] args)
        {
            System.ServiceProcess.ServiceBase[] ServicesToRun = new System.ServiceProcess.ServiceBase[]
            {
                new CWService()
            };
            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
        }

        public static System.IO.FileInfo GetProgramFileInfo()
        {
            string file = (new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
            System.IO.FileInfo fi = new System.IO.FileInfo(file);
            var folderInfo = fi.Directory;
            var programFileInfo = folderInfo.GetFiles("PointGaming.exe")[0];
            return programFileInfo;
        }

        private static string GetVersion()
        {
            var programFileInfo = GetProgramFileInfo();

            var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(programFileInfo.FullName);
            return versionInfo.ProductVersion;
        }

        public void ListenerCallback(IAsyncResult result)
        {
            HttpListener listener = null;
            HttpListenerContext context = null;

            try
            {
                listener = (HttpListener)result.AsyncState;
                context = listener.EndGetContext(result);
            }
            catch (Exception e)
            {
                CWService.AppendConsoleLine(e.Message);
                CWService.AppendConsoleLine(e.StackTrace);
                return;
            }

            try
            {
                HandleRequest(context);
            }
            catch (Exception e)
            {
                CWService.AppendConsoleLine(e.Message);
                CWService.AppendConsoleLine(e.StackTrace);
                return;
            }
            finally
            {
                context.Response.OutputStream.Close();
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            GetAppendedLines();

            var request = context.Request;

            var segments = request.Url.Segments;

            if (segments.Length == 0 || segments[0] != "/")
                return;

            if (segments.Length == 1)
            {
                HandlePlain(context);
                return;
            }

            var firstSeg = segments[1];
            if (firstSeg == "info" || firstSeg == "info/")
            {
                HandleInfo(context);
            }
            else if (firstSeg == "joinchat" || firstSeg == "joinchat/")
            {
                HandleJoinChat(context);
            }
            else
                Handle404(context);

            GetAppendedLines();
        }

        private static void Handle404(HttpListenerContext context)
        {
            var response = context.Response;
            SimpleHttpServer.WriteResponse(response, "404: Document not found.");
            response.StatusCode = 404;
        }

        private static void Handle402(HttpListenerContext context, string message)
        {
            var response = context.Response;
            SimpleHttpServer.WriteResponse(response, message);
            response.StatusCode = 402;
        }

        private void HandleJoinChat(HttpListenerContext context)
        {
            var segments = context.Request.Url.Segments;
            if (segments.Length != 3)
            {
                Handle404(context);
                return;
            }
            var chatId = segments[2];
            var vars = SimpleHttpServer.ExplodeQuery(context.Request.Url.Query);

            var missing = new List<string>();
            var requiredVars = new[] { "username", "password" };
            if (!SimpleHttpServer.Require(vars, requiredVars, missing))
            {
                Handle402(context, CreateMissingMessage(missing));
                return;
            }

            _wcfHost.JoinChat(vars["username"], vars["password"], chatId);

            var response = context.Response;
            SimpleHttpServer.WriteResponse(response, "ok");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Headers", "X-Requested-With");
        }

        private static string CreateMissingMessage(List<string> missing)
        {
            return "Missing: " + string.Join(", ", missing.ToArray());
        }

        private static void HandleInfo(HttpListenerContext context)
        {
            var response = context.Response;
            SimpleHttpServer.WriteResponse(response, "{ \"version\": \"" + GetVersion() + "\" }");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Headers", "X-Requested-With");
        }

        private static void HandlePlain(HttpListenerContext context)
        {
            var response = context.Response;
            SimpleHttpServer.WriteResponse(response, "Point Gaming ClientWebService");
        }
    }

    public class SimpleHttpServer : IDisposable
    {
        private bool _shouldListen;
        private AsyncCallback _responseHandler;

        public void Start(string[] prefixes, AsyncCallback responseHandler)
        {
            _responseHandler = responseHandler;
            HttpListener listener = new HttpListener();
            foreach (var prefix in prefixes)
                listener.Prefixes.Add(prefix);
            listener.Start();
            CWService.AppendConsoleLine("Listening...");

            Thread t = new Thread(Listen);
            t.IsBackground = false;
            t.Name = "Http Listener";
            _shouldListen = true;
            t.Start(listener);
        }

        public void Dispose()
        {
            Stop();
        }

        public void Stop()
        {
            _shouldListen = false;
        }

        private void Listen(object param)
        {
            var listener = param as HttpListener;
            try
            {

                IAsyncResult asynchResult = null;

                while (_shouldListen)
                {
                    asynchResult = listener.BeginGetContext(_responseHandler, listener);
                    while (_shouldListen)
                    {
                        if (asynchResult.AsyncWaitHandle.WaitOne(200))
                        {
                            asynchResult = null;
                            break;
                        }
                    }
                }

                if (asynchResult != null)
                {
                    asynchResult.AsyncWaitHandle.Close();
                    asynchResult = null;
                }
            }
            catch (Exception e)
            {
                CWService.AppendConsoleLine("Todo: log the exception: " + e);
            }
            finally
            {
                listener.Stop();
            }
        }

        public static Dictionary<string, string> ExplodeQuery(string queryString)
        {
            Dictionary<string, string> res = new Dictionary<string,string>();
            var pairs = queryString.Split(_Seperator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var assign = pair.Split(_Equals, StringSplitOptions.None);
                if (assign.Length == 1)
                    res.Add(UrlDecode(assign[0]), "");
                if (assign.Length == 2)
                    res.Add(UrlDecode(assign[0]), UrlDecode(assign[1]));
            }
            return res;
        }
        private static readonly char[] _Equals = new char[] { '=' };
        private static readonly char[] _Seperator = new char[] { '?', '&' };

        private static string UrlDecode(string s)
        {
            return Uri.UnescapeDataString(s);
        }


        public static bool Require(Dictionary<string, string> vars, string[] requiredVars, List<string> missing)
        {
            var count = missing.Count;
            foreach (var req in requiredVars)
                if (!vars.ContainsKey(req))
                    missing.Add(req);
            return missing.Count == count;
        }


        public static void WriteResponse(HttpListenerResponse response, string responseString)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
        }
    }

}
