using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.Controllers;
using VCM.Shared.API;
using VCM.Shared.API.PhucLongV2;
using VCM.Shared.Enums;
using WebApi.Partner.Authentication;

namespace WebApi.Partner.Controllers
{
    [DisplayName("PhucLong")]
    public class MakeOrderController : BaseController
    {
        private readonly ILogger<PartnerController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPhucLongV2Service _makeOrderService;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly ITransService _transService;
        public MakeOrderController
            (
                 IConfiguration configuration,
                 ILogger<PartnerController> logger,
                 IPhucLongV2Service makeOrderService,
                 IMemoryCacheService memoryCacheService,
                 ITransService transService
            )
        {
            _configuration = configuration;
            _logger = logger;
            _makeOrderService = makeOrderService;
            _memoryCacheService = memoryCacheService;
            _transService = transService;
        }

        [HttpPost]
        [Route("api/v1/phuc-long/order/create")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.CRX })]
        public async Task<ResponseClient> CreateOrderAsync([FromBody] OrderRequestBody bodyData)
        {
            if (ModelState.IsValid)
            {
                return await _makeOrderService.CreateOrderAsync(bodyData);
            }
            else
            {
                return ResponseHelper.RspNotWarning(401, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }

        }

        [HttpGet]
        [Route("api/v1/phuc-long/order/check")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.CRX })]
        public async Task<ResponseClient> GetOrderDetailAsync([Required] string AppCode = "PLH", [Required] string OrderNo = "200101-41498-001-0001")
        {
            string requestId = StringHelper.InitRequestId();
            _logger.LogWarning("===> GetOrderDetailAsync start requestId: " + requestId + " ===> param: " + AppCode + "_" + OrderNo);
            if (!string.IsNullOrEmpty(AppCode) && !string.IsNullOrEmpty(OrderNo))
            {
                ResponseClient responseObject = new ResponseClient();
                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == "PLG").SingleOrDefault();
                var itemData = _memoryCacheService.GetItemAsync().Result?.Where(x => x.AppCode == AppCode.ToUpper()).ToList();

                switch (AppCode.ToUpper())
                {
                    case "PLH":
                        responseObject =  await _transService.GetOrderDetailCheck(AppCode, OrderNo, _webApiAirPayInfo);
                        break;
                    default:
                        responseObject = ResponseHelper.RspNotWarning(9998, AppCode + " input data is incorrect");
                        break;
                }
                return responseObject;
            }
            else
            {
                return ResponseHelper.RspNotWarning(9998, "Input data is incorrect");
            }
        }
    }
}
