using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace DiscordStatusGUI
{
    class AES
    {
        static byte[] CreateKey(string key, int length)
        {
            if (string.IsNullOrEmpty(key))
            {
                //byte[] bytes = new byte[length];

                //var rndm = new Random();
                //rndm.NextBytes(bytes);

                //return bytes;
                return Encoding.UTF8.GetBytes(new string('\0', length));
            }
            else
            {
                List<byte> bytes = new List<byte>(Encoding.UTF8.GetBytes(key));

                while (bytes.Count < length)
                    bytes.AddRange(bytes);

                return bytes.GetRange(0, length).ToArray();
            }
        }

        public static bool TryEncryptString(string value, string key, out string encrypted)
        {
            try
            {
                encrypted = EncryptString(value, key);
                return true;
            }
            catch
            {
                encrypted = null;
                return false;
            }
        }

        public static bool TryDecryptString(string value, string key, out string decrypted)
        {
            try
            {
                decrypted = DecryptString(value, key);
                return true;
            }
            catch
            {
                decrypted = null;
                return false;
            }
        }

        public static string EncryptString(string value, string key)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(key))
                return null;

            List<byte> encrypted = new List<byte>();

            using (Aes myAes = Aes.Create())
            {
                encrypted.AddRange(EncryptStringToBytes(value, CreateKey(key, 32), myAes.IV));
                encrypted.AddRange(myAes.IV);
            }

            return Convert.ToBase64String(encrypted.ToArray());
        }

        public static string DecryptString(string value, string key)
        {
            if (string.IsNullOrEmpty(value) ||
                value.Length <= 16 || string.IsNullOrEmpty(key))
                return null;

            List<byte> bytes = new List<byte>(Convert.FromBase64String(value));
            byte[] IV = bytes.GetRange(bytes.Count - 16, 16).ToArray(),
                   Value = bytes.GetRange(0, bytes.Count - 16).ToArray();

            return DecryptStringFromBytes(Value, CreateKey(key, 32), IV);
        }

        public static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        swEncrypt.Write(plainText);

                    encrypted = msEncrypt.ToArray();
                }
            }

            return encrypted;
        }

        public static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    plaintext = srDecrypt.ReadToEnd();
            }

            return plaintext;
        }
    }
}
