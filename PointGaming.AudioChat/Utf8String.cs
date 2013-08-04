using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PointGaming.AudioChat
{
    public class Utf8String
    {
        public byte[] Buffer;
        public int Position;
        public int Length;

        public Utf8String(byte[] buffer, int position, int length)
        {
            Buffer = buffer;
            Position = position;
            Length = length;
        }

        public Utf8String DeepCopy()
        {
            byte[] buffer = new byte[Length];
            System.Buffer.BlockCopy(Buffer, Position, buffer, 0, Length);
            return new Utf8String(buffer, 0, Length);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Utf8String;
            if (other == null)
                return false;
            if (other.Length != Length)
                return false;
            int i = Position;
            int io = other.Position;
            int end = i + Length;
            while (i < end)
            {
                if (Buffer[i++] != other.Buffer[io++])
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            uint sum = (uint)Length;
            int i= Position;
            int end = i + Length;
            while (i < end)
                sum = (sum << 24) | (sum >> 8) | Buffer[i++];
            return (int)sum;
        }

        public override string ToString()
        {
            var str = System.Text.Encoding.UTF8.GetString(Buffer, Position, Length);
            return str;
        }
    }
}
