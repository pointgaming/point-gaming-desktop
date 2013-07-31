using System.Collections.Generic;

namespace PointGaming.POCO
{
    public class FriendStatus
    {
        public string _id { get; set; }
        public string username { get; set; }
        public string status { get; set; }
        public List<string> lobbies { get; set; }
    }
}
