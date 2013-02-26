namespace PointGaming.Desktop.POCO
{
	public static class Persistence
	{
		private static string _authToken = "";
		private static string _loggedInUsername = "";
		private static FriendInviteRoot frcRootObject = new FriendInviteRoot();

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

		public static FriendInviteRoot friendRequests
		{
			get { return frcRootObject; }
			set { frcRootObject = value; }
		}
	}
}
