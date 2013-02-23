namespace PointGaming.Desktop.POCO
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
