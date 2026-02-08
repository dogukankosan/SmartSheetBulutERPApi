using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SmartSheetProject.Classes
{
    internal static class EncryptionHelper
    {
        private static readonly byte[] keyBytes = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
        private static readonly byte[] ivBytes = Encoding.UTF8.GetBytes("1234567890123456");
        internal static string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                return null;
            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = ivBytes;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    using (var encryptor = aes.CreateEncryptor())
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(plainBytes, 0, plainBytes.Length);
                            cs.FlushFinalBlock();
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                _ = TextLog.LogToSQLiteAsync($"Şifreleme hatası: {ex.Message}");
                return null;
            }
        }
        internal static string Decrypt(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
                return null;
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = ivBytes;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    using (var decryptor = aes.CreateDecryptor())
                    using (MemoryStream ms = new MemoryStream(cipherBytes))
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                _ = TextLog.LogToSQLiteAsync($"Şifre çözme hatası: {ex.Message}");
                return null;
            }
        }
        internal static bool IsEncrypted(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            if (text.StartsWith("Server=", StringComparison.OrdinalIgnoreCase))
                return false;
            try
            {
                Convert.FromBase64String(text);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}