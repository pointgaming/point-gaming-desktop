using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming
{
	public class FriendRequestsCollection
	{
			public string _id { get; set; }
			public string friend_request_user_id { get; set; }
			public string user_id { get; set; }
	}

	public class FriendRequestsCollectionRootObject
	{
		public bool success { get; set; }
		public List<FriendRequestsCollection> friend_requests { get; set; }
	}
}
