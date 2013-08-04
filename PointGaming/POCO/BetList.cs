using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.POCO
{

    public class BetPoco
    {
        public string _id { get; set; }

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

    public class BetListPoco
    {
        public List<BetPoco> bets { get; set; }
    }

    public class BetSinglePoco
    {
        public BetPoco bet { get; set; }
    }

    public class MatchPoco
    {
        public string _id { get; set; }

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
