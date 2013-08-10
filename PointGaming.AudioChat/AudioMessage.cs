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
        public string RoomName;
        public string FromUserId;
        public int MessageNumber;
        public byte[] Audio;

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

            if (!BufferIO.ReadInt(buffer, length, ref position, out MessageNumber))
                return false;

            if (!BufferIO.ReadRawBytes(buffer, length, ref position, out Audio))
                return false;

            return true;
        }

        public int Write(byte[] buffer, byte[] key)
        {
            var position = 0;
            BufferIO.WriteString(buffer, ref position, FromUserId);

            var cryptoStart = position;
            var nonce = new byte[4];
            AesIO.CryptoRNG.GetBytes(nonce);
            BufferIO.WriteRawBytes(buffer, ref position, nonce);
            BufferIO.WriteRawBytes(buffer, ref position, AesIO.AntiDoS);
            buffer[position++] = MessageType;
            BufferIO.WriteString(buffer, ref position, RoomName);

            BufferIO.WriteInt(buffer, ref position, MessageNumber);
            BufferIO.WriteRawBytes(buffer, ref position, Audio);

            var encryptedData = AesIO.AesEncrypt(key, AesIO.HardcodedIv, buffer, cryptoStart, position - cryptoStart);
            Buffer.BlockCopy(encryptedData, 0, buffer, cryptoStart, encryptedData.Length);
            position = cryptoStart + encryptedData.Length;

            return position;
        }
    }
}
