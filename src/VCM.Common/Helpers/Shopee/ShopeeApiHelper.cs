using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using VCM.Shared.API.Shopee;

namespace VCM.Common.Helpers.Shopee
{
    public static class ShopeeApiHelper
    {
        private static Dictionary<string, string> AddHeaderRestSharp(string version, string app_id, string signature, ref int request_id)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            Random random = new Random();
            request_id = random.Next(1, 99999999);
            headers.Add("Authorization", "Signature " + signature);
            headers.Add("X-Foody-App-Id", app_id.ToString());
            headers.Add("X-Foody-Api-Version", version.ToString());
            headers.Add("X-Foody-Request-Id-", request_id.ToString());
            headers.Add("X-Foody-Language", "vi");
            headers.Add("X-Foody-Country", "VN");
            Console.WriteLine(headers);

            return headers;
        }
        public static string PostRestSharpApi(string url, string route, string version, string app_id, string signature, Dictionary<string, string> dictHeader, object jsonbody, string proxyHttp, int port, ref int request_id, int timeout = 60000)
        {
            RestShapHelper restShapHelper = new RestShapHelper(
                                            url,
                                            route,
                                            "POST",
                                            AddHeaderRestSharp(version, app_id, signature, ref request_id),
                                            null,
                                            jsonbody,
                                            timeout,
                                            proxyHttp,
                                            port
                                            );
            int status = 200;
            string error_mess = string.Empty;
            string result = restShapHelper.InteractWithApi(ref status, ref error_mess);
            return result;
        }

        //------------------------
        private static void AddHeader(HttpWebRequest request, string version, string app_id, string signature, ref int request_id)
        {
            Random random = new Random();
            request_id = random.Next(1, 99999999);
            request.Headers.Add("Content-Type", "application/json");
            request.Headers.Add("Authorization", "signature " + signature);
            request.Headers.Add("X-Foody-App-Id", app_id.ToString());
            request.Headers.Add("X-Foody-Api-Version", version.ToString());
            request.Headers.Add("X-Foody-Request-Id", request_id.ToString());
            request.Headers.Add("X-Foody-Language", "vi");
            request.Headers.Add("X-Foody-Country", "vn");
        }
        public static string PostApi(string route, string version, string app_id, string signature, Dictionary<string, string> dicHeader, string jsonbody, string proxyHttp, string[] byPass, ref int request_id, int timeout = 60000)
        {
            string result = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(route);
                //request.ContentType = "application/json";
                //request.PreAuthenticate = true;
                request.Timeout = timeout;

                if (!string.IsNullOrEmpty(proxyHttp))
                {
                    WebProxy proxy = new WebProxy(proxyHttp, false, byPass);
                    request.Proxy = proxy;
                }

                AddHeader(request, version, app_id, signature, ref request_id);

                if (dicHeader != null)
                {
                    foreach (var p in dicHeader)
                    {
                        request.Headers.Add(p.Key, p.Value);
                    }
                }
                request.Method = "POST";

                Console.WriteLine(request.Headers);
                Console.WriteLine(jsonbody);
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(jsonbody);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                HttpWebResponse Response = null;

                Response = (HttpWebResponse)request.GetResponse();

                if (Response.StatusCode == HttpStatusCode.OK)
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
                using (WebResponse response = ex.Response)
                {
                    if (response != null)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        using Stream data = response.GetResponseStream();
                        using var reader = new StreamReader(data);
                        if(httpResponse.StatusCode == HttpStatusCode.Forbidden)
                        {
                            result = JsonConvert.SerializeObject(new ResultShopeeRsp()
                            {
                                result = HttpStatusCode.Forbidden.ToString(),
                                reply = null
                            });
                        }
                        else
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
            }
            return result;
        }
        public static string GetApi(string route_full, string version, string app_id, string signature, string proxyHttp, string[] byPass, ref int request_id, int timeout = 60000)
        {
            string result = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(route_full);
                request.ContentType = "application/json";
                request.PreAuthenticate = true;
                request.Timeout = timeout;

                if (!string.IsNullOrEmpty(proxyHttp))
                {
                    WebProxy proxy = new WebProxy(proxyHttp, false, byPass);
                    request.Proxy = proxy;
                }
                AddHeader(request, version, app_id, signature, ref request_id);
                request.Method = "GET";

                HttpWebResponse Response = null;

                Response = (HttpWebResponse)request.GetResponse();

                if (Response.StatusCode == HttpStatusCode.OK)
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
                using (WebResponse response = ex.Response)
                {
                    if (response != null)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        using Stream data = response.GetResponseStream();
                        using var reader = new StreamReader(data);
                        if (httpResponse.StatusCode == HttpStatusCode.Forbidden)
                        {
                            result = JsonConvert.SerializeObject(new ResultShopeeRsp()
                            {
                                result = HttpStatusCode.Forbidden.ToString(),
                                reply = null
                            });
                        }
                        else
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
            }
            return result;
        }
    }
}
