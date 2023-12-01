using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Shared.API;
using VCM.Shared.Enums;
using WebApi.PriceEngine.Application.Interfaces;
using WebApi.PriceEngine.Enums;
using WebApi.PriceEngine.Models.API;
using WebApiPriceEngine.Authentication;

namespace WebApi.PriceEngine.Controllers
{
    public class PriceEngineController : BaseController
    {
        private readonly ILogger<PriceEngineController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITransactionService _transactionService;

        private string _proxyHttp = "";
        private string[] _bypassList;
        public PriceEngineController
        (
             IConfiguration configuration,
             ILogger<PriceEngineController> logger,
             ITransactionService transactionService
         )
        {
            _configuration = configuration;
            _logger = logger;
            _transactionService = transactionService;
            if (!string.IsNullOrEmpty(_configuration["WebProxy:Http"]) && !string.IsNullOrEmpty(_configuration["WebProxy:Port"]))
            {
                _proxyHttp = _configuration["WebProxy:Http"];
                _bypassList = new string[] { _configuration["WebProxy:Bypasslist"] };
            }
        }

        [HttpPost]
        [Route("api/v1/price-engine/sales-price/barcode")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS, PermissionEnum.KOS })]
        public async Task<ResponseClient> GetBarcodeSalesPriceAsync([FromBody] CheckSalesPriceRequest salesPriceRequest)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogWarning("===> request: " + JsonConvert.SerializeObject(salesPriceRequest));

                    var result = await _transactionService.CheckBarcodeSalePriceAsync(salesPriceRequest);
                    if(result != null)
                    {
                        return ResponseHelper.RspOK(result);
                    }
                    else
                    {
                        return ResponseHelper.RspNotWarning((int)PriceEngineEnum.BarcodeNotPrice, EnumHelper.ToEnumString(PriceEngineEnum.BarcodeNotPrice));
                    }
                }
                else
                {
                    return ResponseHelper.RspNotWarning((int)PriceEngineEnum.BarcodeException, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> PriceEngineController.GetBarcodeSalesPriceAsync.Exception: " + ex.Message.ToString());
                return ResponseHelper.RspMessageEnum(PriceEngineEnum.BarcodeNotFound, (int)PriceEngineEnum.BarcodeNotFound, null);
            }
        }

        [HttpPost]
        [Route("api/v1/price-engine/sales-price/items")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS, PermissionEnum.KOS })]
        public async Task<ResponseClient> GetItemSalesPriceAsync([FromBody] ItemSalesPriceRequest salesPriceRequest)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogWarning("===> request: " + JsonConvert.SerializeObject(salesPriceRequest));
                    var result = await _transactionService.CheckItemSalePriceAsync(salesPriceRequest);
                    if(result.Count > 0)
                    {
                        return ResponseHelper.RspOK(result);
                    }
                    else
                    {
                        return ResponseHelper.RspNotWarning((int)PriceEngineEnum.ItemNotPrice, EnumHelper.ToEnumString(PriceEngineEnum.ItemNotPrice));
                    }
                }
                else
                {
                    return ResponseHelper.RspNotWarning((int)PriceEngineEnum.ItemException, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> PriceEngineController.GetItemSalesPriceAsync.Exception: " + ex.Message.ToString());
                return ResponseHelper.RspMessageEnum(PriceEngineEnum.ItemNotFound, (int)PriceEngineEnum.ItemNotFound, null);
            }
        }

        [HttpPost]
        [Route("api/v1/price-engine/order")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS, PermissionEnum.KOS })]
        public async Task<ResponseClient> GetCreateOrderAsync([FromBody] TransactionRequest transactionRequest)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogWarning("===> request: " + JsonConvert.SerializeObject(transactionRequest));
                    return await _transactionService.CreateOrderAsync(transactionRequest);
                }
                else
                {
                    return ResponseHelper.RspNotWarning((int)PriceEngineEnum.OrderException, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> PriceEngineController.GetCreateOrderAsync.Exception: " + ex.Message.ToString());
                return ResponseHelper.RspMessageEnum(PriceEngineEnum.OrderException, (int)PriceEngineEnum.OrderException, null);
            }
        }
    }
}
