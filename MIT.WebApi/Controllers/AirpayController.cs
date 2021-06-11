using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MIT.Dtos;
using MIT.EntityFrameworkCore;
using MIT.Utils;
using MIT.Utils.Helpers;
using MIT.WebApi.GPAY.Application.Interfaces;
using MIT.WebApi.GPAY.Controllers;
using MIT.WebApi.GPAY.ViewModels;
using MIT.WebApi.GPAY.ViewModels.AirPay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.WebApi.Controllers
{
    //[Authorize]
    public class AirpayController : BaseController
    {
        private readonly string _appCode = "APY";
        private readonly ILogger<AirpayController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IAirPayService _airPayService;
        private WebApiViewModel _webApiAirPayInfo = new();
        public AirpayController(
            IConfiguration configuration,
            ILogger<AirpayController> logger,
            IMemoryCacheService memoryCacheService,
            IAirPayService airPayService
            )
        {
            _configuration = configuration;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
            _webApiAirPayInfo = _memoryCacheService.GetDataWebApi().Where(x => x.AppCode == _appCode).SingleOrDefault();
            _airPayService = airPayService;
        }

        [HttpPost]
        [Route("v1/api/air-pay/purchase-card-v2")]
        public ResponseObject PurchaseCardV2([FromBody] POSPurchaseCardRequest bodyData)
        {
            if (ModelState.IsValid)
            {
                return _airPayService.CallPurchaseCardV2(_webApiAirPayInfo, bodyData);
            }
            else
            {
                return new ResponseObject()
                {
                    Meta = new Meta { Code = 9999, Message = ModelState.Values.First().Errors[0].ErrorMessage.ToString() },
                    Data = null
                };
            }
        }

    }
}
