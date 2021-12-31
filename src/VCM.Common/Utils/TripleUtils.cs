using System;
using System.Security.Cryptography;
using System.Text;

namespace MIT.Utils.Utils
{
    public class TripleUtils
    {
        public static string Encrypt(string strKey, string data)
        {
            byte[] key = Encoding.UTF8.GetBytes(TripleUtils.ToMd5(Encoding.UTF8.GetBytes(strKey)).Substring(0, 24));
            byte[] bData = Encoding.UTF8.GetBytes(data);
            TripleDES des = new TripleDESCryptoServiceProvider
            {
                Mode = CipherMode.ECB,
                Key = key,
                Padding = PaddingMode.PKCS7,
            };
            var re = des.CreateEncryptor().TransformFinalBlock(bData, 0, bData.Length);

            return Convert.ToBase64String(re);
        }
        public static string ToMd5(byte[] data)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(data);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                sb.Append(result[i].ToString("x2"));
            }
            return sb.ToString();
        }
        public static string Decrypt(string strKey, string data)
        {

            byte[] key = Encoding.UTF8.GetBytes(TripleUtils.ToMd5(Encoding.UTF8.GetBytes(strKey)).Substring(0, 24));

            var bData = Convert.FromBase64String(data);

            TripleDES des = new TripleDESCryptoServiceProvider
            {
                Mode = CipherMode.ECB,
                Key = key,
                Padding = PaddingMode.PKCS7,
            };
            var re = des.CreateDecryptor().TransformFinalBlock(bData, 0, bData.Length);
            return Encoding.UTF8.GetString(re);
        }
    }
}
