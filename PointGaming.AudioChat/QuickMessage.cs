using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PointGaming.AudioChat
{
    public class QuickMessage : IChatMessage
    {
        private byte _messageType;
        public byte MessageType { get { return _messageType; } }
        public Utf8String RoomName = new Utf8String(null, 0, 0);
        public Utf8String FromUserId = new Utf8String(null, 0, 0);

        public bool Read(byte[] buffer, int length)
        {
            if (length <= 0)
                return false;
            _messageType = buffer[0];

            var position = 1;
            if (!BufferIO.ReadUtf8String(buffer, length, ref position, RoomName))
                return false;

            if (!BufferIO.ReadUtf8String(buffer, length, ref position, FromUserId))
                return false;

            return true;
        }

        public int Write(byte[] buffer)
        {
            buffer[0] = MessageType;
            var position = 1;
            BufferIO.WriteUtf8String(buffer, ref position, RoomName);
            BufferIO.WriteUtf8String(buffer, ref position, FromUserId);
            return position;
        }
    }
}
