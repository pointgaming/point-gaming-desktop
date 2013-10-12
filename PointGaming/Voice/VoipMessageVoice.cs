using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PointGaming.Voice
{
    class DateTimePrecise
    {
        private static long _startTick;
        private static Stopwatch _stopWatch;

        static DateTimePrecise()
        {
            _startTick = DateTime.UtcNow.Ticks;
            _stopWatch = Stopwatch.StartNew();
        }

        public static long UtcNowTicks
        {
            get
            {
                return _startTick + _stopWatch.ElapsedTicks;
            }
        }

        public static DateTime UtcNow
        {
            get
            {
                return new DateTime(_startTick + _stopWatch.ElapsedTicks);
            }
        }
    }


    class VoipMessageVoice : IVoipMessage
    {
        public const byte MType = 3;
        public byte MessageType { get { return MType; } }
        public string RoomName;
        public string FromUserId;
        public bool IsTeamOnly;
        public byte[] Audio;
        public DateTime RxTime;
        public DateTime TxTime;
        
        public bool Read(byte[] buffer, int position, int length)
        {
            RxTime = DateTimePrecise.UtcNow;
            var rxkTicks = (uint)(RxTime.Ticks / 1000);

            if (!VoipSerialization.ReadRawHex(buffer, length, ref position, 12,  out RoomName))
                return false;

            if (!VoipSerialization.ReadRawGuid(buffer, length, ref position, true, out FromUserId))
                return false;

            if (position >= length)
                return false;
            IsTeamOnly = buffer[position++] == 1;

            uint txkTicks;
            if (!VoipSerialization.ReadUInt(buffer, length, ref position, out txkTicks))
                return false;
            var dTime = (int)(txkTicks - rxkTicks);
            TxTime = RxTime.AddTicks(1000L * (long)dTime);
            
            if (!VoipSerialization.ReadRemainingRawBytes(buffer, length, ref position, out Audio))
                return false;

            VoipSession.VoipDebug("rx audio: rn " + RoomName + " fuid " + FromUserId + " @ " + TxTime + " to " + IsTeamOnly + " audio " + Audio.BytesToHex());

            return true;
        }

        public int Write(byte[] buffer, byte[] key)
        {
            TxTime = DateTimePrecise.UtcNow;
            var txkTicks = (uint)(TxTime.Ticks / 1000);

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

            buffer[position++] = (byte)(IsTeamOnly ? 1 : 0);
            VoipSerialization.WriteUInt(buffer, ref position, txkTicks);
            var audioStart = position;
            VoipSerialization.WriteRawBytes(buffer, ref position, Audio);

            var suid = buffer.BytesToHex(0, 16);
            var skey = key.BytesToHex();
            var siv = iv.BytesToHex();
            var splainxa = buffer.BytesToHex(cryptoStart, audioStart - cryptoStart);
            var saudio = buffer.BytesToHex(audioStart, position - audioStart);
            
            var encryptedData = VoipCrypt.Encrypt(key, iv, buffer, cryptoStart, position - cryptoStart);
            Buffer.BlockCopy(encryptedData, 0, buffer, cryptoStart, encryptedData.Length);
            position = cryptoStart + encryptedData.Length;

            var scrypt = buffer.BytesToHex(cryptoStart, position - cryptoStart);
            VoipSession.VoipDebug("tx audio: uid[{0}] key[{1}] iv[{2}] plainxa[{3}] audio[{4}] crypt[{5}]", suid, skey, siv, splainxa, saudio, scrypt);

            return position;
        }
    }
}
