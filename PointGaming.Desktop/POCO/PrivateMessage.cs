using System.Collections.Generic;

namespace PointGaming.Desktop.POCO
{
	public class PrivateMessage
	{
		public string user_id { get; set;}
		public string message { get; set; }
	}

    public class RoomMembership
    {
        public List<UserBase> membership { get; set; }
    }
}
