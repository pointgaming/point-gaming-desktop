using System.Collections.Generic;

namespace PointGaming.Desktop.POCO
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
}
