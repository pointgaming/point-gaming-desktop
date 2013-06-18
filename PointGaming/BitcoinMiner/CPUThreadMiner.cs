using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Numerics;
using System.ComponentModel;

namespace PointGaming.BitcoinMiner
{

    class CPUThreadMiner : Miner
    {
        public override string Name { get; protected set; }

        public CPUThreadMiner()
        {
            Name = "CPU Thread Miner";
        }

        public override Miner Copy()
        {
            return new CPUThreadMiner();
        }

        protected override List<uint> ScanHash_CryptoPP(MinerData md, UInt256 target)
        {
            uint nonce = (uint)md.nHashesDone;
            List<uint> results = new List<uint>(1);

            UInt256 hashResult = new UInt256();

            byte[] tmp = new byte[16 * 4];

            DateTime endLhutc = DateTime.UtcNow + new TimeSpan(0, 0, 1);
            long count = 0;
            while (true)
            {
                count++;   
                Single(md, hashResult, nonce, tmp);

                var lastInt = BitConverter.ToInt32(hashResult.bytes, 7*4);
                if (lastInt == 0 && hashResult.CompareTo(target) < 0)
                    results.Add(nonce);
                
                nonce++;

                if (DateTime.UtcNow >= endLhutc || nonce == 0)
                    break;
            }

            md.nHashesDone += count;
            HashedSome(count);

            return results;
        }

    }

}
