using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.POCO
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

    public class GameRoomListPoco
    {
        public List<GameRoomPoco> game_rooms { get; set; }
    }

    public class GameRoomSinglePoco
    {
        public GameRoomPoco game_room { get; set; }
    }

    public class GameRoomPoco
    {
        public string _id { get; set; }
        public string match_id { get; set; }
        public string game_id { get; set; }
        public int position { get; set; }
        public bool takeover_position { get; set; }
        public bool is_advertising { get; set; }
        public bool is_locked { get; set; }
        public bool betting { get; set; }
        public string betting_type { get; set; }
        public string password { get; set; }
        public int member_count { get; set; }
        public int max_member_count { get; set; }
        public string description { get; set; }
        public bool is_team_bot_placed { get; set; }
        public UserBase owner { get; set; }
        public List<UserBase> members { get; set; }
        public List<MatchPoco> matches { get; set; }
        public List<UserBase> admins { get; set; }
    }
}
