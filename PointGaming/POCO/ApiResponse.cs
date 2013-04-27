using System.Collections.Generic;

namespace PointGaming.POCO
{
	public class ApiResponse
	{
		public bool success { get; set; }
		public string auth_token { get; set; }
        public string _id { get; set; }
		public string username { get; set; }
		public string message { get; set; }
        public List<string> errors { get; set; }
	}
}
