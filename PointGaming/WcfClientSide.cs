using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.ServiceModel;
using PointGaming.ClientWebService;


namespace PointGaming
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public partial class WcfClientSide : PointGaming.ClientWebService.IWcfClientSide
    {
        private InstanceContext _context = null;
        private PointGaming.ClientWebService.WcfClientConnection _proxy = null;
                
        public static void AppStarted()
        {
            var t = new Thread((ThreadStart)delegate
            {
                try
                {
                    WcfClientSide wcf = new WcfClientSide();
                    wcf.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to connect to PointGaming client web service: " + e.Message);
                    //Console.WriteLine(e.StackTrace);
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        public void Start()
        {
            _context = new InstanceContext(this);
            _proxy = new PointGaming.ClientWebService.WcfClientConnection(_context);
            _proxy.RegisterClientWithServer();
        }

        public void LoginAndJoinChat(string username, string password, string chatId)
        {
            ((Action)delegate {
                LoginAndJoinChatOnUI(username, password, chatId);
            }).BeginInvokeUI();
        }

        public void LoginAndJoinChatOnUI(string username, string password, string chatId)
        {
            Console.WriteLine("LoginAndJoinChatOnUI {0}, {1}, {2}", username, password, chatId);

            bool isLoggedIn = UserDataManager.UserData != null;
            bool isCorrectLogin = isLoggedIn && UserDataManager.UserData.User.Username == username;

            bool needToLogout = isLoggedIn && !isCorrectLogin;
            bool needToLogin = !isCorrectLogin;
            
            if (needToLogout)
                HomeWindow.Home.LogOut(true, false, false);

            if (needToLogin)
            {
                var lw = LoginWindow.Instance;
                lw.OnLoginSuccess((Action)delegate
                {
                    UserDataManager.UserData.JoinChat(chatId);
                });
                lw.ProgramaticallyLogIn(username, password);
            }
            else
            {
                UserDataManager.UserData.JoinChat(chatId);
            }
        }
    }
}
