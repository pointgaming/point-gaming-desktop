using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demo
{
	public class UserLogin
	{
		public string username { get; set; }
		public string password { get; set; }
	}

	public class RootLoginObject
	{
		public UserLogin user_login { get; set; }
	}
}
