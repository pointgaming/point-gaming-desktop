using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            WcfClientSide wcf = new WcfClientSide();
            wcf.Start();
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
