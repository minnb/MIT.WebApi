using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Shared.API;
using VCM.Partner.API.Application.Interfaces;
using VCM.Shared.Enums;
using WebApi.Partner.Authentication;
using VCM.Shared.API.PhucLongV2;
using WebApi.Partner.ViewModels.Partner;
using Microsoft.AspNetCore.Authorization;

namespace VCM.Partner.API.Controllers
{
    public class TransactionController : BaseController
    {
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IPhanoService _phanoService;
        private readonly IMobiCastService _mobiCastService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPhucLongService _phucLongService;
        private readonly IPhucLongV2Service _phucLongV2Service;
        private readonly ITransService _transService;
        private readonly IShopeeService _shopeeService;
        private string _proxyHttp = "";
        private string[] _bypassList = {};
        private readonly string[] _WebWinmart = { "WMT", "VMP", "WCM" };
        private readonly string requestId = string.Empty;
        public TransactionController
        (
             IMemoryCacheService memoryCacheService,
             IPhanoService phanoService,
             IMobiCastService mobiCastService,
             IShoppingCartService shoppingCartService,
             IPhucLongService phucLongService,
             IPhucLongV2Service phucLongV2Service,
             ITransService transService,
             IShopeeService shopeeService
        )
        {
            _phanoService = phanoService;
            _memoryCacheService = memoryCacheService;
            _phucLongV2Service = phucLongV2Service;
            _mobiCastService = mobiCastService;
            _shoppingCartService = shoppingCartService;
            _phucLongService = phucLongService;
            _transService = transService;
            _shopeeService = shopeeService;
            requestId = StringHelper.InitRequestId();
        }

        [HttpPost]
        [Route("api/v1/transaction/order/create")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.CRX })]
        public async Task<ResponseClient> CreateOrderAsync([FromBody] OrderRequestBody bodyData)
        {
            if (ModelState.IsValid)
            {
                return await _phucLongV2Service.CreateOrderAsync(bodyData);
            }
            else
            {
                return ResponseHelper.RspNotWarning(401, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }

        }

        [HttpGet]
        [Route("api/v1/transaction/order")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> GetOrderAsync([Required] string PartnerCode = "PLH", [Required] string PosNo = "373701", [Required] string OrderNo = "1630307833376", string AppCode = "NOWFOOD")
        {
            if (!string.IsNullOrEmpty(PartnerCode) && !string.IsNullOrEmpty(PosNo) && !string.IsNullOrEmpty(OrderNo))
            {
                ResponseClient responseObject = new ResponseClient();
                var request = new RequestTransaction()
                {
                    PartnerCode = PartnerCode,
                    AppCode = PartnerCode == "WCM" ? "VMP" : AppCode,
                    StoreNo = PosNo[..4],
                    PosNo = PosNo,
                    OrderNo = OrderNo
                };
                var _webApiAirPayInfo =  _memoryCacheService.GetDataWebApiAsync().Result ? .Where(x => x.AppCode == PartnerCode.ToUpper()).FirstOrDefault();
                if (_webApiAirPayInfo == null)
                {
                    return ResponseHelper.RspNotWarning(9998, "AppCode: " + request.AppCode + " không tồn tại");
                }
                if (request.PartnerCode == "WMT")
                {
                    request.PartnerCode = "WCM";
                }

                var itemData = _memoryCacheService.GetItemAsync().Result ? .Where(x => x.AppCode == PartnerCode.ToUpper()).ToList();
                _proxyHttp = _webApiAirPayInfo.HttpProxy;
                _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };
                switch (PartnerCode.ToUpper())
                {
                    case "PLH":
                        if (string.IsNullOrEmpty(AppCode))
                        {
                            responseObject = await _phucLongV2Service.GetOrderDetail(request, itemData);
                        }
                        else
                        {
                            switch (AppCode.ToUpper())
                            {
                                case "CRX":
                                    responseObject = await _phucLongV2Service.GetOrderDetail(request, itemData);
                                    break;
                                case "NOWFOOD":
                                    _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == AppCode.ToUpper()).FirstOrDefault();
                                    responseObject = await _shopeeService.GetOrderDetail(request, _webApiAirPayInfo, itemData, _webApiAirPayInfo.HttpProxy, new string[] { _webApiAirPayInfo.Bypasslist });
                                    break;
                                default:
                                    responseObject = ResponseHelper.RspNotWarning(9998, "AppCode: " + request.AppCode + " không đúng");
                                    break;
                            }
                        }
                        break;
                    case "PLG":
                        responseObject = await _phucLongService.GetOrderDetailPhucLong(request, _webApiAirPayInfo, itemData);
                        break;
                    case "PNP":
                        responseObject = _phanoService.GetSalesOrderPhano(request, _webApiAirPayInfo, itemData,  _proxyHttp, _bypassList);
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
                    case "WCM":
                        request.AppCode = "VMP";
                        var vatCode = _memoryCacheService.GetVATCodeAsync().Result?.Where(x => x.AppCode == PartnerCode.ToUpper()).ToList();
                        if(vatCode == null)
                        {
                            responseObject = ResponseHelper.RspNotWarning(9998, "Chưa khai báo TaxGroupCode, vui lòng liên hệ IT");
                        }

                        responseObject = request.AppCode.ToUpper() switch
                        {
                            //case "WMP":
                            //    responseObject = await _shoppingCartService.GetBillDetail_WCM_Async(request, _webApiAirPayInfo, _proxyHttp, _bypassList);
                            //    break;
                            "VMP" => await _shoppingCartService.GetBillDetail_WCM_Async(request, _webApiAirPayInfo, vatCode, _proxyHttp, _bypassList, requestId),
                            "WMT" => ResponseHelper.RspNotWarning(9998, "WMT - Winmart không có đơn hàng online"),
                            _ => ResponseHelper.RspNotWarning(9998, "AppCode: " + request.AppCode + " không đúng"),
                        };
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
        [Route("api/v1/transaction/order/list")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> GetListOrderAsync([FromBody] RequestListOrderPOS request)
        {
            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();
                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result ? .Where(x => x.AppCode == request.PartnerCode.ToUpper()).FirstOrDefault();
                if (_webApiAirPayInfo == null)
                {
                    return ResponseHelper.RspNotWarning(9998, "AppCode: " + request.AppCode + " không tồn tại");
                }
                if(request.PartnerCode == "WMT")
                {
                    request.PartnerCode = "WCM";
                }

                _proxyHttp = _webApiAirPayInfo.HttpProxy;
                _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };
                switch (request.PartnerCode.ToUpper())
                {
                    case "PLG":
                        responseObject = await _phucLongService.GetOrderListPhucLong(request, _webApiAirPayInfo);
                        break;
                    case "PLH":
                        if (string.IsNullOrEmpty(request.AppCode))
                        {
                            responseObject = await _phucLongV2Service.GetOrderList(request);
                        }
                        else
                        {
                            switch (request.AppCode.ToUpper())
                            {
                                case "CRX":
                                    responseObject = await _phucLongV2Service.GetOrderList(request);
                                    break;
                                case "NOWFOOD":
                                    _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == request.AppCode.ToUpper()).FirstOrDefault();
                                    responseObject = await _shopeeService.GetOrderList(request, _webApiAirPayInfo, _webApiAirPayInfo.HttpProxy, new string[] { _webApiAirPayInfo.Bypasslist });
                                    break;
                                default:
                                    responseObject = ResponseHelper.RspNotWarning(9998,"AppCode: " + request.AppCode + " không đúng");
                                    break;
                            }
                        }
                        break;
                    case "MBC":
                        responseObject = await _mobiCastService.GetListBillMBC_Triple(request, _webApiAirPayInfo, await _memoryCacheService.MBCTokenAsync(_webApiAirPayInfo, _proxyHttp, _bypassList), _proxyHttp, _bypassList);
                        break;
                    case "WCM":
                        request.AppCode = "VMP";
                        responseObject = request.AppCode.ToUpper() switch
                        {
                            //case "WMP":
                            //    responseObject = await _shoppingCartService.GetListBill_WCM_Async(request, _webApiAirPayInfo, _proxyHttp, _bypassList);
                            //    break;
                            "VMP" => await _shoppingCartService.GetListBill_WCM_Async(request, _webApiAirPayInfo, _proxyHttp, _bypassList, requestId),
                            "WMT" => await _shoppingCartService.GetListBill_WCM_Async(request, _webApiAirPayInfo, _proxyHttp, _bypassList, requestId),
                            _ => ResponseHelper.RspNotWarning(9998, "AppCode: " + request.AppCode + " không đúng"),
                        };
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

        [HttpPost]
        [Route("api/v1/transaction/order/count")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> CountOrderAsync([FromBody] CountOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();
                string AppCode = request.AppCode;
                if (request.PartnerCode == "WMT")
                {
                    request.PartnerCode = "WCM";
                }
                if (AppCode == "PLH" || request.PartnerCode == "PLH")
                {
                    AppCode = "PLH_WEB";
                }
                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == AppCode).FirstOrDefault();
                if (_webApiAirPayInfo == null)
                {
                    return ResponseHelper.RspNotWarning(9998, "AppCode: " + request.AppCode + " không tồn tại");
                }

                _proxyHttp = _webApiAirPayInfo.HttpProxy;
                _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };
                switch (request.PartnerCode.ToUpper())
                {
                    case "WCM":
                        request.AppCode = "VMP";
                        switch (request.AppCode.ToUpper())
                        {
                            case "VMP":
                                responseObject = await _shoppingCartService.CountedOrder_WCM(request, _webApiAirPayInfo, requestId);
                                break;
                            case "WMT":
                                responseObject = await _shoppingCartService.CountedOrder_WCM(request, _webApiAirPayInfo, requestId);
                                break;
                            default:
                                responseObject = ResponseHelper.RspNotWarning(9998, "AppCode: " + request.AppCode + " không đúng");
                                break;
                        }
                        break;
                    case "PLH":
                        switch (request.AppCode.ToUpper())
                        {
                            case "CRX":
                                responseObject = await _phucLongV2Service.CountedOrder(request, _webApiAirPayInfo);
                                break;
                            case "NOWFOOD":
                                _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == request.AppCode.ToUpper()).FirstOrDefault();
                                responseObject = await _shopeeService.CountedOrder(request, _webApiAirPayInfo);
                                break;
                            default:
                                responseObject = ResponseHelper.RspNotWarning(9998, "AppCode: " + request.AppCode + " không đúng");
                                break;
                        }
                        break;
                    default:
                        responseObject = ResponseHelper.RspNotWarning(9998, "PartnerCode: " + request.PartnerCode + " không đúng");
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
        [Route("api/v1/transaction/order/update-status")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> UpdateOrderStatusAsync([FromBody] RequestUpdateOrderStatus request)
        {
            request.AppCode ??= "";
            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();

                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result ? .Where(x => x.AppCode == request.PartnerCode.ToUpper()).FirstOrDefault();
                if (_webApiAirPayInfo == null)
                {
                    return ResponseHelper.RspNotWarning(9998, "AppCode: " + request.AppCode + " không tồn tại");
                }

                _proxyHttp = _webApiAirPayInfo.HttpProxy;
                _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };

                switch (request.PartnerCode.ToUpper())
                {
                    case "PLH":
                        if (string.IsNullOrEmpty(request.AppCode))
                        {
                            responseObject = await _phucLongV2Service.UpdateStatusOrder(request, _webApiAirPayInfo, _proxyHttp, _bypassList);
                        }
                        else
                        {
                            switch (request.AppCode.ToUpper())
                            {
                                case "CRX":
                                    responseObject = await _phucLongV2Service.UpdateStatusOrder(request, _webApiAirPayInfo, _proxyHttp, _bypassList);
                                    break;
                                case "NOWFOOD":
                                    _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == request.AppCode.ToUpper()).FirstOrDefault();
                                    responseObject = _shopeeService.UpdateStatusOrder(request, _webApiAirPayInfo, _webApiAirPayInfo.HttpProxy, new string[] { _webApiAirPayInfo.Bypasslist });
                                    break;
                                default:
                                    responseObject = ResponseHelper.RspNotWarning(9998, "AppCode: " + request.AppCode + " không đúng");
                                    break;
                            }
                        }                       
                        break;
                    case "PLG":
                        responseObject = await _phucLongService.UpdateStatusOrderPhucLong(request, _webApiAirPayInfo);
                        break;
                    case "PNP":
                        responseObject = _phanoService.UpdateSalesOrderPhano(request, _webApiAirPayInfo, _proxyHttp, _bypassList);
                        break;
                    case "WCM":
                        request.AppCode = "VMP";
                        switch (request.AppCode.ToUpper())
                        {
                            case "VMP":
                                responseObject = await _shoppingCartService.UpdateStatusOrder_WCM_Async(request, _webApiAirPayInfo, _proxyHttp, _bypassList, requestId);
                                break;
                            default:
                                responseObject = ResponseHelper.RspNotWarning(9998, "AppCode: " + request.AppCode + " không đúng");
                                break;
                        }
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
                return responseObject;
            }
            else
            {
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpPost]
        [Route("api/v1/transaction/order/refund")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> OrderRefundAsync([FromBody] SalesReturnRequest request)
        {
            if (ModelState.IsValid)
            {
                ResponseClient responseObject = new ResponseClient();
                request.AppCode ??= "";

                if (_WebWinmart.Contains(request.PartnerCode))
                {
                    request.AppCode = "WCM";
                }
                else if (request.PartnerCode == "PLH")
                {
                    request.AppCode = "PLH_WEB";
                }

                var _webApiAirPayInfo = _memoryCacheService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == request.AppCode).FirstOrDefault();
                if (_webApiAirPayInfo == null)
                {
                    return ResponseHelper.RspNotWarning(9998, "AppCode: " + request.AppCode + " không tồn tại");
                }

                _proxyHttp = _webApiAirPayInfo.HttpProxy;
                _bypassList = new string[] { _webApiAirPayInfo.Bypasslist };
                switch (request.PartnerCode.ToUpper())
                {
                    case "WCM":
                        request.AppCode = "VMP";
                        responseObject = request.AppCode.ToUpper() switch
                        {
                            "VMP" => await _shoppingCartService.RefundDetails_WebOnline(request, _webApiAirPayInfo),
                            "WMT" => await _shoppingCartService.RefundDetails_WebOnline(request, _webApiAirPayInfo),
                            _ => ResponseHelper.RspNotWarning(9998, "AppCode: " + request.AppCode + " không đúng"),
                        };
                        break;
                    case "PLH":
                        responseObject = request.AppCode.ToUpper() switch
                        {
                            "PLH_WEB" => await _shoppingCartService.RefundDetails_WebOnline(request, _webApiAirPayInfo),
                            _ => ResponseHelper.RspNotWarning(9998, "AppCode: " + request.AppCode + " không đúng"),
                        };
                        break;
                    default:
                        responseObject = ResponseHelper.RspNotWarning(9998, "PartnerCode: " + request.PartnerCode + " không đúng");
                        break;
                }
                return responseObject;
            }
            else
            {
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpGet]
        [Route("api/v1/transaction/order/campaign")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> CheckCampaigns([Required] string appCode, [Required] string orderNo)
        {
            if (ModelState.IsValid)
            {
                return await _transService.CheckCampaign(appCode, orderNo);
            }
            else
            {
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpGet]
        [Route("api/v1/transaction/check-sum")]
        [PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<ResponseClient> TotalSalesWCM()
        {
            if (ModelState.IsValid)
            {
                return await _transService.TotalSalesByDate("");
            }
            else
            {
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

        [HttpGet]
        [Route("api/v2/transaction/order")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public ResponseClient CheckOrderDetail([Required] string appCode, [Required] string orderNo)
        {
            if (ModelState.IsValid)
            {
                return _transService.GetOrderDetail(appCode, orderNo);
            }
            else
            {
                return ResponseHelper.RspNotWarning(9998, ModelState.Values.First().Errors[0].ErrorMessage.ToString());
            }
        }

    }
}
