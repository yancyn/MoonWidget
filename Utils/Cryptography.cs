using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Plexus.Utils
{
    /// <summary>
    /// Summary description for Cryptography.
    /// </summary>
    /// <remarks>
    /// <b>Author</b>   yeang-shing.then<br/>
    /// <b>Since</b>    2006-07-11<br/>
    /// </remarks>
    public class Cryptography
    {
        /// <summary>
        /// A generic key for extra encrypt and decrypt.
        /// </summary>
        private static string Key = "plexus77";
        /// <summary>
        /// Encrypt/Decrypt class.
        /// </summary>
        /// <example>
        /// <code>
        /// string encrypted = Cryptography.Encrypt(txtPassword.Text.Trim());
        /// Login(txtUser.Text.Trim(), encrypted);
        /// </code>
        /// </example>
        public Cryptography()
        {
        }
        /// <summary>
        /// Decrypt a string.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static string Decrypt(string sender)
        {
            return Cryptography.Decrypt(sender, Key);
        }
        internal static string Decrypt(string sender, string key)
        {
            string text1;
            if (sender == null) sender = "";

            byte[] buffer4 = new byte[0];
            byte[] buffer1 = buffer4;
            byte[] buffer2 = new byte[] { 110, 120, 130, 140, 150, 160, 170, 180 };
            byte[] buffer3 = new byte[sender.Length];

            try
            {
                buffer1 = Encoding.UTF8.GetBytes(key.Substring(0, 8));
                DESCryptoServiceProvider provider1 = new DESCryptoServiceProvider();
                buffer3 = Convert.FromBase64String(sender);
                MemoryStream stream1 = new MemoryStream();
                CryptoStream stream2 = new CryptoStream(stream1, provider1.CreateDecryptor(buffer1, buffer2), CryptoStreamMode.Write);
                stream2.Write(buffer3, 0, buffer3.Length);
                stream2.FlushFinalBlock();
                text1 = Encoding.UTF8.GetString(stream1.ToArray());
            }
            catch (Exception ex)
            {
                text1 = string.Empty;
            }
            return text1;
        }
        /// <summary>
        /// Encrypt a string.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static string Encrypt(string sender)
        {
            //must min 8 char length
            return Cryptography.Encrypt(sender, Key);
        }
        internal static string Encrypt(string sender, string key)
        {
            string text1;
            if (sender == null) sender = "";

            byte[] buffer4 = new byte[0];
            byte[] buffer1 = buffer4;
            byte[] buffer2 = new byte[] { 110, 120, 130, 140, 150, 160, 170, 180 };

            try
            {
                buffer1 = Encoding.UTF8.GetBytes(key.Substring(0, 8));
                DESCryptoServiceProvider provider1 = new DESCryptoServiceProvider();
                byte[] buffer3 = Encoding.UTF8.GetBytes(sender);
                MemoryStream stream1 = new MemoryStream();
                CryptoStream stream2 = new CryptoStream(stream1, provider1.CreateEncryptor(buffer1, buffer2), CryptoStreamMode.Write);
                stream2.Write(buffer3, 0, buffer3.Length);
                stream2.FlushFinalBlock();
                text1 = Convert.ToBase64String(stream1.ToArray());
            }
            catch (Exception ex)
            {
                text1 = string.Empty;
            }
            return text1;
        }
    }
}