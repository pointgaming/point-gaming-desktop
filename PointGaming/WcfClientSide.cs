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
                    Console.WriteLine(e.StackTrace);
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

        public void LoginAndJoinLobby(string userId, string userName, string sessionId, string lobbyId)
        {
            Console.WriteLine("{0}, {1}, {2}, {3}", userId, userName, sessionId, lobbyId);
        }
    }
}
