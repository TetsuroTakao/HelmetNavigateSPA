using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp
{

    public class Encryptor
    {
        //RijndaelManaged aes = new RijndaelManaged();
        Aes aes { get; set; }
        static string key = "sampleappcodekey";
        byte[] bufferPassword = null;
        public static string EncryptedString { get; set; }

        public Encryptor()
        {
            aes = AesCryptoServiceProvider.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            createAESKey();
        }

        public Encryptor(CipherMode mode = CipherMode.ECB)
        {
            //Aes aes = AesCryptoServiceProvider.Create();
            //default aes.BlockSize = 128;              // BlockSize = 16bytes
            //aes.KeySize = 128;                // KeySize = 16bytes default 256
            //default aes.Mode = System.Security.Cryptography.CipherMode.CBC;        // CBC mode
            aes.Mode = mode;
            //default aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            createAESKey();
        }

        void createAESKey()
        {
            //byte[] bufferKey = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] bufferKey = new byte[16];
            bufferPassword = Encoding.UTF8.GetBytes(key);
            aes.Key = bufferPassword.Concat(bufferKey).Take(16).ToArray();
        }

        public string Crypt(string text)
        {
            byte[] encrypted = null;
            byte[] text_bytes = Encoding.UTF8.GetBytes(text);
            if (aes.Key.Length == 0) createAESKey();
            byte[] Key = Convert.FromBase64String(Environment.GetEnvironmentVariable("EncKey"));
            byte[] IV = Convert.FromBase64String(Environment.GetEnvironmentVariable("EncIV"));
            aes.Key = Key;
            aes.IV = IV;
            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptStream.Write(text_bytes, 0, text_bytes.Length);
                        cryptStream.FlushFinalBlock();
                        encrypted = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }

        public string Decrypt(string encryptedText)
        {
            //aes.Padding = PaddingMode..;
            byte[] encrypted = System.Convert.FromBase64String(encryptedText);
            byte[] planeText = new byte[encrypted.Length];
            //aes.Mode = CipherMode.ECB;
            byte[] Key = Convert.FromBase64String(Environment.GetEnvironmentVariable("EncKey"));
            byte[] IV = Convert.FromBase64String(Environment.GetEnvironmentVariable("EncIV"));
            aes.Key = Key;
            aes.IV = IV;
            using (ICryptoTransform decryptor = aes.CreateDecryptor())
            {
                using (MemoryStream memoryStream = new MemoryStream(encrypted))
                {
                    using (CryptoStream cryptStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        cryptStream.Read(planeText, 0, planeText.Length);
                    }
                }
            }
            string result = Encoding.UTF8.GetString(planeText);
            return result.Replace("\0","");
        }

        public static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            EncryptedString = Convert.ToBase64String(encrypted);
            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        public static string DecryptStringFromBytes_Aes(string original)
        {
            byte[] cipherText = new byte[0];
            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;
            byte[] Key = Convert.FromBase64String(Environment.GetEnvironmentVariable("EncKey"));
            byte[] IV = Convert.FromBase64String(Environment.GetEnvironmentVariable("EncIV"));
            using (AesCryptoServiceProvider myAes = new AesCryptoServiceProvider() { KeySize = 128,Padding = PaddingMode.PKCS7 })
            {
                cipherText = Convert.FromBase64String(original);
                // Check arguments.
                if (cipherText == null || cipherText.Length <= 0)
                    throw new ArgumentNullException("cipherText");
                if (Key == null || Key.Length <= 0)
                    throw new ArgumentNullException("Key");
                if (IV == null || IV.Length <= 0)
                    throw new ArgumentNullException("IV");
                // Create an AesCryptoServiceProvider object
                // with the specified key and IV.
                myAes.Key = Key;
                myAes.IV = IV;
                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = myAes.CreateDecryptor(myAes.Key, myAes.IV);
                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}