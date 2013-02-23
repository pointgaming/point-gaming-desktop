using System.Collections.Generic;

namespace PointGaming.Desktop.POCO
{
    public class FriendRequestResponse
    {
        public string _id { get; set; }
        public User user { get; set; }
        public User friend_request_user { get; set; }
    }

    public class FriendRequestsCollectionRootObject
	{
		public bool success { get; set; }
        public List<FriendRequestResponse> friend_requests { get; set; }
	}
}
