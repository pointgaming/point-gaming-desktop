using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace PointGaming.ClientWebService
{

    public partial class WcfServerConnection : IDisposable
    {
        private ServiceHost _host;
        private WcfServerSide _service;

        public void Start()
        {
            _service = new WcfServerSide();
            _host = new ServiceHost(_service, new Uri("net.tcp://localhost:8085"));

            ServiceThrottlingBehavior throttle;
            throttle = _host.Description.Behaviors.Find<ServiceThrottlingBehavior>();
            if (throttle == null)
            {
                throttle = new ServiceThrottlingBehavior();
                throttle.MaxConcurrentCalls = 100;
                throttle.MaxConcurrentSessions = 100;
                _host.Description.Behaviors.Add(throttle);
            }

            try
            {
                _host.Open();
            }
            catch (Exception ex)
            {
                CWService.AppendConsoleLine(ex.Message);
                CWService.AppendConsoleLine(ex.StackTrace);
            }
            finally
            {
            }
        }

        public void Dispose()
        {
            if (_host == null)
                return;

            if (_host.State == CommunicationState.Opened)
                _host.Close();

            _host = null;
        }

        public void JoinLobby(string lobbyId, string userid, string username, string sessionid)
        {
            _service.InvokeClientOnActiveDesktopSession((Action<IWcfClientSide>)delegate(IWcfClientSide client)
            {
                client.LoginAndJoinLobby(userid, username, sessionid, lobbyId);
            });
        }

    }
}
