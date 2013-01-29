using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming
{
	public class ApiResponse
	{
		public bool success { get; set; }
		public string auth_token { get; set; }
		public string username { get; set; }
		public string message { get; set; }

	}
}
