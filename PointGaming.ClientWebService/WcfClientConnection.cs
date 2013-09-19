using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Configuration;  
using System.Xml;


namespace PointGaming.ClientWebService
{
    public class WcfClientConnection
    {
        private InstanceContext _context;
        private IWcfServerSide _proxy;

        public WcfClientConnection(InstanceContext context)
        {
            _context = context;

            ClientSection clientSection = (ClientSection)System.Configuration.ConfigurationManager.GetSection("system.serviceModel/client");
            ChannelEndpointElement endpoint = clientSection.Endpoints[0];

            EndpointAddress endPointAddress = new EndpointAddress(endpoint.Address);
            NetTcpBinding tcpBinding = new NetTcpBinding(SecurityMode.Transport);

            _proxy = new DuplexChannelFactory<IWcfServerSide>(_context, "PointGamingEndPoint").CreateChannel();
        }

        public void RegisterClientWithServer()
        {
            var desktopSessionId = DesktopSessionHelper.GetCurrentProcessSessionId();
            _proxy.RegisterClientWithServer(desktopSessionId);
        }
    }
}
