using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming
{
	public class FriendPutRequest
	{
		public string action { get; set; }
	}

	public class FriendPutRootObject
	{
		public FriendPutRequest friend_request { get; set; }
	}
}
