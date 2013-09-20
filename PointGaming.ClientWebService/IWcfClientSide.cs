using System.ServiceModel;

namespace PointGaming.ClientWebService
{
    public interface IWcfClientSide
    {
        [OperationContract(IsOneWay = true)]
        void LoginAndJoinChat(string username, string password, string chatId);
    }
}
