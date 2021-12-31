using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Shared.API;
using VCM.Partner.API.Application.Interfaces;

namespace VCM.Partner.API.Controllers
{
    //[Authorize]
    public class TransactionController : BaseController
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IPhanoService _makeOrderService;
        private readonly IMobiCastService _mobiCastService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPhucLongService _phucLongService;
        private string _proxyHttp = "";
        private string[] _bypassList;
        public TransactionController
        (
             IConfiguration configuration,
             ILogger<TransactionController> logger,
             IMemoryCacheService memoryCacheService,
             IPhanoService makeOrderService,
             IMobiCastService mobiCastService,
             IShoppingCartService shoppingCartService,
             IPhucLongService phucLongService
         )
        {
            _configuration = configuration;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
            _makeOrderService = makeOrderService;
            _mobiCastService = mobiCastService;
            _shoppingCartService = shoppingCartService;
            _phucLongService = phucLongService;
            if (!string.IsNullOrEmpty(_configuration["WebProxy:Http"]) && !string.IsNullOrEmpty(_configuration["WebProxy:Port"]))
            {
                _proxyHttp = _configuration["WebProxy:Http"];
                _bypassList = new string[] { _configuration["WebProxy:Bypasslist"] };
            }
        }

        [HttpGet]
        //[Route("v1/api/transaction/order")]
        [Route("api/v1/transaction/order")]
        public async Task<ResponseClient> GetOrderAsync([Required] string PartnerCode = "PLG", [Required] string PosNo = "373701", [Required] string OrderNo = "1630307833376")
        {
            if (!string.IsNullOrEmpty(PartnerCode)&& !string.IsNullOrEmpty(PosNo) && !string.IsNullOrEmpty(OrderNo))
            {
                ResponseClient responseObject = new ResponseClient();
                var request = new RequestTransaction()
                {
                    PartnerCode = PartnerCode,
                    StoreNo = PosNo.Substring(0, 4),
                    PosNo = PosNo,
                    OrderNo = OrderNo
                };
                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result ? .Where(x => x.AppCode == PartnerCode.ToUpper()).SingleOrDefault();
                var itemData = _memoryCacheService.GetItemAsync().Result ? .Where(x => x.AppCode == PartnerCode.ToUpper()).ToList();

                switch (PartnerCode.ToUpper())
                {
                    case "PLG":
                        responseObject = await _phucLongService.GetOrderDetailPhucLong(request, _webApiAirPayInfo, itemData);
                        break;
                    case "PNP":
                        responseObject = _makeOrderService.GetSalesOrderPhano(request, _webApiAirPayInfo, itemData,  _proxyHttp, _bypassList);
                        break;
                    case "MBC":
                        //serial: 8984090xxxxx maxlength = 20
                        if(request.OrderNo.Length == 20 && request.OrderNo.Substring(0,5) == "89840")
                        {
                            responseObject = await _mobiCastService.GetSerialMBC_Triple(request, _webApiAirPayInfo, await _memoryCacheService.MBCTokenAsync(_webApiAirPayInfo, _proxyHttp, _bypassList), itemData, _proxyHttp, _bypassList);
                        }
                        else
                        {
                            responseObject = await _mobiCastService.GetBillDetailMBC_Triple(request, _webApiAirPayInfo, await _memoryCacheService.MBCTokenAsync(_webApiAirPayInfo, _proxyHttp, _bypassList), itemData, _proxyHttp, _bypassList);
                        }
                        break;
                    case "WMT":
                        responseObject = await _shoppingCartService.GetBillDetail_WMTAsync(request, _webApiAirPayInfo, _proxyHttp, _bypassList);
                        break;
                    default:
                        responseObject = ResponseHelper.RspNotWarning(9998, PartnerCode + " input data is incorrect");
                        break;
                }
                return responseObject;
            }
            else
            {
                return ResponseHelper.RspNotWarning(9998, "Input data is incorrect");
            }
        }

        [HttpPost]
        //[Route("v1/api/transaction/list")]
        [Route("api/v1/transaction/order/list")]
        public async Task<ResponseClient> GetListOrderAsync([FromBody] RequestListOrderPOS request)
        {

            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();
                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result ? .Where(x => x.AppCode == request.PartnerCode.ToUpper()).SingleOrDefault();
                switch (request.PartnerCode.ToUpper())
                {
                    case "PLG":
                        responseObject = await _phucLongService.GetOrderListPhucLong(request, _webApiAirPayInfo);
                        break;
                    case "MBC":
                        responseObject = await _mobiCastService.GetListBillMBC_Triple(request, _webApiAirPayInfo, await _memoryCacheService.MBCTokenAsync(_webApiAirPayInfo, _proxyHttp, _bypassList), _proxyHttp, _bypassList);
                        break;
                    case "WMT":
                        responseObject = await _shoppingCartService.GetListBill_WMTAsync(request, _webApiAirPayInfo, _proxyHttp, _bypassList);
                        break;
                    default:
                        responseObject = ResponseHelper.RspNotWarning(9998, request.PartnerCode + " input data is incorrect");
                        break;
                }
                return responseObject;
            }
            else
            {
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPut]
        //[Route("v1/api/transaction/update-order-status")]
        [Route("api/v1/transaction/order/update-status")]
        public async Task<ResponseClient> UpdateOrderStatusAsync([FromBody] RequestUpdateOrderStatus request)
        {
            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();

                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result ? .Where(x => x.AppCode == request.PartnerCode.ToUpper()).SingleOrDefault();
                switch (request.PartnerCode.ToUpper())
                {
                    case "PLG":
                        responseObject = await _phucLongService.UpdateStatusOrderPhucLong(request, _webApiAirPayInfo);
                        break;
                    case "PNP":
                        responseObject = _makeOrderService.UpdateSalesOrderPhano(request, _webApiAirPayInfo, _proxyHttp, _bypassList);
                        break;
                    case "WMT":
                        responseObject = await _shoppingCartService.UpdateStatusOrder_WMTAsync(request, _webApiAirPayInfo, _proxyHttp, _bypassList);
                        break;
                    case "MBC":
                        if(request.Status == 1)
                        {
                            responseObject = await _mobiCastService.UpdateStatusOrderMBC_Triple(request, _webApiAirPayInfo, await _memoryCacheService.MBCTokenAsync(_webApiAirPayInfo, _proxyHttp, _bypassList), _proxyHttp, _bypassList);
                        }
                        else if(request.Status == 11)
                        {
                            responseObject = await _mobiCastService.CancelOrderMBC_TripleAsync(request, _webApiAirPayInfo, await _memoryCacheService.MBCTokenAsync(_webApiAirPayInfo, _proxyHttp, _bypassList), _proxyHttp, _bypassList);
                        }                  
                        break;

                }

                //responseObject = request.PartnerCode.ToUpper() switch
                //{
                //    "PLG" => await _phucLongService.UpdateStatusOrderPhucLong(request, _webApiAirPayInfo),
                //    "PNP" => _makeOrderService.UpdateSalesOrderPhano(request, _webApiAirPayInfo, _proxyHttp, _bypassList),
                //    "MBC" => await _mobiCastService.UpdateStatusOrderMBC_Triple(request, _webApiAirPayInfo, await _memoryCacheService.MBCTokenAsync(_webApiAirPayInfo, _proxyHttp, _bypassList), _proxyHttp, _bypassList),
                //    "WMT" => await _shoppingCartService.UpdateStatusOrder_WMTAsync(request, _webApiAirPayInfo, _proxyHttp, _bypassList),
                //    _ => responseObject = ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString())
                //};
                return responseObject;
            }
            else
            {
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        
    }
}
