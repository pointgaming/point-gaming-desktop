using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Linq;
using System.IO;

namespace PointGaming.BitcoinMiner
{
    public sealed class UInt256 : IComparable<UInt256>, IComparable
    {
        public const int ByteCount = 32;
        public byte[] bytes { get; set; }

        public UInt256()
        {
            bytes = new byte[ByteCount];
        }

        public UInt256(UInt256 copyFrom)
            : this()
        {
            Buffer.BlockCopy(copyFrom.bytes, 0, bytes, 0, ByteCount);
        }

        public UInt256(byte[] data)
        {
            bytes = data;
        }
        public UInt256(string s)
        {
            bytes = new byte[ByteCount];
            s = s.Trim();
            if (s.StartsWith("0x")) s = s.Substring(2);
            s = s.TrimStart('0');
            List<byte> ba = new List<byte>(ByteCount * 2);
            for (int i = 0; i<s.Length;) {
                byte b1 = 0;
                char c;
                c = s[i++];
                if (!c.ToHex(ref b1))
                    break;
                ba.Add(b1);
            }
            ba.Reverse();
            var baCount = ByteCount * 2;
            while (ba.Count < baCount)
                ba.Add(0);

            int j = 0;
            for (int i = 0; i < ByteCount; i++)
            {
                int lo = ba[j++];
                int hi = ba[j++] << 4;
                byte b = (byte)(lo | hi);
                bytes[i] = b;
            }
        }
        public UInt256(BigInteger i)
        {
            bytes = new byte[ByteCount];
            CopyFrom(i.ToByteArray());
        }

        public uint getInt(int index)
        {
            int byteIndex = index * 4;
            if (byteIndex < 0 || byteIndex >= ByteCount)
                throw new IndexOutOfRangeException();
            var bytes = this.bytes;
            uint a = bytes[byteIndex++];
            uint b = bytes[byteIndex++];
            uint c = bytes[byteIndex++];
            uint d = bytes[byteIndex];
            b = b << 8;
            c = c << 16;
            d = d << 24;
            var result = a | b | c | d;
            return result;
        }
        
        public BigInteger ToBigInteger()
        {
            return new BigInteger(bytes);
        }

        public bool IsZero
        {
            get
            {
                foreach (byte b in bytes) if (b != 0) return false;
                return true;
            }
        }

        public void SetToZero()
        {
            Array.Clear(bytes, 0, UInt256.ByteCount);
        }

        public void CopyTo(byte[] data)
        {
            int count = ByteCount;
            int length = data.Length;
            if (length < count)
                count = length;
            Buffer.BlockCopy(bytes, 0, data, 0, count);
        }
        public void CopyFrom(byte[] data)
        {
            int count = ByteCount;
            int length = data.Length;
            if (length < count)
                count = length;
            Buffer.BlockCopy(data, 0, bytes, 0, count);
        }

        public override string ToString()
        {
            var count = ByteCount;
            var strings = new string[count];
            int j = count - 1;
            int i = 0;
            while (i < count)
                strings[i++] = string.Format("{0:x2}", bytes[j--]);
            return string.Concat(strings);
        }

        public string ToEString()
        {
            var s = ToString();
            var sHashTrim = s.TrimStart('0');
            int zeroCount = s.Length - sHashTrim.Length;
            if (sHashTrim.Length > 8)
                sHashTrim = sHashTrim.Substring(0, 8);
            string result = zeroCount.ToString("000") + ">>" + sHashTrim;
            return result;
        }


        public int CompareTo(UInt256 b)
        {
            UInt256 a = this;
            var abytes = a.bytes;
            var bbytes = b.bytes;
            var count = ByteCount;
            for (int i = count-1; i >=0; i--)
            {
                int cmp = abytes[i] - bbytes[i];
                if (cmp == 0)
                    continue;
                return cmp;
            }
            return 0;
        }

        int IComparable.CompareTo(object bb)
        {
            UInt256 b = (UInt256)bb;
            UInt256 a = this;

            var count = ByteCount;
            var auints = a.bytes;
            var buints = b.bytes;
            for (int i = count - 1; i >= 0; i--)
            {
                int cmp = auints[i] - buints[i];
                if (cmp != 0)
                    return cmp;
            }
                   
            return 0;
        }

        public static bool operator ==(UInt256 a, UInt256 b)
        {
            if (System.Object.ReferenceEquals(a, b)) return true;
            if (((object)a == null) || ((object)b == null)) return false;
            var count = ByteCount;
            var aBytes = a.bytes;
            var bBytes = b.bytes;
            for (int i = 0; i < count; i++) 
            {
                if (aBytes[i] != bBytes[i])
                    return false;
            }
            return true;
        }
        public static bool operator !=(UInt256 a, UInt256 b)
        {
            return !(a==b);
        }
        public override bool Equals(object obj)
        {
            return this == (UInt256)obj;
        }

        public override int GetHashCode()
        {
            var bs = bytes;
            uint a = bs[0];
            uint b = bs[1];
            uint c = bs[2];
            uint d = bs[3];
            b = b << 8;
            c = c << 16;
            d = d << 24;
            var sum = (int)(a | b | c | d);

            a = bs[4];
            b = bs[5];
            c = bs[6];
            d = bs[7];
            b = b << 8;
            c = c << 16;
            d = d << 24;
            sum ^= (int)(a | b | c | d);

            a = bs[8];
            b = bs[9];
            c = bs[10];
            d = bs[11];
            b = b << 8;
            c = c << 16;
            d = d << 24;
            sum ^= (int)(a | b | c | d);

            a = bs[12];
            b = bs[13];
            c = bs[14];
            d = bs[15];
            b = b << 8;
            c = c << 16;
            d = d << 24;
            sum ^= (int)(a | b | c | d);

            return sum;
        }

        public UInt256 Not()
        {
            UInt256 b = new UInt256();
            var bbytes = b.bytes;
            for (int i = ByteCount - 1; i >= 0; i--)
                bbytes[i] = (byte)(~(bytes[i]));
            return b;
        }
        public UInt256 ShiftRight(int shift)
        {
            if (shift < 0)
                throw new NotImplementedException();
            var remainder = shift & 0x7;
            if (remainder != 0)
                throw new NotImplementedException();

            UInt256 b = new UInt256();
            var auints = this.bytes;
            var buints = b.bytes;
            int bIndex = 0;
            int aIndex = shift >> 3;
            while (aIndex < ByteCount)
                buints[bIndex++] = auints[aIndex++];
                   
            return b;
        }

        public int FirstOne { get {
                int firstOne = 0;
                for (int i = ByteCount-1; i >= 0; i--)
                {
                    byte b = bytes[i];
                    if (b == 0)
                    {
                        firstOne += 8;
                        continue;
                    }
                    byte cmp = 0x80;
                    for (int j = 0; j < 8; j++)
                    {
                        if (b >= cmp) return firstOne;
                        cmp = (byte)(cmp >> 1);
                        firstOne++;
                    }

                }

                return 64;
        } }
    }
}