using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.Voice
{
    public static class VoipSerialization
    {
        public static bool ReadInt(byte[] buffer, int bufferLength, ref int position, out int value)
        {
            value = 0;

            if (position + 4 > bufferLength)
                return false;

            uint a, b, c, d;
            a = buffer[position++];
            b = buffer[position++];
            c = buffer[position++];
            d = buffer[position++];
            value = (int)(a | (b << 8) | (c << 16) | (d << 24));

            return true;
        }

        public static void WriteInt(byte[] buffer, ref int position, int value)
        {
            buffer[position++] = (byte)(value);
            buffer[position++] = (byte)(value >> 8);
            buffer[position++] = (byte)(value >> 16);
            buffer[position++] = (byte)(value >> 24);
        }


        public static bool ReadU8Length(byte[] buffer, int bufferLength, ref int position, out int value)
        {
            value = 0;

            if (position + 1 > bufferLength)
                return false;

            uint a;
            a = buffer[position++];
            value = (int)a;

            return true;
        }

        public static void WriteU8Length(byte[] buffer, ref int position, int value)
        {
            if (value > byte.MaxValue)
                throw new ArgumentException("value cannot be greater than 255");
            buffer[position++] = (byte)(value);
        }

        public static bool ReadString(byte[] buffer, int bufferLength, ref int position, out string value)
        {
            value = null;

            int len;
            if (!ReadU8Length(buffer, bufferLength, ref position, out len))
                return false;

            if (position + len > bufferLength)
                return false;

            value = System.Text.Encoding.UTF8.GetString(buffer, position, len);
            position += len;

            return true;
        }

        public static void WriteString(byte[] buffer, ref int position, string value)
        {
            byte[] valueBytes = System.Text.Encoding.UTF8.GetBytes(value);
            var len = valueBytes.Length;
            WriteU8Length(buffer, ref position, len);
            Buffer.BlockCopy(valueBytes, 0, buffer, position, len);
            position += len;
        }

        public static bool ReadBytes(byte[] buffer, int bufferLength, ref int position, out byte[] value)
        {
            value = null;

            int len;
            if (!ReadU8Length(buffer, bufferLength, ref position, out len))
                return false;

            if (position + len > bufferLength)
                return false;

            value = new byte[len];
            Buffer.BlockCopy(buffer, position, value, 0, len);
            position += len;

            return true;
        }

        public static void WriteBytes(byte[] buffer, ref int position, byte[] value)
        {
            var len = value.Length;
            WriteU8Length(buffer, ref position, len);
            Buffer.BlockCopy(value, 0, buffer, position, len);
            position += len;
        }

        public static bool ReadRawBytes(byte[] buffer, int bufferLength, ref int position, byte[] value)
        {
            int len = value.Length;
            if (position + len > bufferLength)
                return false;

            Buffer.BlockCopy(buffer, position, value, 0, len);
            position += len;
            return true;
        }

        public static bool ReadRemainingRawBytes(byte[] buffer, int bufferLength, ref int position, out byte[] value)
        {
            value = null;

            int len = bufferLength - position;
            value = new byte[len];
            Buffer.BlockCopy(buffer, position, value, 0, len);
            position += len;

            return true;
        }

        public static void WriteRawBytes(byte[] buffer, ref int position, byte[] value)
        {
            var len = value.Length;
            Buffer.BlockCopy(value, 0, buffer, position, len);
            position += len;
        }


        public static bool ReadRawGuid(byte[] buffer, int bufferLength, ref int position, bool shouldAddDashes, out string guid)
        {
            guid = null;

            int len = 16;
            if (position + len > bufferLength)
                return false;

            var temp = new byte[len];
            Buffer.BlockCopy(buffer, position, temp, 0, len);
            position += len;

            guid = temp.BytesToHex().HexToGuid(shouldAddDashes);

            return true;
        }

        public static bool ReadRawHex(byte[] buffer, int bufferLength, ref int position, int byteCount, out string hex)
        {
            hex = null;

            int len = byteCount;
            if (position + len > bufferLength)
                return false;

            var temp = new byte[len];
            Buffer.BlockCopy(buffer, position, temp, 0, len);
            position += len;

            hex = temp.BytesToHex();

            return true;
        }

        public static void WriteRawGuid(byte[] buffer, ref int position, string guid)
        {
            var temp = guid.GuidToHex().HexToBytes();
            var len = 16;
            Buffer.BlockCopy(temp, 0, buffer, position, len);
            position += len;
        }

        public static void WriteRawHex(byte[] buffer, ref int position, string hex)
        {
            var temp = hex.HexToBytes();
            var len = temp.Length;
            Buffer.BlockCopy(temp, 0, buffer, position, len);
            position += len;
        }

        public static string HexToGuid(this string hex, bool shouldAddDashes)
        {
            char[] result = new char[32 + (shouldAddDashes ? 4 : 0)];
            char[] hexChars = hex.ToCharArray();
            var hexIndex = 0;
            var resIndex = 0;
            Array.Copy(hexChars, hexIndex, result, resIndex, 8);
            hexIndex += 8;
            resIndex += 8;
            if (shouldAddDashes)
                result[resIndex++] = '-';
            Array.Copy(hexChars, hexIndex, result, resIndex, 4);
            hexIndex += 4;
            resIndex += 4;
            if (shouldAddDashes)
                result[resIndex++] = '-';
            Array.Copy(hexChars, hexIndex, result, resIndex, 4);
            hexIndex += 4;
            resIndex += 4;
            if (shouldAddDashes)
                result[resIndex++] = '-';
            Array.Copy(hexChars, hexIndex, result, resIndex, 4);
            hexIndex += 4;
            resIndex += 4;
            if (shouldAddDashes)
                result[resIndex++] = '-';
            Array.Copy(hexChars, hexIndex, result, resIndex, 12);
            return new string(result);
        }

        public static string GuidToHex(this string guid)
        {
            return guid.Replace("-", "");
        }


        public static byte[] HexToBytes(this string hex)
        {
            if ((hex.Length & 1) != 0)
                throw new Exception("The binary key cannot have an odd number of digits");

            var result = new byte[hex.Length >> 1];
            int i = 0;
            while (i < hex.Length)
            {
                var resultIndex = i >> 1;
                var hi = hex[i++].HexToLoByte();
                var lo = hex[i++].HexToLoByte();
                result[resultIndex] = (byte)(lo | (hi << 4));
            }
            return result;
        }

        public static int HexToLoByte(this char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        public static char LoByteToHex(this byte value)
        {
            int trans = value + ((value < 10) ? 48 : 87);
            return (char)trans;
        }

        public static string BytesToHex(this byte[] value, int offset = 0, int length = -1)
        {
            if (length == -1)
                length = value.Length - offset;

            char[] result = new char[length << 1];
            int i = 0;
            while (i < result.Length)
            {
                var valueIndex = offset + (i >> 1);
                var b = value[valueIndex];
                var hi = (byte)((b >> 4) & 0xf);
                var lo = (byte)(b & 0xf);
                result[i++] = hi.LoByteToHex();
                result[i++] = lo.LoByteToHex();
            }
            return new string(result);
        }
    }

}
