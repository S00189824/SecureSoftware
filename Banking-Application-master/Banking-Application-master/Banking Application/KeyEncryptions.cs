using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Banking_Application
{
    public static class KeyEncryptions
    {
        public static string decryptWithKey(string ciphertext, string keyName, string iv)
        {
            CngProvider keyStorageProvider = CngProvider.MicrosoftSoftwareKeyStorageProvider;
            if (!CngKey.Exists(keyName, keyStorageProvider))
            {
                throw new Exception("Error: key doesn't exist...");
            }
            Aes aes = new AesCng(keyName, keyStorageProvider);
            aes.IV = Encoding.UTF8.GetBytes(iv);

            var decryptor = aes.CreateDecryptor();
            byte[] ciphertextBytes = Convert.FromBase64String(ciphertext);
            byte[] plaintextBytes = decryptor.TransformFinalBlock(ciphertextBytes, 0, ciphertextBytes.Length);
            aes.Dispose();

            return Encoding.UTF8.GetString(plaintextBytes);
        }

        public static string encryptWithKey(string plaintext, string keyName, string iv)
        {
            CngProvider keyStorageProvider = CngProvider.MicrosoftSoftwareKeyStorageProvider;
            if (!CngKey.Exists(keyName, keyStorageProvider))
            {
                CngKeyCreationParameters keyCreationParameters = new CngKeyCreationParameters()
                {
                    Provider = keyStorageProvider
                };
                CngKey.Create(new CngAlgorithm("AES"), keyName, keyCreationParameters);
            }
            Aes aes = new AesCng(keyName, keyStorageProvider);
            aes.IV = Encoding.UTF8.GetBytes(iv);

            var encryptor = aes.CreateEncryptor();
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] ciphertextBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
            aes.Dispose();

            return Convert.ToBase64String(ciphertextBytes);
        }
    }
}
