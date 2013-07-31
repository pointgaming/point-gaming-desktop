using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.POCO
{
    public class FriendList
    {
        public List<UserWithLobbies> friends { get; set; }
    }

    public class UserWithLobbies : UserWithStatus
    {
        public List<string> lobbies { get; set; }
    }
}
