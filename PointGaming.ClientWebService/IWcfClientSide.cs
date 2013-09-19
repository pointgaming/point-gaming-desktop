using System.ServiceModel;

namespace PointGaming.ClientWebService
{
    public interface IWcfClientSide
    {
        [OperationContract(IsOneWay = true)]
        void LoginAndJoinLobby(string userId, string userName, string sessionId, string lobbyId);
    }
}
