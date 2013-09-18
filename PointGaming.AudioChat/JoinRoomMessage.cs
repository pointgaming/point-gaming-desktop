using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PointGaming.AudioChat
{
    public class JoinRoomMessage : IChatMessage
    {
        public const byte MType = 1;
        public byte MessageType { get { return MType; } }

        public string RoomName;
        public string FromUserId;

        public bool Read(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
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

            Console.WriteLine("uid__: " + buffer.BytesToHex(0, 16));
            Console.WriteLine("key__: " + key.BytesToHex());
            Console.WriteLine("iv___: " + iv.BytesToHex());
            Console.WriteLine("plain: " + buffer.BytesToHex(cryptoStart, position - cryptoStart));

            var encryptedData = AesIO.AesEncrypt(key, iv, buffer, cryptoStart, position - cryptoStart);
            Buffer.BlockCopy(encryptedData, 0, buffer, cryptoStart, encryptedData.Length);
            position = cryptoStart + encryptedData.Length;

            Console.WriteLine("crypt: " + buffer.BytesToHex(cryptoStart, position - cryptoStart));

            return position;
        }
    }
}
