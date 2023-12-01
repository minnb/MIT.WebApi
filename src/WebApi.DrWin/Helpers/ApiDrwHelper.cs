using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VCM.Shared.Dtos;

namespace WebApi.DrWin.Helpers
{
    public class ApiDrwHelper
    {
        public SysWebApiDto webApiInfo { get; set; }
        public string body { get; set; }
        public string function { get; set; }
        public string token { get; set; }
        public string method { get; set; }
        public string requestId { get; set; }
        public string param { get; set; }
        public ApiDrwHelper() { }
        public ApiDrwHelper(
            SysWebApiDto webApiInfo,
            string body,
            string param,
            string function,
            string token,
            string method,
            string requestId
            )
        {
            this.webApiInfo = webApiInfo;
            this.body = body;
            this.function = function;
            this.token = token;
            this.method = method;
            this.requestId = requestId;
            this.param = param;
        }

        public async Task<string> InteractWithApiAsync()
        {
            string result = string.Empty;
            try
            {
                var routeApi = webApiInfo.WebRoute.Where(x => x.Name == function).FirstOrDefault();
                var url_request = webApiInfo.Host + routeApi.Route;

                if (param != null)
                {
                    url_request = url_request + param;
                }
              

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url_request);
                request.ContentType = "application/json";
                request.PreAuthenticate = false;
                request.Timeout = 60000;
                request.Method = method;
                request.KeepAlive = false;

                if (!string.IsNullOrEmpty(webApiInfo.HttpProxy) && !string.IsNullOrEmpty(webApiInfo.Bypasslist))
                {
                    WebProxy myProxy = new WebProxy();
                    Uri newUri = new Uri(webApiInfo.HttpProxy);
                    myProxy.Address = newUri;
                    request.Proxy = myProxy;
                }

                request.Headers.Add("Accept", "application/json");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", "bearer " + " " + token);
                }

 
                if (method.ToUpper() == "POST" || method.ToUpper() == "PUT" || method.ToUpper() == "DELETE")
                {
                    using var streamWriter = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.UTF8);
                    streamWriter.Write(body);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var response = (HttpWebResponse)await request.GetResponseAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using Stream stream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
                    result = streamReader.ReadToEnd();
                }
                else
                {
                    using Stream stream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
                    result = streamReader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                string content = "";
                using WebResponse response = ex.Response;
                if (response != null)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    using Stream data = response.GetResponseStream();
                    using var reader = new StreamReader(data);
                    content = reader.ReadToEnd();
                  
                }
                result = content!="" ? content: ex.Message.ToString();
            }
            return result;
        }
    }
}
