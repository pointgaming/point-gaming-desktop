﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming
{

	public class Game
	{
		public string _id { get; set; }
		public string name { get; set; }
		public int player_count { get; set; }
	}

	public class GamesRootObject
	{
		public bool success { get; set; }
		public List<Game> games { get; set; }
	}
}
