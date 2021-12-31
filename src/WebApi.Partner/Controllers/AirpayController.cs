using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.ViewModels.AirPay;

namespace VCM.Partner.API.Controllers
{
    [Authorize]
    public class AirpayController : BaseController
    {
        private readonly string _appCode = "APY";
        private readonly IConfiguration _configuration;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IAirPayService _airPayService;
        private readonly WebApiViewModel _webApiAirPayInfo;
        private readonly string _proxyHttp = "";
        private readonly string[] _bypassList;
        public AirpayController(
            IConfiguration configuration,
            IMemoryCacheService memoryCacheService,
            IAirPayService airPayService
            )
        {
            _configuration = configuration;
            _memoryCacheService = memoryCacheService;
            _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result ? .Where(x => x.AppCode == _appCode).SingleOrDefault();
            _airPayService = airPayService;

            if (!string.IsNullOrEmpty(_configuration["WebProxy:Http"]) && !string.IsNullOrEmpty(_configuration["WebProxy:Port"]))
            {
                _proxyHttp = _configuration["WebProxy:Http"];
                _bypassList = new string[] { "10.*;*.winmart.vn" };
            }
        }

        //[HttpPost]
        //[Route("v1/api/air-pay/purchase-card-v2")]
        //public ResponseObject PurchaseCardV2([FromBody] POSPurchaseCardRequest bodyData)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string privateKeyVCM = _memoryCacheService.GetDataWebApi().Where(x => x.AppCode == "VCM").SingleOrDefault().PrivateKey.ToString();

        //        return _airPayService.CallPurchaseCardV2(_webApiAirPayInfo, bodyData, privateKeyVCM, _proxyHttp, _bypassList);
        //    }
        //    else
        //    {
        //        return new ResponseObject()
        //        {
        //            Meta = new Meta { Code = 9999, Message = ModelState.Values.First().Errors[0].ErrorMessage.ToString() },
        //            Data = null
        //        };
        //    }
        //}

        //[HttpPost]
        //[Route("v1/api/air-pay/get-card-v2")]
        //public ResponseObject GetCardV2([FromBody] POSGetCardV2Request bodyData)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string privateKeyVCM = _memoryCacheService.GetDataWebApi().Where(x => x.AppCode == "VCM").SingleOrDefault().PrivateKey.ToString();
        //        return _airPayService.GetCardV2(_webApiAirPayInfo, bodyData, privateKeyVCM, _proxyHttp, _bypassList);
        //    }
        //    else
        //    {
        //        return new ResponseObject()
        //        {
        //            Meta = new Meta { Code = 9999, Message = ModelState.Values.First().Errors[0].ErrorMessage.ToString() },
        //            Data = null
        //        };
        //    }
        //}

        //[HttpPost]
        //[Route("v1/api/air-pay/get-balance")]
        //public ResponseObject GetBalance()
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string privateKeyVCM = _memoryCacheService.GetDataWebApi().Where(x => x.AppCode == "VCM").SingleOrDefault().PrivateKey.ToString();
        //        return _airPayService.GetBalance(_webApiAirPayInfo, privateKeyVCM, _proxyHttp, _bypassList);
        //    }
        //    else
        //    {
        //        return new ResponseObject()
        //        {
        //            Meta = new Meta { Code = 9999, Message = ModelState.Values.First().Errors[0].ErrorMessage.ToString() },
        //            Data = null
        //        };
        //    }
        //}

        //[HttpPost]
        //[Route("v1/api/air-pay/get-transactions")]
        //public ResponseObject GetTransactions([FromBody]  GetTransactionsRequest bodyData)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string privateKeyVCM = _memoryCacheService.GetDataWebApi().Where(x => x.AppCode == "VCM").SingleOrDefault().PrivateKey.ToString();
        //        return _airPayService.GetTransactions(_webApiAirPayInfo, bodyData, privateKeyVCM, _proxyHttp, _bypassList);
        //    }
        //    else
        //    {
        //        return new ResponseObject()
        //        {
        //            Meta = new Meta { Code = 9999, Message = ModelState.Values.First().Errors[0].ErrorMessage.ToString() },
        //            Data = null
        //        };
        //    }
        //}
    }
}
