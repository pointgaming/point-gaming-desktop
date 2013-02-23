namespace PointGaming.Desktop.POCO
{
	public class User
	{
		public string _id { get; set; }
		public string username { get; set; }
	}

	public class UserRootObject
	{
		public User user { get; set; }
	}
}
