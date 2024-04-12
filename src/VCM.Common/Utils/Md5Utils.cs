using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MIT.Utils
{
    public static class Md5Utils
    {
        public static string CalculateMD5Hash(string input)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder stringBuilder = new StringBuilder();

            foreach (byte b in hashBytes)
            {
                stringBuilder.Append(b.ToString("x2"));
            }

            return stringBuilder.ToString();
        }
        public static string Md5(string sInput)
        {
            ASCIIEncoding enCoder = new ASCIIEncoding();
            byte[] valueByteArr = enCoder.GetBytes(sInput);
            // Encrypt Input string 
            HashAlgorithm algorithmType = new MD5CryptoServiceProvider();
            byte[] hashArray = algorithmType.ComputeHash(valueByteArr);
            //Convert byte hash to HEX
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashArray)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }
    }
}
