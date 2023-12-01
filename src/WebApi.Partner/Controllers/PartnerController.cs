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
using WebApi.Partner.Authentication;
using VCM.Shared.Enums;
using VCM.Shared.API.Wintel;
using WebApi.Partner.ViewModels.MBC;
using System.ComponentModel.DataAnnotations;
using WebApi.Partner.ViewModels.O2;
using WebApi.Partner.Application.Interfaces;
using VCM.Shared.API.O2;

namespace VCM.Partner.API.Controllers
{
    public class PartnerController : BaseController
    {
        private readonly ILogger<PartnerController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IAirPayService _airPayService;
        private readonly IMobiCastService _mobiCastService;
        private readonly IO2Service _O2Service;
        private string _proxyHttp = "";
        private string[] _bypassList;
        public PartnerController
            (
                 IConfiguration configuration,
                 ILogger<PartnerController> logger,
                 IMemoryCacheService memoryCacheService,
                 IAirPayService airPayService,
                 IMobiCastService mobiCastService,
                 IO2Service O2Service
            )
        {
            _configuration = configuration;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
            _airPayService = airPayService;
            _mobiCastService = mobiCastService;
            _O2Service = O2Service;
        //if (!string.IsNullOrEmpty(_configuration["WebProxy:Http"]) && !string.IsNullOrEmpty(_configuration["WebProxy:Port"]))
        //{
        //    _proxyHttp = _configuration["WebProxy:Http"];
        //    _bypassList = new string[] { _configuration["WebProxy:Bypasslist"] };
        //}
    }

        [HttpPost]
        [Route("api/v1/partner/card")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> PurchaseCardV2Async([FromBody] POSPurchaseCardRequest bodyData)
        {
            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();

                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result ? .Where(x => x.AppCode == bodyData.PartnerCode).SingleOrDefault();
                var itemData = _memoryCacheService.GetItemAsync().Result ? .Where(x => x.AppCode == bodyData.PartnerCode.ToUpper()).ToList();
                _proxyHttp = _webApiAirPayInfo.HttpProxy;
                _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };
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
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> TopupAsync([FromBody] POSTopupRequest request)
        {
            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();

                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result ? .Where(x => x.AppCode == request.PartnerCode).SingleOrDefault();
                var itemData = _memoryCacheService.GetItemAsync().Result ? .Where(x => x.AppCode == request.PartnerCode.ToUpper()).ToList();
                _proxyHttp = _webApiAirPayInfo.HttpProxy;
                _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };
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

        [HttpPost]
        [Route("api/v1/partner/check/serial")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> ValidateKitStatus([FromBody] ValidateKitStatusRequest request)
        {
            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();

                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == request.PartnerCode).SingleOrDefault();
                var itemData = _memoryCacheService.GetItemAsync().Result?.Where(x => x.AppCode == request.PartnerCode.ToUpper()).ToList();
                if(_webApiAirPayInfo != null)
                {
                    _proxyHttp = _webApiAirPayInfo.HttpProxy;
                    _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };
                }
                switch (request.PartnerCode.ToUpper())
                {
                    case "MBC":
                        var userWintel = await _memoryCacheService.MBCTokenAsync(_webApiAirPayInfo, _proxyHttp, _bypassList);
                        if(userWintel != null)
                        {
                            if (request.ServiceType == "KIT_WINTEL_WIN99")
                            {
                                responseObject = await _mobiCastService.ValidateKitStatus_MBC_Triple(request, _webApiAirPayInfo, userWintel, itemData, _proxyHttp, _bypassList);
                            }
                            else
                            {
                                responseObject = ResponseHelper.RspNotFoundData("ServiceType: " + request.ServiceType + " không đúng");
                            }
                        }
                        else
                        {
                            responseObject = ResponseHelper.RspNotFoundData("Lỗi đăng nhập API WINTEL");
                        }
                        break;
                    case "O2":
                        if(request.ServiceType.ToUpper() == O2ServiceEnum.O2_PING.ToString())
                        {
                            responseObject = await _O2Service.PingO2(_webApiAirPayInfo);
                        }
                        else if (request.ServiceType.ToUpper() == O2ServiceEnum.O2_CHECK_MEMBER.ToString())
                        {
                            responseObject = await _O2Service.CheckPhoneNumberAsync(_webApiAirPayInfo, request, itemData);
                        }
                        else
                        {
                            responseObject = ResponseHelper.RspNotFoundData("ServiceType " + request.ServiceType +" của O2 không đúng");
                        }
                        break;
                    default:
                        responseObject = ResponseHelper.RspNotFoundData("PartnerCode, ServiceType không đúng");
                        break;
                }

                return responseObject;
            }
            else
            {
                _logger.LogWarning("api/v1/partner/check/serial: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/partner/esim")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> CreateEsimOrderWintel([FromBody] CreateEsimOrderPosWcmRequest request)
        {
            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();

                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == request.PartnerCode).SingleOrDefault();
                var itemData = _memoryCacheService.GetItemAsync().Result?.Where(x => x.AppCode == request.PartnerCode.ToUpper()).ToList();
                _proxyHttp = _webApiAirPayInfo.HttpProxy;
                _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };
                switch (request.PartnerCode.ToUpper())
                {
                    case "MBC":
                        var userWintel = await _memoryCacheService.MBCTokenAsync(_webApiAirPayInfo, _proxyHttp, _bypassList);
                        if (userWintel != null)
                        {
                            if (request.ServiceType == "WINTEL_LIST_PACKAGE") //(1)
                            {
                                responseObject = await _mobiCastService.GetListPackage_MBC_Triple(request, _webApiAirPayInfo, userWintel, itemData, _proxyHttp, _bypassList);
                            }
                            else if (request.ServiceType == "WINTEL_SELECT_PHONE") //(2)
                            {
                                responseObject = await _mobiCastService.GetSingleIsdn_MBC_Triple(request, _webApiAirPayInfo, userWintel, itemData, _proxyHttp, _bypassList);
                            }
                            else if (request.ServiceType == "WINTEL_KEEP_PHONE")  //(3)
                            {
                                responseObject = await _mobiCastService.KeepIsdn_MBC_Triple(request, _webApiAirPayInfo, userWintel, itemData, _proxyHttp, _bypassList);
                            }
                            else if(request.ServiceType == "WINTEL_CREATE_ORDER")  //(4)
                            {
                                responseObject = await _mobiCastService.CreateEsimOrder_MBC_Triple(request, _webApiAirPayInfo, userWintel, itemData, _proxyHttp, _bypassList);
                            }
                            else
                            {
                                responseObject = ResponseHelper.RspNotFoundData("ServiceType: " + request.ServiceType + " không đúng");
                            }
                        }
                        else
                        {
                            responseObject = ResponseHelper.RspNotFoundData("Lỗi đăng nhập API WINTEL");
                        }
                        break;
                    default:
                        responseObject = ResponseHelper.RspNotFoundData("PartnerCode, ServiceType không đúng");
                        break;
                }

                return responseObject;
            }
            else
            {
                _logger.LogWarning("api/v1/partner/esim: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }
        [HttpPost]
        [Route("api/v1/partner/physical-sim")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> CreatePhysicalSimOrderWintel([FromBody] CreatePhysicalSimOrderPosWcmRequest request)
        {
            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();

                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == request.PartnerCode).SingleOrDefault();
                var itemData = _memoryCacheService.GetItemAsync().Result?.Where(x => x.AppCode == request.PartnerCode.ToUpper()).ToList();
                _proxyHttp = _webApiAirPayInfo.HttpProxy;
                _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };
                switch (request.PartnerCode.ToUpper())
                {
                    case "MBC":
                        var userWintel = await _memoryCacheService.MBCTokenAsync(_webApiAirPayInfo, _proxyHttp, _bypassList);
                        if (userWintel != null)
                        {
                            if (request.ServiceType == "WINTEL_LIST_PACKAGE") //(1)
                            {
                                responseObject = await _mobiCastService.GetListPackagePhysicalSim_Wintel_Triple(request, _webApiAirPayInfo, userWintel, itemData, _proxyHttp, _bypassList);
                            }
                            else if (request.ServiceType == "WINTEL_SELECT_PHONE") //(2)
                            {
                                responseObject = await _mobiCastService.GetSingleIsdnNoneEsim_MBC_Triple(request, _webApiAirPayInfo, userWintel, itemData, _proxyHttp, _bypassList);
                            }
                            else if (request.ServiceType == "WINTEL_KEEP_PHONE")  //(3)
                            {
                                responseObject = await _mobiCastService.KeepIsdn_MBC_Triple(request, _webApiAirPayInfo, userWintel, itemData, _proxyHttp, _bypassList);
                            }
                            else if (request.ServiceType == "WINTEL_VALIDATE_SIM_STATUS")  //(4)
                            {
                                responseObject = await _mobiCastService.ValidateSimStatu_MBC_Triple(request, _webApiAirPayInfo, userWintel, itemData, _proxyHttp, _bypassList);
                            }
                            else if (request.ServiceType == "WINTEL_CREATE_ORDER")  //(5)
                            {
                                responseObject = await _mobiCastService.CreatePhysicalSimOrder_MBC_Triple(request, _webApiAirPayInfo, userWintel, itemData, _proxyHttp, _bypassList);
                            }
                            else
                            {
                                responseObject = ResponseHelper.RspNotFoundData("ServiceType Physical Sim: " + request.ServiceType + " không đúng");
                            }
                        }
                        else
                        {
                            responseObject = ResponseHelper.RspNotFoundData("Lỗi đăng nhập API WINTEL");
                        }
                        break;
                    default:
                        responseObject = ResponseHelper.RspNotFoundData("PartnerCode, ServiceType Physical Sim không đúng");
                        break;
                }

                return responseObject;
            }
            else
            {
                _logger.LogWarning("api/v1/partner/physical-sim: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/partner/check/subscriber-info")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> GetSubscriberInfo([FromBody] ValidateKitStatusRequest request)
        {
            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();

                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == request.PartnerCode).SingleOrDefault();
                var itemData = _memoryCacheService.GetItemAsync().Result?.Where(x => x.AppCode == request.PartnerCode.ToUpper()).ToList();
                _proxyHttp = _webApiAirPayInfo.HttpProxy;
                _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };
                switch (request.PartnerCode.ToUpper())
                {
                    case "MBC":
                        var userWintel = await _memoryCacheService.MBCTokenAsync(_webApiAirPayInfo, _proxyHttp, _bypassList);
                        if (userWintel != null)
                        {
                            if (request.ServiceType == "KIT_WINTEL_INFO")
                            {
                                responseObject = await _mobiCastService.CheckExtendSubscriberInfo_MBC_Triple(request, _webApiAirPayInfo, userWintel, itemData, _proxyHttp, _bypassList);
                            }
                            else
                            {
                                responseObject = ResponseHelper.RspNotFoundData("ServiceType: " + request.ServiceType + " không đúng");
                            }
                        }
                        else
                        {
                            responseObject = ResponseHelper.RspNotFoundData("Lỗi đăng nhập API WINTEL");
                        }
                        break;
                    default:
                        responseObject = ResponseHelper.RspNotFoundData("PartnerCode, ServiceType không đúng");
                        break;
                }

                return responseObject;
            }
            else
            {
                _logger.LogWarning("api/v1/partner/check/serial: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/partner/o2/update")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> UpdateTierO2([FromBody] UpdateTierMemberO2 request)
        {
            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();

                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == request.PartnerCode).SingleOrDefault();
                var itemData = _memoryCacheService.GetItemAsync().Result?.Where(x => x.AppCode == request.PartnerCode.ToUpper()).ToList();
                _proxyHttp = _webApiAirPayInfo.HttpProxy;
                _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };
                switch (request.PartnerCode.ToUpper())
                {
                    case "O2":
                        switch (request.ServiceType.ToUpper())
                        {
                            case "O2_UPDATE_TIER":
                                return await _O2Service.UpdateTierMemberO2Async(_webApiAirPayInfo, request);
                            default:
                                return ResponseHelper.RspNotFoundData("ServiceType không đúng");
                        }
                        
                    default:
                        return ResponseHelper.RspNotFoundData("PartnerCode không đúng");

                }
            }
            else
            {
                _logger.LogWarning("api/v1/partner/o2/update-tier: " + ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }
    }
}
