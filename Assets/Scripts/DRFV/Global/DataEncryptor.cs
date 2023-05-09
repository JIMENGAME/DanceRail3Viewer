using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DRFV.Global
{
    public static class DataEncryptor
    {
        private static string key = "DanceRail3Viewer";
        private static string iv = "PepoyoMyWife2333";
        public static byte[] Encode(string raw)
        {
            if (raw == null || raw.Length <= 0)
                throw new ArgumentNullException(nameof(raw));

            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.ASCII.GetBytes(key);
            aesAlg.IV = Encoding.ASCII.GetBytes(iv);
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using MemoryStream msEncrypt = new MemoryStream();
            using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt, Encoding.UTF8))
            {
                swEncrypt.Write(raw);
            }

            return msEncrypt.ToArray();
        }

        public static string Decode(byte[] data)
        {
            if (data.Length < 1) return "";
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.ASCII.GetBytes(key);
            aesAlg.IV = Encoding.ASCII.GetBytes(iv);
            ICryptoTransform descriptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using MemoryStream msDecrypt = new MemoryStream(data);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, descriptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8);

            return srDecrypt.ReadToEnd();
        }
    }
}