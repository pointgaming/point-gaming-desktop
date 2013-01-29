using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming
{
	public static class AuthTokenStatic
	{
		private static string _globalVar = "";

		public static string GlobalVar
		{
			get { return _globalVar; }
			set { _globalVar = value; }
		}
	}
}
