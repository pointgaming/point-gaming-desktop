using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming
{
	public class FriendRequest
	{
		public string username { get; set; }
	}

	public class FriendRequestRootObject
	{
		public FriendRequest friend_request { get; set; }
	}
}
