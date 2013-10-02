using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PointGaming.Voice
{
    class VoipMessageLeaveRoom : IVoipMessage
    {
        public const byte MType = 2;
        public byte MessageType { get { return MType; } }

        public string RoomName;
        public string FromUserId;
        public bool IsSuccess;

        public bool Read(byte[] buffer, int position, int length)
        {
            if (!VoipSerialization.ReadRawHex(buffer, length, ref position, 12, out RoomName))
                return false;

            if (position >= length)
                return false;
            IsSuccess = buffer[position++] == 1;

            VoipSession.VoipDebug(string.Format("rx leave: roomname'{0}' issuccess '{1}'", RoomName, IsSuccess));

            return true;
        }

        public int Write(byte[] buffer, byte[] key)
        {
            var position = 0;
            VoipSerialization.WriteRawGuid(buffer, ref position, FromUserId);
            var iv = VoipCrypt.GenerateIv();
            VoipSerialization.WriteRawBytes(buffer, ref position, iv);

            var cryptoStart = position;
            var nonce = new byte[4];
            VoipCrypt.CryptoRNG.GetBytes(nonce);
            VoipSerialization.WriteRawBytes(buffer, ref position, nonce);
            VoipSerialization.WriteRawBytes(buffer, ref position, VoipCrypt.AntiDos);
            buffer[position++] = MessageType;
            VoipSerialization.WriteRawHex(buffer, ref position, RoomName);

            var suid = buffer.BytesToHex(0, 16);
            var skey = key.BytesToHex();
            var siv = iv.BytesToHex();
            var splain = buffer.BytesToHex(cryptoStart, position - cryptoStart);

            var encryptedData = VoipCrypt.Encrypt(key, iv, buffer, cryptoStart, position - cryptoStart);
            Buffer.BlockCopy(encryptedData, 0, buffer, cryptoStart, encryptedData.Length);
            position = cryptoStart + encryptedData.Length;

            var scrypt = buffer.BytesToHex(cryptoStart, position - cryptoStart);
            VoipSession.VoipDebug("tx leave: uid[{0}] key[{1}] iv[{2}] plain[{3}] crypt[{4}]", suid, skey, siv, splain, scrypt);

            return position;
        }
    }
}
