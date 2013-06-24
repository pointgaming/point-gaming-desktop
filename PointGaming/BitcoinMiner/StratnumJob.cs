using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.BitcoinMiner
{
    public class StratumJob
    {
        public string JobId;
        public string PrevHash;
        public string CoinB1;
        public string CoinB2;
        public string[] MerkleBranch;
        public string Version;
        /// <summary>
        /// network difficulty
        /// </summary>
        public string NBits;
        /// <summary>
        /// network time
        /// </summary>
        public uint NTime;
        public DateTime ReceiveTime;
        public UInt256 Target;
    }
}
