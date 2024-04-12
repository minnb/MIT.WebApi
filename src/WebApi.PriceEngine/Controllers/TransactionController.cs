using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Common.Validation;
using VCM.Shared.API;
using VCM.Shared.API.PhucLongV2;
using VCM.Shared.Enums;
using WebApi.PriceEngine.Application.Interfaces;
using WebApi.PriceEngine.Enums;
using WebApi.PriceEngine.Models.API;
using WebApi.PriceEngine.Models.API.KIOS;
using WebApiPriceEngine.Authentication;
namespace WebApi.PriceEngine.Controllers
{
    public class TransactionController : BaseController
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITransactionService _transactionService;
        private readonly IBillPaymentService _billPaymentService;
        private string _proxyHttp = "";
        private string[] _bypassList;
        public TransactionController
        (
             IConfiguration configuration,
             ILogger<TransactionController> logger,
             ITransactionService transactionService,
             IBillPaymentService billPaymentService
         )
        {
            _configuration = configuration;
            _logger = logger;
            _transactionService = transactionService;
            _billPaymentService = billPaymentService;
            if (!string.IsNullOrEmpty(_configuration["WebProxy:Http"]) && !string.IsNullOrEmpty(_configuration["WebProxy:Port"]))
            {
                _proxyHttp = _configuration["WebProxy:Http"];
                _bypassList = new string[] { _configuration["WebProxy:Bypasslist"] };
            }
        }

        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("api/v1/transaction/order/detail/{OrderNo}")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS, PermissionEnum.KOS })]
        public async Task<ResponseClient> GetOrderDetail([Required] string OrderNo = "153521220800148")
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogWarning("===> scan: " + OrderNo);
                    string error_message = string.Empty;
                    string AppCode = "KIOS";
                    return await _billPaymentService.OrderDetailKIOS(GetUserNameAPI(), AppCode, OrderNo);
                }
                else
                {
                    return ResponseHelper.RspNotWarning((int)PriceEngineEnum.OrderException, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> TransactionController.GetOrderDetail.Exception: " + ex.Message.ToString());
                return ResponseHelper.RspMessageEnum(PriceEngineEnum.OrderInsDatabase, (int)PriceEngineEnum.OrderInsDatabase, null);
            }
        }

        [HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("api/v1/transaction/order/payment")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS, PermissionEnum.KOS })]
        public async Task<ResponseClient> UpdatePaymentKIOS([FromBody] TransPaymentKios payments)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string AppCode = "KIOS";
                    _logger.LogWarning("===> payment request: " + JsonConvert.SerializeObject(payments));
                    string error_message = string.Empty;
                    return await _billPaymentService.UpdatePaymentKIOS(GetUserNameAPI(), AppCode, payments);
                }
                else
                {
                    return ResponseHelper.RspNotWarning((int)PriceEngineEnum.OrderException, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> TransactionController.UpdatePaymentKIOS.Exception: " + ex.Message.ToString());
                return ResponseHelper.RspMessageEnum(PriceEngineEnum.OrderInsDatabase, (int)PriceEngineEnum.OrderInsDatabase, null);
            }
        }
        
        [HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("api/v1/transaction/order")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS, PermissionEnum.KOS })]
        public async Task<ResponseClient> PostOrderData([FromBody] OrderRequestBody rawDataRequest)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogWarning("===> request: " + JsonConvert.SerializeObject(rawDataRequest));
                    string error_message = string.Empty;
                    if (!OrderValidation.OrderRequest(rawDataRequest, ref error_message))
                    {
                        return ResponseHelper.RspNotWarning((int)PriceEngineEnum.OrderException, error_message);
                    }
                    else
                    {
                        return await _transactionService.PutOrderDataAsync(GetUserNameAPI(), rawDataRequest);
                    }
                }
                else
                {
                    return ResponseHelper.RspNotWarning((int)PriceEngineEnum.OrderException, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> TransactionController.PutOrderData.Exception: " + ex.Message.ToString());
                return ResponseHelper.RspMessageEnum(PriceEngineEnum.OrderInsDatabase, (int)PriceEngineEnum.OrderInsDatabase, null);
            }
        }

        [HttpPut]
        [Microsoft.AspNetCore.Mvc.Route("api/v1/transaction/order")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS, PermissionEnum.KOS })]
        public async Task<ResponseClient> PutOrderData([FromBody] OrderRequestBody rawDataRequest)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogWarning("===> request: " + JsonConvert.SerializeObject(rawDataRequest));
                    string error_message = string.Empty;
                    if (!OrderValidation.OrderRequest(rawDataRequest, ref error_message))
                    {
                        return ResponseHelper.RspNotWarning((int)PriceEngineEnum.OrderException, error_message);
                    }
                    else
                    {
                        return await _transactionService.PutOrderDataAsync(GetUserNameAPI(), rawDataRequest);
                    }
                }
                else
                {
                    return ResponseHelper.RspNotWarning((int)PriceEngineEnum.OrderException, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> TransactionController.PutOrderData.Exception: " + ex.Message.ToString());
                return ResponseHelper.RspMessageEnum(PriceEngineEnum.OrderInsDatabase, (int)PriceEngineEnum.OrderInsDatabase, null);
            }
        }

        [HttpPut]
        [Microsoft.AspNetCore.Mvc.Route("api/v1/transaction/raw")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS, PermissionEnum.KOS })]
        public async Task<ResponseClient> PutTmpRawData([FromBody] RawDataRequest rawDataRequest)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogWarning("===> request: " + JsonConvert.SerializeObject(rawDataRequest));
                    return await _transactionService.SaveTmpTransRawAsync(GetUserNameAPI(), rawDataRequest);
                }
                else
                {
                    return ResponseHelper.RspNotWarning((int)PriceEngineEnum.OrderException, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> TransactionController.PutOrderData.Exception: " + ex.Message.ToString());
                return ResponseHelper.RspMessageEnum(PriceEngineEnum.OrderInsDatabase, (int)PriceEngineEnum.OrderInsDatabase, null);
            }
        }
        private string GetUserNameAPI()
        {
           return ConvertHelper.GetUserNameAuthorization(Request.Headers["Authorization"]);
        }
    }
}
