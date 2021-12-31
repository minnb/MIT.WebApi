using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MIT.Utils
{
    public static class Base64Utils
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public async static Task<string> GetImageAsBase64Url(string url)
        {
            try
            {
                var credentials = new NetworkCredential("user", "pw");
                using var handler = new HttpClientHandler { Credentials = credentials };
                using var client = new HttpClient(handler);
                var bytes = await client.GetByteArrayAsync(url);
                return "image/jpeg;base64," + Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                return null;
            }
        }
    }
}
