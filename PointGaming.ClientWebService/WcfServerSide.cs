using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace PointGaming.ClientWebService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
    ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public class WcfServerSide : IWcfServerSide
    {
        private readonly List<RegisterListener> _registerListeners = new List<RegisterListener>();
        private readonly List<ClientInfo> _clients = new List<ClientInfo>();

        public void RegisterClientWithServer(uint desktopSessionId)
        {
            foreach (var item in _clients)
            {
                if (item.DesktopSessionId == desktopSessionId)
                {
                    _clients.Remove(item);
                    break;
                }
            }

            ClientInfo ci = new ClientInfo
            {
                DesktopSessionId = desktopSessionId,
                Client =  OperationContext.Current.GetCallbackChannel<IWcfClientSide>(),
            };

            _clients.Add(ci);

            NotifyRegister(ci);
        }

        public void InvokeClientOnActiveDesktopSession(Action<IWcfClientSide> asyncResult)
        {
            var timeoutTime = DateTime.Now + TimeSpan.FromSeconds(10);
            var desktopSessionId = DesktopSessionHelper.GetActiveSessionId();

            var removes = new List<ClientInfo>();
            foreach (var item in _clients)
            {
                if (item.DesktopSessionId == desktopSessionId)
                {
                    if (Invoke(asyncResult, item))
                        return;
                    removes.Add(item);
                }
            }
            foreach (var item in removes)
                _clients.Remove(item);
            
            // connected client not found, so launch new client
            RegisterListener rl = new RegisterListener
            {
                DesktopSessionId = desktopSessionId,
                AsyncResult = asyncResult,
                TimeoutTime = timeoutTime,
            };
            lock (_registerListeners)
            {
                _registerListeners.Add(rl);
            }
            var path = CWService.GetProgramFileInfo().FullName;
            DesktopSessionHelper.LaunchInActiveSession(path);
        }

        private void NotifyRegister(ClientInfo ci)
        {
            List<RegisterListener> listeners = new List<RegisterListener>();
            lock (_registerListeners)
            {
                foreach (var item in _registerListeners)
                    if (item.DesktopSessionId == ci.DesktopSessionId)
                        listeners.Add(item);

                foreach (var item in listeners)
                    _registerListeners.Remove(item);
            }
            var now = DateTime.Now;
            foreach (var item in listeners)
            {
                if (now < item.TimeoutTime)
                    Invoke(item.AsyncResult, ci);
            }
        }

        private class RegisterListener
        {
            public uint DesktopSessionId;
            public Action<IWcfClientSide> AsyncResult;
            public DateTime TimeoutTime;
        }

        private bool Invoke(Action<IWcfClientSide> asyncResult, ClientInfo ci)
        {
            var suc = false;
            try
            {
                asyncResult(ci.Client);
                suc = true;
            }
            catch (System.ServiceModel.CommunicationObjectAbortedException e)
            {
            }
            return suc;
        }

        private class ClientInfo
        {
            public uint DesktopSessionId;
            public IWcfClientSide Client;
        }
    }
}
