using System;
using System.Collections.Generic;

namespace PointGaming.POCO
{
    public class UserFullResponse
    {
        public UserFull user { get; set; }
    }

    public class UserFull : UserWithStatus
    {
        public bool admin { get; set; }
        public string avatar_content_type { get; set; }
        public string avatar_file_name { get; set; }
        public int avatar_file_size { get; set; }
        public DateTime avatar_updated_at { get; set; }
        public string birth_date { get; set; }
        public string cash { get; set; }
        public string country { get; set; }
        public DateTime created_at { get; set; }
        public int dispute_lost_count { get; set; }
        public int dispute_won_count { get; set; }
        public string email { get; set; }
        public int finalized_bets_count { get; set; }
        public string first_name { get; set; }
        public int friend_count { get; set; }
        public string game_id { get; set; }
        public string group_id { get; set; }
        public string last_name { get; set; }
        public int match_dispute_lost_count { get; set; }
        public int match_dispute_won_count { get; set; }
        public int match_participation_count { get; set; }
        public string phone { get; set; }
        public int points { get; set; }
        public string profile_id { get; set; }
        public string reputation { get; set; }
        public string slug { get; set; }
        public string state { get; set; }
        public int stream_owner_count { get; set; }
        public string stripe_customer_token { get; set; }
        public TeamFull team { get; set; }

        public string team_id { get; set; }
        public string time_zone { get; set; }
        public DateTime updated_at { get; set; }

        public string profile_url { get; set; }
        public string age { get; set; }
        public string avatar { get; set; }
    }
    public class TeamFull : TeamBase
    {
        public DateTime created_at { get; set; }
        //"game_points":{"":25},
        public string logo_content_type { get; set; }
        public string logo_file_name { get; set; }
        public int logo_file_size { get; set; }
        public DateTime logo_updated_at { get; set; }
        public int member_count { get; set; }
        public int points { get; set; }
        public string slug { get; set; }
        public string tag { get; set; }
        public DateTime updated_at { get; set; }
        public bool temporarily { get; set; }
    }


    public class UserWithStatus : UserBase
    {
        public string status { get; set; }
    }

    public class UserBase
	{
		public string _id { get; set; }
        public string username { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as UserBase;
            if (other == null)
                return false;
            return _id == other._id;
        }
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
        public override string ToString()
        {
            return username;
        }
	}

    public class TeamBase
    {
        public string _id { get; set; }
        public string name { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as TeamBase;
            if (other == null)
                return false;
            return _id == other._id;
        }
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
        public override string ToString()
        {
            return name;
        }
    }


    public class BetOperandPoco
    {
        public string _id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
    }
}
