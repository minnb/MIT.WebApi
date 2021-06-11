using Microsoft.Extensions.Logging;
using MIT.Utils;
using MIT.Utils.Helpers;
using MIT.WebApi.GPAY.Application.Interfaces;
using MIT.WebApi.GPAY.ViewModels;
using MIT.WebApi.GPAY.ViewModels.AirPay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.WebApi.GPAY.Application.Implementation
{
    public class AirPayService : IAirPayService
    {
        private readonly ILogger<AirPayService> _logger;
        public AirPayService(
            ILogger<AirPayService> logger
            )
        {
            _logger = logger;
        }
        public ResponseObject CallPurchaseCardV2(WebApiViewModel _webApiAirPayInfo, POSPurchaseCardRequest bodyData)
        {
            try
            {
                var posRequest = new PurchaseCard()
                {
                    brand_id = bodyData.Brand_id.ToString(),
                    service_code = bodyData.Service_code.ToString(),
                    quantity = bodyData.Quantity.ToString(),
                    partner_id = _webApiAirPayInfo.UserName.ToString(),
                    reference_no = Guid.NewGuid().ToString("N").Replace("-", "")
                };
                QueryStringHelper queryStringHelper = new();
                CreateQueryPurchaseCard(queryStringHelper, posRequest);

                var dataRequest = new PurchaseCardAirPay()
                {
                    brand_id = posRequest.brand_id,
                    service_code = posRequest.service_code,
                    quantity = posRequest.quantity,
                    partner_id = posRequest.partner_id,
                    reference_no = posRequest.reference_no,
                    signature = SignatureAirPay(queryStringHelper, _webApiAirPayInfo.PublicKey)
                };

                ApiHelper api = new(
                        _webApiAirPayInfo.Host + _webApiAirPayInfo.WebRouteDto.Where(x => x.Name == "purchase_card").FirstOrDefault().Route.ToString(),
                        "",
                        "",
                        "POST",
                        JsonConvert.SerializeObject(dataRequest),
                        false
                        );

                _logger.LogWarning("Request: " + JsonConvert.SerializeObject(dataRequest));

                var rp = JsonConvert.DeserializeObject<RespCode>(api.InteractWithApi());

                _logger.LogWarning("Response: " + rp);

                return new ResponseObject()
                {
                    Meta = new Meta { Code = rp.resp_code, Message = rp.resp_msg },
                    Data = null
                };

            }
            catch(Exception ex)
            {
                return new ResponseObject()
                {
                    Meta = new Meta { Code = 9999, Message = ex.Message.ToString() },
                    Data = null
                };
            }
        }

        private static string SignatureAirPay(QueryStringHelper queryStringHelper, string public_key)
        {
            string rawData = queryStringHelper.GetRequestRaw();

            var strSHA1 = RsaUtils.Sha1(rawData);

            var signature = RsaUtils.RSAEncrypt(public_key, strSHA1);

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(signature);

            return System.Convert.ToBase64String(plainTextBytes);
        }

        private static void CreateQueryPurchaseCard(QueryStringHelper queryStringHelper, PurchaseCard pay)
        {
            queryStringHelper.AddRequestData("service_code", pay.service_code);
            queryStringHelper.AddRequestData("quantity", pay.quantity.ToString());
            queryStringHelper.AddRequestData("reference_no", pay.reference_no);
            queryStringHelper.AddRequestData("partner_id", pay.partner_id);
            queryStringHelper.AddRequestData("brand_id", pay.brand_id);
        }
    }
}
