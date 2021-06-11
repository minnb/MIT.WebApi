using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MIT.Utils.Helpers
{
    public class ApiHelper
    {
        public string ApiAddress { get; set; }
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string WebMethod { get; set; }
        public string JsonData { get; set; }
        public bool HasHeaders { get; set; }
        public ApiHelper() { }
        public ApiHelper(string apiAdress, string tokenType, string accessToken, string webMethod, string jsonData = null, bool hasHeaders = false)
        {
            this.ApiAddress = apiAdress;
            this.TokenType = tokenType;
            this.AccessToken = accessToken;
            this.WebMethod = webMethod;
            this.JsonData = jsonData;
            this.HasHeaders = hasHeaders;
        }
        public string InteractWithApi()
        {
            string result = string.Empty;
            try
            {
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(ApiAddress);
                Request.ContentType = "application/json";
                Request.PreAuthenticate = true;

                if (HasHeaders)
                {
                    Request.Headers.Add("Authorization", TokenType + " " + AccessToken);
                }

                Request.Method = WebMethod;

                if (WebMethod.ToUpper() == "POST" || WebMethod.ToUpper() == "PUT")
                {
                    using (var streamWriter = new StreamWriter(Request.GetRequestStream()))
                    {
                        streamWriter.Write(JsonData);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }

                HttpWebResponse Response = null;
                Response = (HttpWebResponse)Request.GetResponse();
                using (Stream stream = Response.GetResponseStream())
                {
                    StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
                    result = streamReader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
            return result;
        }
    }
}
