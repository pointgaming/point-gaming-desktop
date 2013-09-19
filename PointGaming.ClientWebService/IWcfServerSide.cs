using System.ServiceModel;

namespace PointGaming.ClientWebService
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IWcfClientSide))]
    public interface IWcfServerSide
    {
        [OperationContract(IsOneWay = true)]
        void RegisterClientWithServer(uint desktopSessionId);
    }
}
