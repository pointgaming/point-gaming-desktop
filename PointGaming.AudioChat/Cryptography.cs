using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace PointGaming.AudioChat
{
    public class AesIO
    {
        public static byte[] AntiDoS = { 0x8E, 0xAA, 0xCF, 0x12 };

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

        public static void Test()
        {
            try
            {
                string original = "This is a super long message.  lkasjdfj sdklf lksjadkjf slkfksdf lsdkl sdksdflkslkfk skldkl sldflk lsdfkj sllkjs klfksdj dkdkk kdksdkjfsdakdfjsd kfjksdjk sjk sksjsdjkjk fjk sdkf jsdkfj sdk fksdj jksdjkfjks dfjksdkfj";
                var originalData = System.Text.Encoding.UTF8.GetBytes(original);

                byte[] encrypted = AesEncrypt(HardcodedKey, HardcodedIv, originalData, 0, original.Length);
                byte[] roundtripData = AesDecrypt(HardcodedKey, HardcodedIv, encrypted);
                var roundtrip = System.Text.Encoding.UTF8.GetString(roundtripData);

                Console.WriteLine("Original:   {0}", original);
                Console.WriteLine("Round Trip: {0}", roundtrip);
                Console.WriteLine("Equal:      {0}", original == roundtrip);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
        }

        public static byte[] AesEncrypt(byte[] Key, byte[] IV, byte[] plainData, int offset, int length)
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
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(plainData, offset, length);
                        csEncrypt.FlushFinalBlock();
                        encryptedData = msEncrypt.ToArray();
                    }
                }
            }

            return encryptedData;
        }

        public static byte[] AesDecrypt(byte[] Key, byte[] IV, byte[] encryptedData)
        {
            if (encryptedData == null || encryptedData.Length <= 0)
                throw new ArgumentNullException("encryptedData");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] plainData = null;
            
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
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
