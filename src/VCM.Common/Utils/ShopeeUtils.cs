using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace VCM.Common.Utils
{
    public static class ShopeeUtils
    {
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        public static byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }
        public static string CreateSignature(string appKey, string baseString)
        {
            if(!string.IsNullOrEmpty(baseString) && !string.IsNullOrEmpty(appKey))
            {
                HMACSHA256 hashObject = new HMACSHA256(FromHex(appKey));

                var signature = hashObject.ComputeHash(Encoding.UTF8.GetBytes(baseString));

                var encodedSignature = BitConverter.ToString(signature).Replace("-", string.Empty).ToLower().ToString();

                return encodedSignature;
            }
            else
            {
                return "";
            }
        }

        public static string CreateSignature2(string key, string text)
        {
            // change according to your needs, an UTF8Encoding
            // could be more suitable in certain situations
            ASCIIEncoding encoding = new ASCIIEncoding();

            Byte[] textBytes = encoding.GetBytes(text);
            Byte[] keyBytes = encoding.GetBytes(key);

            Byte[] hashBytes;

            using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        }
        public static string CreateSignature3(string appkey, string message)
        {
            Encoding utf8 = Encoding.UTF8;

            byte[] keyByte =  utf8.GetBytes(appkey);
            byte[] messageBytes = utf8.GetBytes(message);

            byte[] hashmessage = new HMACSHA256(keyByte).ComputeHash(messageBytes);

            return  String.Concat(Array.ConvertAll(hashmessage, x => x.ToString("x2")));

        }


        public static string CreateBaseString(string method, string url, string param)
        {
            if(method.ToUpper() == "GET")
            {
                return method + "|" + url + "|";
            }
            else
            {
                return method + "|" + url + "|" + param;
            }
        }

    }
}
