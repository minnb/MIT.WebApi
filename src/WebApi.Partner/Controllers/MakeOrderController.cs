using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.Controllers;
using VCM.Shared.API;
using VCM.Shared.API.MakeOrder;


namespace WebApi.Partner.Controllers
{
    public class MakeOrderController : BaseController
    {
        private readonly ILogger<PartnerController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMakeOrderService _makeOrderService;

        public MakeOrderController
            (
                 IConfiguration configuration,
                 ILogger<PartnerController> logger,
                 IMakeOrderService makeOrderService
            )
        {
            _configuration = configuration;
            _logger = logger;
            _makeOrderService = makeOrderService;
        }

        [HttpPost]
        [Route("api/v1/make-order/create")]
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
    }
}
