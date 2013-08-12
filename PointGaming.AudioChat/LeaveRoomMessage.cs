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

        public string RoomName;
        public string FromUserId;

        public bool Read(byte[] buffer, int length, byte[] key)
        {
            if (length <= 0)
                return false;
            if (buffer[0] != MessageType)
                return false;

            var position = 1;
            if (!BufferIO.ReadString(buffer, length, ref position, out RoomName))
                return false;

            if (!BufferIO.ReadString(buffer, length, ref position, out FromUserId))
                return false;

            return true;
        }

        public int Write(byte[] buffer, byte[] key)
        {
            var position = 0;
            BufferIO.WriteRawGuid(buffer, ref position, FromUserId);
            var iv = AesIO.GenerateIv();
            BufferIO.WriteRawBytes(buffer, ref position, iv);

            var cryptoStart = position;
            var nonce = new byte[4];
            AesIO.CryptoRNG.GetBytes(nonce);
            BufferIO.WriteRawBytes(buffer, ref position, nonce);
            BufferIO.WriteRawBytes(buffer, ref position, AesIO.AntiDoS);
            buffer[position++] = MessageType;
            BufferIO.WriteString(buffer, ref position, RoomName);

            var encryptedData = AesIO.AesEncrypt(key, iv, buffer, cryptoStart, position - cryptoStart);
            Buffer.BlockCopy(encryptedData, 0, buffer, cryptoStart, encryptedData.Length);
            position = cryptoStart + encryptedData.Length;

            return position;
        }
    }
}
