namespace PointGaming.Desktop.POCO
{
	public static class Persistence
	{
		private static FriendRequestRoot frcRootObject = new FriendRequestRoot();

        public static string AuthToken { get; set; }
        public static string loggedInUsername { get; set; }
        public static string loggedInUserId { get; set; }

		public static FriendRequestRoot friendRequests
		{
			get { return frcRootObject; }
			set { frcRootObject = value; }
		}
	}
}
