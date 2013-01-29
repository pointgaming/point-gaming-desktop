using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.POCO
{
	public class CoinResponse
	{
		public string _id { get; set; }
		public float number_of_coins { get; set; }
		public string user_id { get; set; }
		public string wallet_id { get; set; }
	}

	public class RootObject
	{
		public bool success { get; set; }
		public List<Coin> coins { get; set; }
	}
}
