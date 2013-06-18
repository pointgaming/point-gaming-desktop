using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Numerics;

namespace PointGaming.BitcoinMiner
{
    public static class Numerical
    {
        public static string SIFormat(this double value)
        {
            string[] posts = new string[] { "p", "n", "u", "m", "", "k", "M", "G", "T" };
            int postOffset = 4;
            while (value < 00.001 && postOffset > 0)
            { value *= 1000.0; postOffset--; }
            while (value > 1000.0 && postOffset < posts.Length)
            { value /= 1000.0; postOffset++; }
            string post = "?";
            int preZeros = 0;
            if (postOffset < posts.Length) post = posts[postOffset];

            double dispt = value;
            while (dispt > 1) { dispt /= 10.0; preZeros++; }

            int postZeros = 4 - preZeros;
            char[] strPostZeros = new char[postZeros];
            for (int i = 0; i < postZeros; i++) strPostZeros[i] = '0';
            return string.Format("{0:0." + new String(strPostZeros) + "}" + post, value);
        }

        public static uint ByteReverse(this uint x)
        {
            x = ((x & 0xFF00FF00u) >> 8) | ((x & 0x00FF00FFu) << 8);
            return (x << 16) | (x >> 16);
        }

        public static void ByteReverse(this byte[] x)
        {
            int i = 0;
            while (i < x.Length)
            {
                int j, k, l, m;
                j = i++;
                k = i++;
                l = i++;
                m = i++;
                byte a = x[j];
                byte b = x[k];
                byte c = x[l];
                byte d = x[m];
                x[j] = d;
                x[k] = c;
                x[l] = b;
                x[m] = a;
            }
        }

        public static bool IsEqual(this IList<byte> src, byte[] cmp, int srcOffset, int length)
        {
            for (int i = 0; i < length; i++)
                if (src[srcOffset + i] != cmp[i])
                    return false;
            return true;
        }

        public static void Randomize<T>(this List<T> list)
        {
            var count = list.Count;
            for (int i = 0; i < count; i++)
            {
                int j = Numerical.RandomWeakNext(count);
                if (i == j) continue;
                T t = list[i];
                list[i] = list[j];
                list[j] = t;
            }
        }

        public static void Resize<T>(this List<T> t, int size)
        {
            if (t.Count > size) t.RemoveRange(size, t.Count - size);
            else while (t.Count < size)
                {
                    T def = Activator.CreateInstance<T>();
                    t.Add(def);
                }
        }


        public static RNGCryptoServiceProvider RandomStrong = new RNGCryptoServiceProvider();
        public static int RandomStrongNext(int lessThan)
        {
            byte[] seed = new byte[4];
            RandomStrong.GetBytes(seed);
            return BitConverter.ToInt32(seed, 0) % lessThan;
        }
        private static object _randomWeakSynch = new object();
        private static Random _RandomWeak;
        public static int RandomWeakInt { get { lock (_randomWeakSynch) { return _RandomWeak.Next(); } } }
        public static double RandomWeakDouble { get { lock (_randomWeakSynch) { return _RandomWeak.NextDouble(); } } }
        public static int RandomWeakNext(int lessThan)
        {
            return RandomWeakInt % lessThan;
        }
        public static void RandomWeakNextBytes(this byte[] buffer)
        {
            lock (_randomWeakSynch) { _RandomWeak.NextBytes(buffer); }
        }
        static Numerical()
        {
            byte[] seed = new byte[4];
            RandomStrong.GetBytes(seed);
            _RandomWeak = new Random(BitConverter.ToInt32(seed, 0));
        }


        private static bool ishex(char cur, char lo, char hi, byte offset, ref byte hex)
        {
            bool inrange = lo <= cur && cur <= hi;
            if (inrange) hex = (byte)(cur - lo + offset);
            return inrange;
        }

        public static readonly char[] SpaceChars = new char[] { ' ', '\t', '\n', '\v', '\f', '\r', };
        public static bool IsSpace(this char c) { return SpaceChars.Contains(c); }

        public static byte[] ParseHex(this string psz)
        {
            // convert hex dump to vector
            List<byte> vch = new List<byte>();
            byte last = 0;
            bool isFirst = true;
            foreach (char c in psz)
            {
                if (IsSpace(c))
                    continue;
                byte now = 0;
                if (!(ToHex(c, ref now)))
                    break;

                if (isFirst)
                    last = now;
                else 
                    vch.Add((byte)((last << 4) | now));
                isFirst = !isFirst;
            }
            return vch.ToArray();
        }

        public static bool ToHex(this char c, ref byte now)
        {
            return
                ishex(c, '0', '9', 0, ref now) ||
                ishex(c, 'A', 'F', 10, ref now) ||
                ishex(c, 'a', 'f', 10, ref now);
        }

        public static string ToHexString(this IList<byte> vch, bool fSpaces = false)
        {
            string fm = fSpaces ? "{0:x2} " : "{0:x2}";
            System.Text.StringBuilder sb = new System.Text.StringBuilder(vch.Count * (fSpaces ? 3 : 2));
            foreach (byte b in vch)
            {
                sb.AppendFormat(fm, b);
            }
            return sb.ToString();
        }

        public static void PrintHex(List<byte> vch, string pszFormat = "{0}", bool fSpaces = true)
        {
            Console.Write(string.Format(pszFormat, ToHexString(vch, fSpaces)));
        }

        public static byte[] ToBytes(this uint number)
        {
            byte[] nonceBytes = new byte[] { (byte)number, (byte)(number >> 8), (byte)(number >> 16), (byte)(number >> 24), };
            return nonceBytes;
        }
    }
}
