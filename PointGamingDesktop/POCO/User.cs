using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming
{
	public class User
	{
		public string username { get; set; }
	}

	public class UserRootObject
	{
		public User user { get; set; }
	}
}
