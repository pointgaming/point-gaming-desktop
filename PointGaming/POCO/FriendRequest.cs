using System.Collections.Generic;

namespace PointGaming.POCO
{
    public class FriendRequest
    {
        public string _id { get; set; }
        public UserBase from_user { get; set; }
        public UserBase to_user { get; set; }
    }

    public class FriendRequestRoot
	{
        public List<FriendRequest> friend_requests { get; set; }
	}

    public class FriendLobbyChange
    {
        public string game_id { get; set; }
        public string user_id { get; set; }
        public string status { get; set; }
    }
}
