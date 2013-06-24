using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PointGaming.POCO;
using RestSharp;

namespace PointGaming
{
    public class FriendshipManager
    {
        private SocketSession _pgSession { get; set; }

        public FriendshipManager(SocketSession session)
        {
            _pgSession = session;
        }

        public void RequestFriend(string username, Action<string> onCompleted)
        {
            var friendRequest = new InitialFriendRequest { username = username };
            var friendRequestRootObject = new InitialFriendRequestRoot { friend_request = friendRequest };

            var request = new RestRequest(Method.POST);
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddBody(friendRequestRootObject);

            var friendsRequestApiCall = _pgSession.GetWebApiV1Function("/friend_requests");
            var client = new RestClient(friendsRequestApiCall);

            _pgSession.Begin(delegate
            {
                var apiResponse = (RestResponse<ApiResponse>)client.Execute<ApiResponse>(request);

                if (!apiResponse.IsOk())
                {
                    onCompleted(apiResponse.Data.message);
                }
                else
                {
                    onCompleted(null);
                }
            });
        }
    }
}
