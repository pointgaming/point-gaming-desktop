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
        public bool IsTeamOnly;
        public byte[] Audio;

        public bool Read(byte[] buffer, int position, int length)
        {
            if (!BufferIO.ReadRawHex(buffer, length, ref position, 12,  out RoomName))
                return false;

            if (!BufferIO.ReadRawGuid(buffer, length, ref position, true, out FromUserId))
                return false;

            if (position >= length)
                return false;
            IsTeamOnly = buffer[position++] == 1;

            if (!BufferIO.ReadRemainingRawBytes(buffer, length, ref position, out Audio))
                return false;

            Console.WriteLine("rx audio: rn " + RoomName + " fuid " + FromUserId + " to " + IsTeamOnly + " audio " + Audio.BytesToHex());

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

            buffer[position++] = (byte)(IsTeamOnly ? 1 : 0);
            var audioStart = position;
            BufferIO.WriteRawBytes(buffer, ref position, Audio);

            Console.WriteLine("Tx AudioMessage:");
            Console.WriteLine("uid__: " + buffer.BytesToHex(0, 16));
            Console.WriteLine("key__: " + key.BytesToHex());
            Console.WriteLine("iv___: " + iv.BytesToHex());
            Console.WriteLine("plnxa: " + buffer.BytesToHex(cryptoStart, audioStart - cryptoStart));
            Console.WriteLine("audio: " + buffer.BytesToHex(audioStart, position - audioStart));

            var encryptedData = AesIO.AesEncrypt(key, iv, buffer, cryptoStart, position - cryptoStart);
            Buffer.BlockCopy(encryptedData, 0, buffer, cryptoStart, encryptedData.Length);
            position = cryptoStart + encryptedData.Length;

            Console.WriteLine("crypt: " + buffer.BytesToHex(cryptoStart, position - cryptoStart));

            return position;
        }
    }
}
