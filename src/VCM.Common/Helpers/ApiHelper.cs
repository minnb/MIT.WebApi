﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace VCM.Common.Helpers
{
    public class ApiHelper
    {
        public string ApiAddress { get; set; }
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string WebMethod { get; set; }
        public string JsonData { get; set; }
        public bool HasHeaders { get; set; }
        public string ProxyHttp { get; set; }
        public string[] BypassList { get; set; }
        public ApiHelper() { }
        public ApiHelper(
            string apiAdress, 
            string tokenType, 
            string accessToken, 
            string webMethod, 
            string jsonData = null, 
            bool hasHeaders = false,
            string proxyHttp = "",
            string[] bypassList = null
            )
        {
            this.ApiAddress = apiAdress;
            this.TokenType = tokenType;
            this.AccessToken = accessToken;
            this.WebMethod = webMethod;
            this.JsonData = jsonData;
            this.HasHeaders = hasHeaders;
            this.ProxyHttp = proxyHttp;
            this.BypassList = bypassList;
        }
        public string InteractWithApi()
        {
            string result = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ApiAddress);
                request.ContentType = "application/json";
                request.PreAuthenticate = true;
                request.Timeout = 45000;

                if (!string.IsNullOrEmpty(ProxyHttp))
                {
                    WebProxy proxy = new WebProxy(ProxyHttp, false, BypassList);
                    request.Proxy = proxy;
                }

                if (HasHeaders)
                {
                    if (!string.IsNullOrEmpty(TokenType))
                    {
                        request.Headers.Add("Authorization", TokenType + " " + AccessToken);
                    }
                    else
                    {
                        request.Headers.Add("Authorization", AccessToken);
                    }
                    
                }
                Console.WriteLine(request.Headers);
                request.Method = WebMethod;

                if (WebMethod.ToUpper() == "POST" || WebMethod.ToUpper() == "PUT")
                {
                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWriter.Write(JsonData);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }

                HttpWebResponse Response = null;
                
                Response = (HttpWebResponse)request.GetResponse();
                
                if(Response.StatusCode == HttpStatusCode.OK)
                {
                    using Stream stream = Response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
                    result = streamReader.ReadToEnd();
                }
                else
                {
                    result = string.Empty;
                }
                
            }
            catch (WebException ex)
            {
                using WebResponse response = ex.Response;
                if (response != null)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    using Stream data = response.GetResponseStream();
                    using var reader = new StreamReader(data);
                    result = reader.ReadToEnd();
                    FileHelper.WriteLogs("InteractWithApi.WebException: " + result);
                }
                FileHelper.WriteLogs("InteractWithApi.WebException: " + JsonConvert.SerializeObject(response));
            }
            return result;
        }
        public HttpWebResponse InteractWithApiResponse(ref string errMsg)
        {
            //HttpWebResponse result = new HttpWebResponse();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ApiAddress);
                request.ContentType = "application/json";
                request.PreAuthenticate = true;
                request.Timeout = 30000;

                if (!String.IsNullOrEmpty(ProxyHttp))
                {
                    WebProxy proxy = new WebProxy(ProxyHttp, false, BypassList);
                    request.Proxy = proxy;
                }

                if (HasHeaders)
                {
                    if (!string.IsNullOrEmpty(TokenType))
                    {
                        request.Headers.Add("Authorization", TokenType + " " + AccessToken);
                    }
                    else
                    {
                        request.Headers.Add("Authorization", AccessToken);
                    }

                }
                Console.WriteLine(request.Headers);
                request.Method = WebMethod;

                if (WebMethod.ToUpper() == "POST" || WebMethod.ToUpper() == "PUT")
                {
                    using var streamWriter = new StreamWriter(request.GetRequestStream());
                    streamWriter.Write(JsonData);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var result =  (HttpWebResponse)request.GetResponse();
                errMsg = result.StatusCode.ToString();
                return result;
            }
            catch(Exception ex) 
            {
                errMsg = ex.Message;
                FileHelper.WriteLogs("InteractWithApiResponse Exception: " + ex.Message.ToString());
                return null;
            }
        }
    }
}
