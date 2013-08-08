using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PointGaming.AudioChat
{
    public class BufferIO
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

        public static bool ReadUtf8String(byte[] buffer, int bufferLength, ref int position, Utf8String value)
        {
            int len;
            if (!ReadU8Length(buffer, bufferLength, ref position, out len))
                return false;

            if (position + len > bufferLength)
                return false;

            value.Buffer = buffer;
            value.Position = position;
            value.Length = len;
            position += len;

            return true;
        }

        public static void WriteUtf8String(byte[] buffer, ref int position, Utf8String value)
        {
            var len = value.Length;
            WriteU8Length(buffer, ref position, len);
            Buffer.BlockCopy(value.Buffer, value.Position, buffer, position, len);
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
    }

}
