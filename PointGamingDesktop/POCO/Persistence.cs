using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming
{
	public static class Persistence
	{
		private static string _authToken = "";
		private static string _loggedInUsername = "";
		private static FriendRequestsCollectionRootObject frcRootObject = new FriendRequestsCollectionRootObject();

		public static string AuthToken
		{
			get { return _authToken; }
			set { _authToken = value; }
		}

		public static string loggedInUsername
		{
			get { return _loggedInUsername; }
			set { _loggedInUsername = value; }
		}

		public static FriendRequestsCollectionRootObject friendRequests
		{
			get { return frcRootObject; }
			set { frcRootObject = value; }
		}
	}
}
