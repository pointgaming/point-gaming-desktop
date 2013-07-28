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
        public bool betting{ get; set; }
        public string password { get; set; }
        public int member_count { get; set; }
        public int max_member_count { get; set; }
        public string description { get; set; }
        public UserBase owner { get; set; }
        public List<UserBase> members { get; set; }
    }

    public class BetPoco
    {
        public string _id { get; set; }
        //public string created_at { get; set; }
        //public string updated_at { get; set; }
        
        public string match_id { get; set; }
        public string match_hash { get; set; }
        public string outcome { get; set; }

        public string offerer_id { get; set; }
        public string offerer_username { get; set; }
        public decimal offerer_wager { get; set; }
        public string offerer_odds { get; set; }
        public string offerer_choice_id { get; set; }
        public string offerer_choice_name { get; set; }
        public string offerer_choice_type { get; set; }

        public string taker_id { get; set; }
        public string taker_username { get; set; }
        public decimal taker_wager { get; set; }
        public string taker_odds { get; set; }
        public string taker_choice_id { get; set; }
        public string taker_choice_name { get; set; }
        public string taker_choice_type { get; set; }
    }

    public class BetSinglePoco
    {
        public BetPoco bet { get; set; }
    }

    public class MatchPoco
    {
        public string _id { get; set; }
        //public string created_at { get; set; }
        //public string updated_at { get; set; }
        
        public string match_hash { get; set; }
        public bool betting { get; set; }
        public string map { get; set; }
        public string game_id { get; set; }

        public string player_1_id { get; set; }
        public string player_1_type { get; set; }
        public string player_1_name { get; set; }
        public string player_2_id { get; set; }
        public string player_2_type { get; set; }
        public string player_2_name { get; set; }

        public string room_id { get; set; }
        public string room_type { get; set; }

        public string state { get; set; }
        public string winner_id { get; set; }
        public string winner_type { get; set; }
    }

    public class MatchSinglePoco
    {
        public MatchPoco match { get; set; }
    }
    public class MatchAndBetsPoco
    {
        public MatchPoco match { get; set; }
        public List<BetPoco> bets { get; set; }
    }
}
