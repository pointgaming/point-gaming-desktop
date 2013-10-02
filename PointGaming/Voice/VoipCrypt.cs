using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace PointGaming.Voice
{
    class VoipCrypt
    {
        public static byte[] AntiDos = { 0x8E, 0xAA, 0xCF, 0x12 };

        public static bool AntiDosCheck(byte[] test)
        {
            if (test == null || test.Length != AntiDos.Length)
                return false;
            for (int i = 0; i < AntiDos.Length; i++)
                if (AntiDos[i] != test[i])
                    return false;
            return true;
        }

        public static RandomNumberGenerator CryptoRNG = RandomNumberGenerator.Create();

        public static readonly byte[] HardcodedKey = new byte[]
        {
            0x64, 0xFF, 0xBB, 0xF6,
            0xAF, 0x7C, 0xBA, 0x75,
            0x3F, 0x56, 0x79, 0xF3,
            0x56, 0x45, 0xDD, 0xCE,
        };

        public static readonly byte[] HardcodedIv = new byte[]
        {
            0x65, 0x99, 0x71, 0xDE,
            0xF1, 0xB9, 0xFA, 0x22,
            0x18, 0x56, 0x61, 0x2F,
            0x83, 0x71, 0x4E, 0xAD,
        };

        public static byte[] GenerateIv()
        {
            byte[] buffer = new byte[16];
            CryptoRNG.GetBytes(buffer);
            return buffer;
        }

        public static void Test()
        {
            try
            {
                var key = "4907ebd7d23548019a8dac24d25d9722";
                var iv =  "00000000000000000000000000000000";
                var plain = "b6a517de8eaacf120105726e616d65";
                var message = "b7cecdddfe70e38ce85f869d6b1fb5eadea93e81a17317bfa797175553591acc";

                message = Encrypt(key.HexToBytes(), iv.HexToBytes(), plain.HexToBytes(), 0, plain.Length>>1).BytesToHex();

                plain = Decrypt(key.HexToBytes(), iv.HexToBytes(), message.HexToBytes()).BytesToHex();

                string original = "Hello World.  This is a longer test.";
                var originalData = System.Text.Encoding.UTF8.GetBytes(original);

                byte[] encrypted = Encrypt(HardcodedKey, HardcodedIv, originalData, 0, original.Length);
                byte[] roundtripData = Decrypt(HardcodedKey, HardcodedIv, encrypted);
                var roundtrip = System.Text.Encoding.UTF8.GetString(roundtripData);

                var keyy = HardcodedKey.BytesToHex();
                var ivv = HardcodedIv.BytesToHex();

                VoipSession.VoipDebug("plainr: {0}", original);
                VoipSession.VoipDebug("plainx: {0}", originalData.BytesToHex());
                VoipSession.VoipDebug("cryptx: {0}", encrypted.BytesToHex());
                VoipSession.VoipDebug("plainx: {0}", roundtripData.BytesToHex());
                VoipSession.VoipDebug("plainr: {0}", roundtrip);

                var opensslRes = File.ReadAllBytes("C:\\OpenSSL-Win32\\bin\\test\\test.bin");
                var opensslRess = opensslRes.BytesToHex();


                VoipSession.VoipDebug("Equal:      {0}", original == roundtrip);
            }
            catch (Exception e)
            {
                VoipSession.VoipDebug("Error: {0}", e.Message);
            }
        }

        private static readonly byte[] _zeros = new byte[32];

        public static byte[] Encrypt(byte[] Key, byte[] IV, byte[] plainData, int offset, int length)
        {
            if (plainData == null || plainData.Length < offset + length)
                throw new ArgumentNullException("plainData");
            if (offset < 0)
                throw new ArgumentNullException("offset");
            if (length <= 0)
                throw new ArgumentNullException("length");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] encryptedData;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(plainData, offset, length);
                        if (aesAlg.Padding == PaddingMode.None)
                        {
                            var extra = length % 16;
                            if (extra != 0)
                                csEncrypt.Write(_zeros, 0, 16 - extra);
                        }
                        csEncrypt.FlushFinalBlock();
                        encryptedData = msEncrypt.ToArray();
                    }
                }
            }

            return encryptedData;
        }

        public static byte[] Decrypt(byte[] Key, byte[] IV, byte[] encryptedData, int offset = 0, int length = int.MinValue)
        {
            if (length == int.MinValue)
                length = encryptedData.Length - offset;
            if (encryptedData == null || encryptedData.Length < offset + length)
                throw new ArgumentNullException("encryptedData");
            if (offset < 0)
                throw new ArgumentNullException("offset");
            if (length <= 0)
                throw new ArgumentNullException("length");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] plainData = null;
            
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(encryptedData, offset, length))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        var plainMs = new MemoryStream();
                        var plainBuffer = new byte[16];
                        int readLength;
                        while ((readLength = csDecrypt.Read(plainBuffer, 0, plainBuffer.Length)) > 0)
                            plainMs.Write(plainBuffer, 0, readLength);
                        plainData = plainMs.ToArray();
                    }
                }
            }

            return plainData;
        }
    }
}
