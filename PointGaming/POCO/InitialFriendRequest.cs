namespace PointGaming.POCO
{
	public class InitialFriendRequest
	{
		public string username { get; set; }
	}

	public class InitialFriendRequestRoot
	{
		public InitialFriendRequest friend_request { get; set; }
	}
}
