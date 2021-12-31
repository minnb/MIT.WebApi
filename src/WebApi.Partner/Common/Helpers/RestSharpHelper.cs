using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VCM.Partner.API.ViewModels.AirPay;

namespace VCM.Partner.API.Common.Helpers
{
    public static class RestSharpHelper
    {
        public static async Task<IRestResponse> InteractWithApi(WebApiViewModel webApiInfo, string function, Method method, string param = null, Dictionary<string, string> header = null, object bodyData = null)
        {
            IRestResponse response = new RestResponse();
            try
            {
                RestClient client = new RestClient(webApiInfo.Host)
                {
                    Timeout = 30000
                };

                string router = webApiInfo.WebRoute.Where(x => x.Name == function).FirstOrDefault().Route;

                if (!string.IsNullOrEmpty(param))
                {
                    router += param;
                }
                RestRequest restRequest = new RestRequest(router, method);

                restRequest.AddHeader("Accept", "application/json");

                if (header != null)
                {
                    foreach (var p in header)
                    {
                        restRequest.AddHeader(p.Key, p.Value);
                    }
                }

                if (bodyData != null)
                {
                    restRequest.AddJsonBody(bodyData);
                }

                response = await client.ExecuteAsync(restRequest);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = ResponseStatus.Error;
                response.ErrorMessage = ex.Message;
                response.ErrorException = ex;
            }

            return response;
        }
    }
}
