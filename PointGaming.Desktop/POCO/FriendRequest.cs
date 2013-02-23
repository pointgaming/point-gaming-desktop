namespace PointGaming.Desktop.POCO
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
