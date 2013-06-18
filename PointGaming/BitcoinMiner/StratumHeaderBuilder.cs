using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.BitcoinMiner
{
    public class StratumHeaderBuilder
    {
        public readonly StratumJob Job;
        private readonly string ExtraNonce1;
        public string ExtraNonce2 { get; private set; }
        private byte[] _extraNonce2;

        private string _blockHeader;
        public string BlockHeader
        {
            get
            {
                BuildMerkleRoot();
                UpdateTime();
                BuildBlockHeader();
                return _blockHeader;
            }
        }
        public uint NTime { get; private set; }

        private string _merkleRoot;

        public StratumHeaderBuilder(StratumJob job, string extraNonce1, string extraNonce2)
        {
            Job = job;
            ExtraNonce1 = extraNonce1;
            ExtraNonce2 = extraNonce2;
            _extraNonce2 = extraNonce2.ParseHex();
            UpdateTime();
        }

        public void IncrementNonce2()
        {
            _merkleRoot = null;
            int index = _extraNonce2.Length - 1;
            while (index >= 0)
            {
                if (++_extraNonce2[index--] != 0)
                    break;
            }
            ExtraNonce2 = _extraNonce2.ToHexString().ToLower();
        }

        private byte[] CreateCoinbase()
        {
            var fullString = Job.CoinB1 + ExtraNonce1 + ExtraNonce2 + Job.CoinB2;
            var fullBytes = fullString.ParseHex();
            return fullBytes;
        }

        private void BuildMerkleRoot()
        {
            if (_merkleRoot != null)
                return;

            var coinbase = CreateCoinbase();
            var merkleRoot = Miner.ComputeSha256Hash(coinbase, 0, coinbase.Length).bytes;
            var merkleBranches = Job.MerkleBranch;

            foreach (var item in merkleBranches)
            {
                var cur = item.ParseHex();
                var buffer = Concat(merkleRoot, cur);
                merkleRoot = Miner.ComputeSha256Hash(buffer, 0, buffer.Length).bytes;
            }

            _merkleRoot = merkleRoot.ToHexString();
            _blockHeader = null;
        }

        private void BuildBlockHeader()
        {
            if (_blockHeader != null)
                return;

            var nTimeString = NTime.ToBytes().ToHexString();
            _blockHeader = Job.Version + Job.PrevHash + _merkleRoot + nTimeString + Job.NBits + "00000000";
        }

        private void UpdateTime()
        {
            var dTime = (uint)((DateTime.UtcNow - Job.ReceiveTime).TotalSeconds);
            var newTime = Job.NTime + dTime;
            if (NTime != newTime)
            {
                _blockHeader = null;
                NTime = newTime;
            }
        }

        private static byte[] Concat(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            Buffer.BlockCopy(a, 0, c, 0, a.Length);
            Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }
    }

}
