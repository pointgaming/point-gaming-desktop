namespace PointGaming.Desktop.POCO
{
	public class FriendInviteResponse
	{
		public string action { get; set; }
	}

	public class FriendInviteResponseRoot
	{
		public FriendInviteResponse friend_request { get; set; }
	}
}
