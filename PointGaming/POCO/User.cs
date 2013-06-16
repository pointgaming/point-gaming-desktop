using System.Collections.Generic;

namespace PointGaming.POCO
{
    public class UserWithStatus : UserBase
    {
        public string status { get; set; }
    }
    public class UserBase
	{
		public string _id { get; set; }
        public string username { get; set; }
        public string rank { get; set; }
        public int points { get; set; }
        public TeamBase team { get; set; }

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

        //public string status { get; set; }

        //public string email { get; set; }
        //public string first_name { get; set; }
        //public string last_name { get; set; }
		


      //  "_id": "4f1ddce7-a62f-4cd9-9593-1c6e160cd85b",
      //"avatar_content_type": null,
      //"avatar_file_name": null,
      //"avatar_file_size": null,
      //"avatar_updated_at": null,
      //"birth_date": null,
      //"country": null,
      //"email": "dev@pointgaming.net",
      //"first_name": "developer",
      //"last_name": "developer",
      //"phone": null,
      //"points": 0,
      //"profile_id": null,
      //"state": null,
      //"status": "offline",
      //"team": null,
      //"username": "developer",
      //"profile_url": "/users/4f1ddce7-a62f-4cd9-9593-1c6e160cd85b/profile",
      //"age": ""

	}

    public class TeamBase
    {
        public string _id { get; set; }
        public string name { get; set; }

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
