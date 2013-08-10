using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PointGaming.AudioChat
{
    public class LeaveRoomMessage : IChatMessage
    {
        public const byte MType = 2;
        public byte MessageType { get { return MType; } }

        public string AuthToken;
        public string RoomName;
        public string FromUserId;

        public bool Read(byte[] buffer, int length, byte[] key)
        {
            if (length <= 0)
                return false;
            if (buffer[0] != MessageType)
                return false;

            var position = 1;
            if (!BufferIO.ReadString(buffer, length, ref position, out AuthToken))
                return false;

            if (!BufferIO.ReadString(buffer, length, ref position, out RoomName))
                return false;

            if (!BufferIO.ReadString(buffer, length, ref position, out FromUserId))
                return false;

            return true;
        }

        public int Write(byte[] buffer, byte[] key)
        {
            buffer[0] = MessageType;
            var position = 1;
            BufferIO.WriteString(buffer, ref position, AuthToken);
            BufferIO.WriteString(buffer, ref position, RoomName);
            BufferIO.WriteString(buffer, ref position, FromUserId);
            return position;
        }
    }
}
