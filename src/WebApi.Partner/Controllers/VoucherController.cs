using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.Controllers;
using VCM.Shared.API;
using VCM.Shared.Enums;

namespace WebApi.Partner.Controllers
{
    public class VoucherController : BaseController
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IPhucLongService _phucLongService;
        private string _proxyHttp = "";
        private string[] _bypassList;
        public VoucherController
        (
             IConfiguration configuration,
             ILogger<TransactionController> logger,
             IMemoryCacheService memoryCacheService,
             IPhucLongService phucLongService
         )
        {
            _configuration = configuration;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
            _phucLongService = phucLongService;
            if (!string.IsNullOrEmpty(_configuration["WebProxy:Http"]) && !string.IsNullOrEmpty(_configuration["WebProxy:Port"]))
            {
                _proxyHttp = _configuration["WebProxy:Http"];
                _bypassList = new string[] { _configuration["WebProxy:Bypasslist"] };
            }
        }

        [HttpPost]
        [Route("api/v1/voucher/update-status")]
        public async Task<ResponseClient> UpdateOrderStatusAsync([FromBody] RequestVoucher request)
        {
            if(request.Status == VoucherStatusEnum.REDE.ToString())
            {
                return ResponseHelper.RspNotWarning(201, @"Không có quyền sử dụng voucher/coupon");
            }

            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();
                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == request.PartnerCode.ToUpper()).SingleOrDefault();

                responseObject = request.PartnerCode.ToUpper() switch
                {
                    "PLG" => await _phucLongService.UpdateStatusVoucherPhucLong(request, _webApiAirPayInfo),
                    _ => responseObject = ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString())
                };

                return responseObject;
            }
            else
            {
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

    }
}
