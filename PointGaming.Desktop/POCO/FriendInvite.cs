using System.Collections.Generic;

namespace PointGaming.Desktop.POCO
{
    public class FriendInvite
    {
        public string _id { get; set; }
        public string username { get; set; }
    }

    public class FriendInviteRoot
	{
		public bool success { get; set; }
        public List<FriendInvite> friend_requests { get; set; }
	}
}
