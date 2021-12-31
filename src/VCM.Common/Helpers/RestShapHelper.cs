using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace VCM.Common.Helpers
{
    public class RestShapHelper
    {
        public string ApiAddress { get; set; }
        public string ApiRoute { get; set; }
        public string WebMethod { get; set; }
        public object JsonData { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public int SetTimeOut { get; set; }
        public string HttpProxy { get; set; }
        public int PortProxy { get; set; }
        public RestShapHelper() { }
        public RestShapHelper(
            string apiAdress,
            string apiRoute,
            string webMethod,
            Dictionary<string, string> headers = null,
            Dictionary<string, string> parameters = null,
            object jsonData = null,
            int setTimeOut = 30000,
            string httpProxy = null,
            int portProxy = 0)
        {
            this.ApiAddress = apiAdress;
            this.ApiRoute = apiRoute;
            this.WebMethod = webMethod;
            this.Headers = headers;
            this.Parameters = parameters;
            this.JsonData = jsonData;
            this.SetTimeOut = setTimeOut;
            this.HttpProxy = httpProxy;
            this.PortProxy = portProxy;
        }
        public string InteractWithApi(ref int status, ref string mess_error)
        {
            string result = string.Empty;
            try
            {
                RestClient client = new RestClient(ApiAddress)
                {
                    Timeout = SetTimeOut
                };

                RestRequest request = new RestRequest(ApiRoute, GetMethod());

                if (Headers != null)
                {
                    foreach (var p in Headers)
                    {
                        request.AddHeader(p.Key, p.Value);
                    }
                }
                if (JsonData != null)
                {
                    request.AddJsonBody(JsonData);
                }

                if (Parameters != null)
                {
                    foreach (var p in Parameters)
                    {
                        if (p.Key == "application/xml")
                        {
                            request.AddParameter("application/xml", p.Value, ParameterType.RequestBody);
                        }
                        else
                        {
                            request.AddParameter(p.Key, p.Value);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(HttpProxy))
                {
                    WebProxy myproxy = new WebProxy(HttpProxy, PortProxy);
                    client.Proxy = myproxy;
                }

                var rp = client.Execute(request);
                if (rp.StatusCode == HttpStatusCode.OK)
                {
                    result = rp.Content;
                    status = (int)HttpStatusCode.OK;
                }
                else
                {
                    status = (int)HttpStatusCode.BadRequest;
                    if (rp.StatusCode.ToString() == "Conflict")
                    {
                        result = rp.Content;
                    }
                    else
                    {
                        mess_error = JsonConvert.SerializeObject(rp.StatusCode.ToString()) + " ==> " + JsonConvert.SerializeObject(rp.Content.ToString());
                        FileHelper.WriteLogs("StatusCode: " + mess_error);
                    }
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
        private Method GetMethod()
        {
            var method = new Method();
            switch (WebMethod.ToUpper())
            {
                case "POST":
                    method = Method.POST;
                    break;
                case "PUT":
                    method = Method.PUT;
                    break;
                case "GET":
                    method = Method.GET;
                    break;
                case "PATCH":
                    method = Method.PATCH;
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }
            return method;
        }
    }
}
