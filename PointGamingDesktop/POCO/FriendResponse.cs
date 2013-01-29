using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming
{
	public class Friend
	{
		public string _id { get; set; }
		public string email { get; set; }
		public string first_name { get; set; }
		public string last_name { get; set; }
		public string username { get; set; }
	}

	public class FriendResponseRootObject
	{
		public bool success { get; set; }
		public List<Friend> friends { get; set; }
	}
}
