﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demo
{
	public class SuccessResponse
	{
		public bool success { get; set; }
		public string auth_token { get; set; }
		public string username { get; set; }
	}
}
