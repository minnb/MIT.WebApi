using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Shared.API;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.ViewModels;
using VCM.Partner.API.ViewModels.AirPay;

namespace VCM.Partner.API.Controllers
{
    //[Authorize]
    public class PartnerController : BaseController
    {
        private readonly ILogger<PartnerController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IAirPayService _airPayService;
        private readonly IMobiCastService _mobiCastService;
        private string _proxyHttp = "";
        private string[] _bypassList;
        public PartnerController
            (
                 IConfiguration configuration,
                 ILogger<PartnerController> logger,
                 IMemoryCacheService memoryCacheService,
                 IAirPayService airPayService,
                 IMobiCastService mobiCastService
            )
        {
            _configuration = configuration;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
            _airPayService = airPayService;
            _mobiCastService = mobiCastService;
            if (!string.IsNullOrEmpty(_configuration["WebProxy:Http"]) && !string.IsNullOrEmpty(_configuration["WebProxy:Port"]))
            {
                _proxyHttp = _configuration["WebProxy:Http"];
                _bypassList = new string[] { _configuration["WebProxy:Bypasslist"] };
            }
        }

        [HttpPost]
        [Route("api/v1/partner/card")]
        public async Task<ResponseClient> PurchaseCardV2Async([FromBody] POSPurchaseCardRequest bodyData)
        {
            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();

                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result ? .Where(x => x.AppCode == bodyData.PartnerCode).SingleOrDefault();
                var itemData = _memoryCacheService.GetItemAsync().Result ? .Where(x => x.AppCode == bodyData.PartnerCode.ToUpper()).ToList();
                switch (bodyData.PartnerCode.ToUpper())
                {
                    case "APY":
                        string privateKeyVCM = _memoryCacheService.GetDataWebApiAsync().Result ? .Where(x => x.AppCode == "VCM").SingleOrDefault().PrivateKey.ToString();
                        responseObject = _airPayService.CallPurchaseCardV2(_webApiAirPayInfo, bodyData, itemData, privateKeyVCM, _proxyHttp, _bypassList);

                        break;
                    case "MBC":
                        responseObject = await _mobiCastService.PurchaseCardMBC_TripleAsync(
                                                                                    bodyData,
                                                                                    _webApiAirPayInfo,
                                                                                    await _memoryCacheService.MBCTokenAsync(_webApiAirPayInfo, _proxyHttp, _bypassList),
                                                                                    itemData,
                                                                                    _proxyHttp, _bypassList);
                        break;
                    default:
                        break;
                }

                return responseObject;
            }
            else
            {
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/partner/topup")]
        public async Task<ResponseClient> TopupAsync([FromBody] POSTopupRequest request)
        {
            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();

                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result ? .Where(x => x.AppCode == request.PartnerCode).SingleOrDefault();
                var itemData = _memoryCacheService.GetItemAsync().Result ? .Where(x => x.AppCode == request.PartnerCode.ToUpper()).ToList();
                switch (request.PartnerCode.ToUpper())
                {
                    case "MBC":
                        responseObject = await _mobiCastService.TopupMBC_Triple(request, _webApiAirPayInfo, await _memoryCacheService.MBCTokenAsync(_webApiAirPayInfo, _proxyHttp, _bypassList), itemData, _proxyHttp, _bypassList);
                        break;
                    default:
                        break;
                }

                return responseObject;
            }
            else
            {
                _logger.LogWarning("v1/api/partner/topup: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }
    }
}
