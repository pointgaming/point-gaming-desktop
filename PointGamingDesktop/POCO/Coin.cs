using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.POCO
{
	public class Coin
	{
		public string wallet_id { get; set; }
		public float number_of_coins { get; set; }
	}

	public class RootObject
	{
		public Coin coin { get; set; }
	}
}
