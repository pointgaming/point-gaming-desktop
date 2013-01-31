using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming
{
	public static class AuthTokenStatic
	{
		private static string _globalVar = "";
		private static string _loggedInUsername = "";

		public static string GlobalVar
		{
			get { return _globalVar; }
			set { _globalVar = value; }
		}

		public static string loggedInUsername
		{
			get { return _loggedInUsername; }
			set { _loggedInUsername = value; }
		}
	}
}
