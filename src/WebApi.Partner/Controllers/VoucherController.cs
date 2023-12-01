using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.Controllers;
using VCM.Shared.API;
using VCM.Shared.API.BLUEPOS;
using VCM.Shared.API.Voucher;
using VCM.Shared.Enums;
using WebApi.Partner.Application.Implementation;
using WebApi.Partner.Authentication;

namespace WebApi.Partner.Controllers
{
    
    public class VoucherController : BaseController
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IPhucLongService _phucLongService;
        public readonly IVoucherService _voucherService;
        private string _proxyHttp = "";
        private string[] _bypassList;
        public VoucherController
        (
             IConfiguration configuration,
             ILogger<TransactionController> logger,
             IMemoryCacheService memoryCacheService,
             IPhucLongService phucLongService,
             IVoucherService voucherService
         )
        {
            _configuration = configuration;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
            _phucLongService = phucLongService;
            _voucherService = voucherService;
            if (!string.IsNullOrEmpty(_configuration["WebProxy:Http"]) && !string.IsNullOrEmpty(_configuration["WebProxy:Port"]))
            {
                _proxyHttp = _configuration["WebProxy:Http"];
                _bypassList = new string[] { _configuration["WebProxy:Bypasslist"] };
            }
        }

        //[HttpPost]
        //[Route("api/v1/voucher/update-status")]
        //public async Task<ResponseClient> UpdateOrderStatusAsync([FromBody] RequestVoucher request)
        //{
        //    if(request.Status == VoucherStatusEnum.REDE.ToString())
        //    {
        //        return ResponseHelper.RspNotWarning(201, @"Không có quyền sử dụng voucher/coupon");
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        ResponseClient responseObject = new ResponseClient();
        //        var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == request.PartnerCode.ToUpper()).SingleOrDefault();

        //        responseObject = request.PartnerCode.ToUpper() switch
        //        {
        //            "PLG" => await _phucLongService.UpdateStatusVoucherPhucLong(request, _webApiAirPayInfo),
        //            _ => responseObject = ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString())
        //        };

        //        return responseObject;
        //    }
        //    else
        //    {
        //        return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
        //    }
        //}


        [HttpPost]
        [Route("api/v1/voucher/register")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS, PermissionEnum.WEB })]
        public async Task<ResponseClient> IssueVoucher([FromBody] IssueVoucher request)
        {
            if (ModelState.IsValid)
            {
                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == "BLUEPOS").SingleOrDefault();
                var itemData = _memoryCacheService.GetItemAsync().Result?.Where(x => x.AppCode == "WCM".ToUpper() && x.PartnerItem.ToUpper() == "PAR_VOUCHER").FirstOrDefault();
                if(itemData == null || _webApiAirPayInfo == null)
                {
                    return ResponseHelper.RspNotWarning(1012, "Chưa cấu hình tạo voucher trên WebApi Partner");
                }
                _proxyHttp = _webApiAirPayInfo.HttpProxy;
                _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };

                var data = _voucherService.RegisterPhoneNumberAsync(request).Result;
                var meta = new Meta
                {
                    Message = data.Item2
                };

                if (data.Item1)
                {
                    meta.Code = 200;
                    var resultCreateVoucher = await _voucherService.CreateVoucherToSAPAsync(_webApiAirPayInfo, itemData, data.Item3, _proxyHttp, _bypassList);
                    if (!resultCreateVoucher.Item1)
                    {
                        await _voucherService.CreateVoucherToSAPAsync(_webApiAirPayInfo, itemData, data.Item3, _proxyHttp, _bypassList);
                    }
                }
                else
                {
                    meta.Code = 1012;
                }

                return new ResponseClient()
                {
                    Meta = meta,
                    Data = null
                };
            }
            else
            {
                return ResponseHelper.RspNotWarning(1012, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpGet]
        [Route("api/v1/voucher/check/phone")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> CheckVoucherByPhoneNumber([FromQuery] CheckVoucherSAP request)
        {
            if (ModelState.IsValid)
            {
                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == "BLUEPOS").SingleOrDefault();
                _proxyHttp = _webApiAirPayInfo.HttpProxy;
                _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };

                var resultCheck = await _voucherService.CheckVouchersync(_webApiAirPayInfo, request, _proxyHttp, _bypassList);

                if (resultCheck.Item1)
                {
                    return ResponseHelper.RspOK(resultCheck.Item3);
                }
                else
                {
                    return ResponseHelper.RspNotFoundData(resultCheck.Item2);
                }
            }
            else
            {
                return ResponseHelper.RspNotWarning(99998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPut]
        [Route("api/v1/voucher/update-status")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> UpdateOrderStatusAsync([FromBody] UpdateStatusVoucherSAP request)
        {
            if (ModelState.IsValid)
            {
                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == "BLUEPOS").SingleOrDefault();
                _proxyHttp = _webApiAirPayInfo.HttpProxy;
                _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };

                var data = await _voucherService.UpdateStatusVouchersync(_webApiAirPayInfo, request, _proxyHttp, _bypassList);
                var meta = new Meta
                {
                    Message = data.Item2
                };

                if (data.Item1)
                {
                    meta.Code = 200;
                }
                else
                {
                    meta.Code = 400;
                }

                return new ResponseClient()
                {
                    Meta = meta,
                    Data = null
                };
            }
            else
            {
                return ResponseHelper.RspNotWarning(1012, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }
    }
}
