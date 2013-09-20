using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PointGaming.AudioChat
{
    public class LeaveRoomMessage : IVoiceMessage
    {
        public const byte MType = 2;
        public byte MessageType { get { return MType; } }

        public string RoomName;
        public string FromUserId;
        public bool IsSuccess;

        public bool Read(byte[] buffer, int position, int length)
        {
            if (!BufferIO.ReadRawHex(buffer, length, ref position, 12, out RoomName))
                return false;

            if (position >= length)
                return false;
            IsSuccess = buffer[position++] == 1;

            Console.WriteLine(string.Format("rx leave: roomname'{0}' issuccess '{1}'", RoomName, IsSuccess));

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
            BufferIO.WriteRawBytes(buffer, ref position, AesIO.AntiDos);
            buffer[position++] = MessageType;
            BufferIO.WriteRawHex(buffer, ref position, RoomName);

            var encryptedData = AesIO.AesEncrypt(key, iv, buffer, cryptoStart, position - cryptoStart);
            Buffer.BlockCopy(encryptedData, 0, buffer, cryptoStart, encryptedData.Length);
            position = cryptoStart + encryptedData.Length;

            return position;
        }
    }
}
