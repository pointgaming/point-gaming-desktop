using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.Desktop.POCO
{
    public class GameList
    {
        public List<GamePoco> games { get; set; }
    }

    public class GamePoco
    {
        public string _id { get; set; }
        public string name { get; set; }
        public int player_count { get; set; }
    }
}
