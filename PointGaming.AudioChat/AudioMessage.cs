using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PointGaming.AudioChat
{
    public class AudioMessage : IChatMessage
    {
        public const byte MType = 3;
        public byte MessageType { get { return MType; } }
        public string AuthToken;
        public string RoomName;
        public string FromUserId;
        public int MessageNumber;
        public byte[] Audio;

        public bool Read(byte[] buffer, int length)
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

            if (!BufferIO.ReadInt(buffer, length, ref position, out MessageNumber))
                return false;

            if (!BufferIO.ReadBytes(buffer, length, ref position, out Audio))
                return false;

            return true;
        }

        public int Write(byte[] buffer)
        {
            buffer[0] = MessageType;
            var position = 1;
            BufferIO.WriteString(buffer, ref position, AuthToken);
            BufferIO.WriteString(buffer, ref position, RoomName);
            BufferIO.WriteString(buffer, ref position, FromUserId);
            BufferIO.WriteInt(buffer, ref position, MessageNumber);
            BufferIO.WriteBytes(buffer, ref position, Audio);
            return position;
        }
    }
}
