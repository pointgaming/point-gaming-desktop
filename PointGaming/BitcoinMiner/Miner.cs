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
    public class MinerCollection
    {
        public List<Miner> AvailableMiners = new List<Miner>();
        public System.Collections.ObjectModel.ObservableCollection<Miner> AddedMiners = new System.Collections.ObjectModel.ObservableCollection<Miner>();

        public MinerCollection()
        {
            AvailableMiners.Add(new CPUThreadMiner());
            AvailableMiners.AddRange(OpenCLMiner.GetAvailableMiners());
        }
    }

    public abstract class Miner
    {
        long hashCount;
        DateTime hashCountStart = DateTime.UtcNow;

        public bool IsMining { get; private set; }
        public bool IsStopRequested { get; set; }
        public ThreadPriority Priority { get; private set; }

        public abstract string Name { get; protected set; }
        public string Performance { get {
            string strStatus = Numerical.SIFormat(HashesPerSecond) + "hash/s";
            return strStatus;
        } }
        public double HashesPerSecond { get; private set; }
        protected abstract List<uint> ScanHash_CryptoPP(MinerData md, UInt256 target);

        public float FPSLimit { get; set; }
        public float UsageLimit { get; set; }
        public int UMLimit { get; set; }
        public float FPS { get; set; }
        public int UM { get; set; }
        public bool IsPaused { get; set; }

        public abstract Miner Copy();

        static void FormatHashBlocks(byte[] pbuffer, int len)
        {
            int blocks = 1 + ((len + 8) / 64);
            int end = 64 * blocks;
            for (int i = end - 1; i >= len; i--) pbuffer[i] = 0;
            pbuffer[len] = 0x80;
            uint bits = ((uint)len) * 8;
            pbuffer[end - 1] = (byte)(bits >> 0);
            pbuffer[end - 2] = (byte)(bits >> 8);
            pbuffer[end - 3] = (byte)(bits >> 16);
            pbuffer[end - 4] = (byte)(bits >> 24);
        }

        public static readonly uint[] SHA256InitState = { 0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19 };
        public static readonly byte[] SHA256InitStateBytes = new byte[8 * 4];
        static Miner()
        {
            Buffer.BlockCopy(SHA256InitState, 0, SHA256InitStateBytes, 0, SHA256InitStateBytes.Length);
        }
        
        public static void SHA256Transform(byte[] pstate, byte[] pinput, byte[] pinit)
        {
            Sha256Computer ctx = new Sha256Computer();
            Buffer.BlockCopy(pinit, 0, ctx._state, 0, 32);
            ctx.Process(pinput, 0, 64);
            Buffer.BlockCopy(ctx._state, 0, pstate, 0, 32);
        }

        protected class MinerData
        {
            public long nHashesDone = 0;
            
            public byte[] blockbuffer = new byte[32 * 4];
            public byte[] hash1buffer = new byte[16 * 4];
            public byte[] midstatebuffer = new byte[8 * 4];
            
            public MinerData()
            {
                FormatHashBlocks(blockbuffer, 80);// 80
            }

            public void PreCalc(string blockHeader)
            {
                var blockHeaderBytes = blockHeader.ParseHex();
                Buffer.BlockCopy(blockHeaderBytes, 0, blockbuffer, 0, 80);
                    
                // Precalc the first half of the first hash, which stays constant
                SHA256Transform(midstatebuffer, blockbuffer, SHA256InitStateBytes);
                                
                var myState = new uint[8];
                Buffer.BlockCopy(midstatebuffer, 0, myState, 0, 32);
            }
        }
        
        protected static void Single(MinerData md, UInt256 hashResult, uint nonce, byte[] tmp)
        {
            Buffer.BlockCopy(md.blockbuffer, 16 * 4, tmp, 0, 16 * 4);

            var nonceBytes = BitConverter.GetBytes(nonce);
            tmp[12] = nonceBytes[0];
            tmp[13] = nonceBytes[1];
            tmp[14] = nonceBytes[2];
            tmp[15] = nonceBytes[3];

            SHA256Transform(md.hash1buffer, tmp, md.midstatebuffer);
            md.hash1buffer.ByteReverse();
            FormatHashBlocks(md.hash1buffer, UInt256.ByteCount);// 32

            SHA256Transform(hashResult.bytes, md.hash1buffer, SHA256InitStateBytes);
            hashResult.bytes.ByteReverse();
        }


        public static UInt256 ComputeSha256Hash(byte[] vch, int offset, int count)
        {
            var sha256Computer = new Sha256Computer();
            UInt256 result = new UInt256();
            var data = result.bytes;
            sha256Computer.Compute(vch, offset, (uint)count, data);
            sha256Computer.Compute(data, data);
            return result;
        }

        System.Diagnostics.Stopwatch _myWatch = new System.Diagnostics.Stopwatch();
        private long _myCount = 0;
        
        protected void HashedSome(long count)
        {
            if (!_myWatch.IsRunning)
                _myWatch.Start();
            else
                _myCount += count;

            var dTime = _myWatch.Elapsed;
            if (dTime.TotalSeconds >= 10)
            {
                HashesPerSecond = _myCount / dTime.TotalSeconds;
                _myWatch.Restart();
                _myCount = 0;
                App.LogLine(Performance);
            }
        }

        private StratumSession _stratumSession;

        internal void BeginMining(StratumSession stratum)
        {
            _stratumSession = stratum;
            var t = new Thread(Mine);
            t.IsBackground = true;
            t.Name = "OpenCL Miner";
            t.Start();
        }

        public static bool IsUserIdle
        {
            get
            {
                var dTime = App.UserIdleTimespan;
                var isIdle = dTime.TotalMinutes > UserDataManager.UserData.Settings.UserIdleMinutes;
                return isIdle;
            }
        }

        private void Mine()
        {
            MinerData md = new MinerData();

            StratumHeaderBuilder latestWork = null;

            while (!IsStopRequested)
            {
                if ((!IsUserIdle && UserDataManager.UserData.Settings.BitcoinMinerOnlyWheUserIdle) || IsPaused)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                if (md.nHashesDone > uint.MaxValue)
                {
                    latestWork.IncrementNonce2();
                    md.nHashesDone = 0;
                }

                latestWork = _stratumSession.GetLatestBuilder(latestWork);
                if (latestWork == null)
                {
                    Thread.Sleep(100);
                    continue;
                }

                md.PreCalc(latestWork.BlockHeader);

                List<uint> results = ScanHash_CryptoPP(md, latestWork.Job.Target);

                foreach (var nonce in results)
                {
                    var nonceBytes = nonce.ToBytes();
                    Buffer.BlockCopy(nonceBytes, 0, md.blockbuffer, 76, 4);

                    var hash = ComputeSha256Hash(md.blockbuffer, 0, 80);

                    if (hash.CompareTo(latestWork.Job.Target) <= 0)
                    {
                        Array.Reverse(nonceBytes);
                        var nonceHex2 = nonceBytes.ToHexString().ToLower();
                        _stratumSession.SubmitShare(latestWork, nonceHex2);
                    }
                }
            }
        }
    }
    
}
