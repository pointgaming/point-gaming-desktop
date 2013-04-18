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
        public string game_id { get; set; }
        public int position { get; set; }
        public bool is_advertising { get; set; }
        public bool is_locked { get; set; }
        public string password { get; set; }
        public int member_count { get; set; }
        public int max_member_count { get; set; }
        public string description { get; set; }
        public UserBase owner { get; set; }
    }

    public class BetPoco
    {
        public string _id { get; set; }
        public string match_id { get; set; }
        public string outcome { get; set; }

        public decimal bookie_wager { get; set; }
        public string bookie_odds { get; set; }


        //public string bookie_id { get; set; }
        public UserBase bookie { get; set; }

        //public string better_id { get; set; }
        public UserBase better { get; set; }

        //public string created_at { get; set; }
        //public string updated_at { get; set; }
        
        public string loser_id { get; set; }
        public string loser_name { get; set; }
        public string loser_type { get; set; }

        public string winner_id { get; set; }
        public string winner_name { get; set; }
        public string winner_type { get; set; }
    }

    public class BetSinglePoco
    {
        public BetPoco bet { get; set; }
        public string bet_tooltip { get; set; }
    }

    public class MatchPoco
    {
        public string _id { get; set; }
        public bool betting { get; set; }
        public string map { get; set; }

        public string player_1_id { get; set; }
        public string player_1_type { get; set; }
        public string player_2_id { get; set; }
        public string player_2_type { get; set; }

        public string room_id { get; set; }
        public string room_type { get; set; }

        public string state { get; set; }
        public string winner_id { get; set; }
        public string winner_type { get; set; }
    }

    public class MatchSinglePoco
    {
        public MatchPoco match { get; set; }
        public string match_details { get; set; }
    }
}
