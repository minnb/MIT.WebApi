using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VCM.Shared.Dtos;
using WebApi.DrWin.Helpers;

namespace WebApi.DrWin.Services
{
    public interface IApiService
    {
        Task<string> CallApiHttpWeb(SysWebApiDto webApiInfo, string body, string param, string function, string token, string method, string request);
        Task<IRestResponse> CallApi(SysWebApiDto webApiInfo, object body, Dictionary<string, string> param, string function, string token, string method, string request);
    }
    public class ApiService : IApiService
    {
        private readonly ILogger<ApiService> _logger;
        public ApiService(
            ILogger<ApiService> logger
        )
        {
            _logger = logger;
        }

        public async Task<string> CallApiHttpWeb(SysWebApiDto webApiInfo, string body, string param, string function, string token, string method, string requestId)
        {
            ApiDrwHelper apiDrwHelper = new ApiDrwHelper(webApiInfo, body, param, function, token, method, requestId );
            string result = await apiDrwHelper.InteractWithApiAsync();
            _logger.LogWarning("===> "+ requestId + "===>: " + result);
            return result;
        }

        public async Task<IRestResponse> CallApi(SysWebApiDto webApiInfo, object bodyData, Dictionary<string, string> param, string function, string token, string method, string request)
        {
            IRestResponse response = new RestResponse();
            try
            {
                var routeApi = webApiInfo.WebRoute.Where(x => x.Name == function).FirstOrDefault();
                var url_request = webApiInfo.Host + routeApi.Route.ToString();
                string router = webApiInfo.WebRoute.Where(x => x.Name == function).FirstOrDefault().Route;

                RestClient client = new RestClient(webApiInfo.Host)
                {
                    Timeout = 60000
                };

                RestRequest restRequest = new RestRequest(router, GetMethod(method));
                restRequest.AddHeader("Accept", "application/json");
                restRequest.AddHeader("Authorization", "bearer " + token);

                if (!string.IsNullOrEmpty(webApiInfo.HttpProxy))
                {
                    _logger.LogWarning(webApiInfo.HttpProxy);
                    WebProxy myproxy = new WebProxy(webApiInfo.HttpProxy, 9090);
                    client.Proxy = myproxy;
                    client.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                }

                if (bodyData != null)
                {
                    restRequest.AddJsonBody(bodyData);
                }

                if (param!=null)
                {
                    foreach (var p in param)
                    {

                        restRequest.AddParameter(p.Key, p.Value);

                    }
                }

                response = await client.ExecuteAsync(restRequest);
                _logger.LogWarning("===> response: " + request + " ===>" + response.Content != null ? response.Content.ToString():response.StatusDescription.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> CallApi.Exception: " + webApiInfo.AppCode + "-" + function + ": " + response.Content.ToString());
                _logger.LogWarning(ex.Message.ToString());
                response.ResponseStatus = ResponseStatus.Error;
                response.ErrorMessage = ex.Message;
                response.ErrorException = ex;
            }

            return response;
        }

        private Method GetMethod(string WebMethod)
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
                case "DELETE":
                    method = Method.DELETE;
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }
            return method;
        }
    }
}
