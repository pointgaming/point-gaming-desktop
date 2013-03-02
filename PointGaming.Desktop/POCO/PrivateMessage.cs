using System.Collections.Generic;

namespace PointGaming.Desktop.POCO
{
    public class PrivateMessageOut
    {
        public string _id { get; set; }
        public string message { get; set; }
    }
    public class PrivateMessageIn
    {
        public UserBase fromUser { get; set; }
        public string message { get; set; }
    }
    public class PrivateMessageSent
    {
        public UserBase toUser { get; set; }
        public string message { get; set; }
    }
}
