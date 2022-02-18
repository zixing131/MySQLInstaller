using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace MySQLInstaller
{
    public class AesHelper
    {
        /// <summary>
        /// AES加密 
        /// </summary>
        /// <param name="text">加密字符</param>
        /// <param name="password">加密的密码</param>
        /// <param name="iv">密钥</param>
        /// <returns></returns>
        public static string AESEncrypt(string text, string password = ComConst.aespwd, string iv = ComConst.aesiv)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();

            rijndaelCipher.Mode = CipherMode.CBC;

            rijndaelCipher.Padding = PaddingMode.PKCS7;

            rijndaelCipher.KeySize = 128;

            rijndaelCipher.BlockSize = 128;

            byte[] pwdBytes = System.Text.Encoding.UTF8.GetBytes(password);

            byte[] keyBytes = new byte[16];

            int len = pwdBytes.Length;

            if (len > keyBytes.Length) len = keyBytes.Length;

            System.Array.Copy(pwdBytes, keyBytes, len);

            rijndaelCipher.Key = keyBytes;


            byte[] ivBytes = System.Text.Encoding.UTF8.GetBytes(iv);
            rijndaelCipher.IV = ivBytes;

            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();

            byte[] plainText = Encoding.UTF8.GetBytes(text);

            byte[] cipherBytes = transform.TransformFinalBlock(plainText, 0, plainText.Length);

            return Convert.ToBase64String(cipherBytes);

        }
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="text"></param>
        /// <param name="password"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string AESDecrypt(string text, string password = ComConst.aespwd, string iv = ComConst.aesiv)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();

            rijndaelCipher.Mode = CipherMode.CBC;

            rijndaelCipher.Padding = PaddingMode.PKCS7;

            rijndaelCipher.KeySize = 128;

            rijndaelCipher.BlockSize = 128;

            byte[] encryptedData = Convert.FromBase64String(text);

            byte[] pwdBytes = System.Text.Encoding.UTF8.GetBytes(password);

            byte[] keyBytes = new byte[16];

            int len = pwdBytes.Length;

            if (len > keyBytes.Length) len = keyBytes.Length;

            System.Array.Copy(pwdBytes, keyBytes, len);

            rijndaelCipher.Key = keyBytes;

            byte[] ivBytes = System.Text.Encoding.UTF8.GetBytes(iv);
            rijndaelCipher.IV = ivBytes;

            ICryptoTransform transform = rijndaelCipher.CreateDecryptor();

            byte[] plainText = transform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

            return Encoding.UTF8.GetString(plainText);

        }

    }
}
