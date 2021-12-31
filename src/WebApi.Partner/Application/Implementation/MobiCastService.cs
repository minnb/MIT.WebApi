using Microsoft.Extensions.Logging;
using MIT.Utils.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VCM.Common.Helpers;
using VCM.Shared.Partner;
using VCM.Shared.API;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Partner.API.ViewModels.MBC;
using System.Threading.Tasks;
using VCM.Shared.Entity.Partner;
using VCM.Partner.API.Common.Const;
using VCM.Shared.Dtos.POS;
using VCM.Shared.Enums;

namespace VCM.Partner.API.Application.Implementation
{
    public class MobiCastService : IMobiCastService
    {
        /*
         * publicKey: checksum
         * privateKey: key
         * description: merchantId
         * username: username
         * password: password
         * */
        private readonly ILogger<MobiCastService> _logger;
        private readonly ITransService _transService;
        private readonly IMemoryCacheService _memoryCacheService;
        //private readonly string _shopId_MBC = "2333";
        private readonly string[] lstStatusAuth = { "S401", "S902" };
        public MobiCastService(
          ILogger<MobiCastService> logger,
          ITransService transService,
          IMemoryCacheService memoryCacheService
          )
        {
            _logger = logger;
            _transService = transService;
            _memoryCacheService = memoryCacheService;
        }
        private string CallApiMBC(WebApiViewModel webApiInfo, UserMBC userMBC, object wsRequest, string func, string proxyHttp, string[] byPass, ref string checkSum)
        {
            string result = string.Empty;
            try
            {
                var routeApi = webApiInfo.WebRoute.Where(x => x.Name == "Routing").FirstOrDefault();
                var url_request = webApiInfo.Host + routeApi.Route.ToString();

                string encData = TripleUtils.Encrypt(webApiInfo.PrivateKey, JsonConvert.SerializeObject(wsRequest).ToString());
                checkSum = TripleUtils.ToMd5(Encoding.UTF8.GetBytes(func + routeApi.Version + webApiInfo.Description + encData + webApiInfo.PublicKey));

                var createRequest = new RoutingRequest()
                {
                    function = func,
                    checksum = checkSum,
                    version = routeApi.Version,
                    merchantId = webApiInfo.Description,
                    encData = encData
                };

                _logger.LogWarning(func + " - request: " + JsonConvert.SerializeObject(createRequest));
                ApiHelper api = new ApiHelper(
                    url_request,
                    "",
                    userMBC.jwtToken,
                    "POST",
                    JsonConvert.SerializeObject(createRequest),
                    true,
                    proxyHttp,
                    byPass
                    );

                result = api.InteractWithApi();
                _logger.LogWarning(func + " - response: " + createRequest.checksum + " @result: " + result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception: " + ex.Message.ToString());
            }

            return result;
        } 
        public async Task<ResponseClient> PurchaseCardMBC_TripleAsync(POSPurchaseCardRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (string.IsNullOrEmpty(userMBC.jwtToken))
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth(request.PartnerCode);
            }
            try
            {
                string checkSum = string.Empty;
                string function = "mb_createVouchForPartner";
                var requestId = Guid.NewGuid().ToString().Replace("-", "");
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsRequestPurchaseCard()
                    {
                        transId = requestId, //request.OrderNo,
                        username = userMBC.username,
                        accountId = userMBC.accountId,
                        quantity = request.Quantity,
                        value = request.PurchaseValue,
                        storeId = request.PosNo.Substring(0, 4),
                        shopId = webApiInfo.WebRoute.Where(x => x.Name == "Routing").FirstOrDefault().Notes.ToString()
                    }
                };
                string strResponse = CallApiMBC(webApiInfo, userMBC, wsRequest, function, proxyHttp, byPass, ref checkSum);
                //_logger.LogWarning(function + " response: " + strResponse);
                if (!string.IsNullOrEmpty(strResponse))
                {
                    var rpData = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                    if (rpData != null)
                    {
                        if (lstStatusAuth.Contains(rpData.code))
                        {
                            strResponse = CallApiMBC(webApiInfo, await _memoryCacheService.MBCTokenAsync(webApiInfo, proxyHttp, byPass, true), wsRequest, function, proxyHttp, byPass, ref checkSum);
                            rpData = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                        }
                        if (rpData.code == "S200")
                        {
                            resultObj.Meta = ResponseHelper.MetaOK(200, rpData.message);

                            var purchaseData = JsonConvert.DeserializeObject<RspPurchaseCard>(TripleUtils.Decrypt(webApiInfo.PrivateKey, rpData.encData));
                            if (purchaseData != null)
                            {
                                List<TransLineDto> transLines = new List<TransLineDto>();
                                if (purchaseData.voucherList.Count > 0)
                                {
                                    var mapItem = itemDto.Where(x => x.PartnerItem == "PURCHASE_CARD").FirstOrDefault();
                                    int i = 0;
                                    foreach (var item in purchaseData.voucherList)
                                    {
                                        i++;
                                        var remark = new Remark()
                                        {
                                            Remark1 = item.code,
                                            Remark2 = item.value,
                                            Remark3 = item.voucherId,
                                            Remark4 = item.serial
                                        };
                                        decimal unitPrice = string.IsNullOrEmpty(item.value) ? 0 : decimal.Parse(item.value);
                                        var infoItemPartner = new InfoItemPartner()
                                        {
                                            ItemNo = mapItem.ItemNo,
                                            Qty = 1,
                                            UnitPrice = unitPrice,
                                            DiscountAmount = 0,
                                            LineAmountInclVAT = unitPrice,
                                            IsLoyalty = false
                                        };
                                        transLines.Add(ObjExample.MappingTransLine(mapItem, i, 0, infoItemPartner, remark));
                                    }

                                    //header
                                    resultObj.Data = new TransHeaderDto()
                                    {
                                        AppCode = request.PartnerCode,
                                        OrderNo = request.OrderNo,
                                        OrderTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                        CustNo = "",
                                        CustName = "",
                                        CustPhone = "",
                                        CustAddress = "",
                                        CustNote = "",
                                        DeliveryType = 1,
                                        CardMember = "",
                                        TotalAmount = request.Quantity * request.PurchaseValue,
                                        PaymentAmount = 0,
                                        Status = 0,
                                        IsPromotion = false,
                                        RefNo = requestId,
                                        CashierId="",
                                        CashierName="",
                                        TransLine = transLines,
                                        TransPaymentEntry = null
                                    };
                                }

                                _transService.AddTransRaw(new TransRaw()
                                {
                                    Id = Guid.NewGuid(),
                                    AppCode = request.PartnerCode,
                                    StoreNo = request.PosNo.Substring(0, 4),
                                    OrderNo = request.OrderNo,
                                    ServiceType = function,
                                    ReferenceNo = requestId,
                                    RawData = JsonConvert.SerializeObject(purchaseData),
                                    UpdateFlg = "N",
                                    CrtDate = DateTime.Now,
                                    CrtUser = request.PosNo,
                                    IPAddress = System.Environment.MachineName.ToString()
                                });
                            }
                        }
                        else
                        {
                            resultObj = ResponseHelper.RspWarnByPartner(rpData.code, rpData.message);
                        }
                    }
                }
                else
                {
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, function, "", checkSum, JsonConvert.SerializeObject(request), ApiStatusEnum.Timeout.ToString());
                    resultObj.Meta = ResponseHelper.RspTimeOut(request.PartnerCode);
                }
                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        } 
        public async Task<ResponseClient> GetListBillMBC_Triple(RequestListOrderPOS request, WebApiViewModel webApiInfo, UserMBC userMBC, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (string.IsNullOrEmpty(userMBC.jwtToken))
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth(request.PartnerCode);
            }
            try
            {
                string checkSum = string.Empty;
                string function = "mb_getListBill";
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsRequestLstBill()
                    {
                        storeId = request.PosNo.Substring(0, 4),
                        username = userMBC.username,
                        accountId = userMBC.accountId
                    }
                };
                string strResponse = CallApiMBC(webApiInfo, userMBC, wsRequest, function, proxyHttp, byPass, ref checkSum);
                //_logger.LogWarning(function + " response: " + strResponse);
                if (!string.IsNullOrEmpty(strResponse))
                {
                    var rpData = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                    if (rpData != null)
                    {
                        if (lstStatusAuth.Contains(rpData.code))
                        {
                            strResponse = CallApiMBC(webApiInfo, await _memoryCacheService.MBCTokenAsync(webApiInfo, proxyHttp, byPass, true), wsRequest, function, proxyHttp, byPass, ref checkSum);
                            rpData = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                        }
                        if (rpData.code == "S200")
                        {
                            resultObj.Meta = ResponseHelper.MetaOK(200, rpData.message);

                            //_logger.LogWarning(TripleUtils.Decrypt(webApiInfo.PrivateKey, rpData.encData));
                            var listBill = JsonConvert.DeserializeObject<ListBill>(TripleUtils.Decrypt(webApiInfo.PrivateKey, rpData.encData));

                            List<ResponseOrderList> rspListOrders = new List<ResponseOrderList>();
                            if (listBill != null)
                            {
                                foreach (var item in listBill.listBill)
                                {
                                    rspListOrders.Add(new ResponseOrderList()
                                    {
                                        PartnerCode = request.PartnerCode,
                                        StoreNo = item.storeId.ToString(),
                                        OrderNo = item.billCode,
                                        OrderDate = item.createdAt,
                                        CustName = item.customerName,
                                        CustPhone = item.customerPhone,
                                        Status = 0,
                                        CashierId = "",
                                        CashierName = "",
                                        TotalItem = 1,
                                        PaymentAmount = item.totalPrice,
                                        Remark = new Remark()
                                        {
                                            Remark5 = item.requireSerial == true ? "REQUIRED_SERIAL": "NO_SERIAL"
                                        }
                                    });
                                }

                                resultObj.Data = rspListOrders;
                            }
                            else
                            {
                                resultObj.Data = null;
                            }
                        }
                        else
                        {
                            resultObj = ResponseHelper.RspWarnByPartner(rpData.code, rpData.message);
                        }
                    }
                }
                else
                {
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, function, "", checkSum, JsonConvert.SerializeObject(request), ApiStatusEnum.Timeout.ToString());
                    resultObj.Meta = ResponseHelper.RspTimeOut(request.PartnerCode);
                }
                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
        public async Task<ResponseClient> GetBillDetailMBC_Triple(RequestTransaction request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (string.IsNullOrEmpty(userMBC.jwtToken))
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth(request.PartnerCode);
            }
            try
            {
                string checkSum = string.Empty;
                string function = "mb_getBillDetail";
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsRequestBillDetail()
                    {
                        storeId = request.PosNo.Substring(0, 4),
                        billCode = request.OrderNo,
                        username = userMBC.username,
                        accountId = userMBC.accountId
                    }
                };
                string strResponse = CallApiMBC(webApiInfo, userMBC, wsRequest, function, proxyHttp, byPass, ref checkSum);
                //_logger.LogWarning(function + " response: " + strResponse);
                if (!string.IsNullOrEmpty(strResponse))
                {
                    var rpData = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                    if (rpData != null)
                    {
                        if (lstStatusAuth.Contains(rpData.code))
                        {
                            strResponse = CallApiMBC(webApiInfo, await _memoryCacheService.MBCTokenAsync(webApiInfo, proxyHttp, byPass, true), wsRequest, function, proxyHttp, byPass, ref checkSum);
                            rpData = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                        }
                        if (rpData.code == "S200")
                        {
                            resultObj.Meta = ResponseHelper.MetaOK(200, rpData.message);

                            var billData = JsonConvert.DeserializeObject<BillDetail>(TripleUtils.Decrypt(webApiInfo.PrivateKey, rpData.encData));
                            //var billData = JsonConvert.DeserializeObject<BillDetail>(JsonConvert.SerializeObject(rp.wsResponse));
                            if (billData != null)
                            {
                                List<TransLineDto> transLines = new List<TransLineDto>();
                                if (billData.lines.Count > 0)
                                {

                                    int i = 0;
                                    foreach (var item in billData.lines)
                                    {
                                        var mapItem = itemDto.Where(x => x.PartnerItem == item.productCode && x.AppCode == request.PartnerCode).FirstOrDefault();
                                        i++;
                                        var remark = new Remark()
                                        {
                                            Remark1 = item.phoneNumber,
                                            Remark2 = item.productCode,
                                            Remark3 = item.productName,
                                            Remark4 = item.productNumber,
                                            Remark5 = item.requireSerial == true ? "REQUIRED_SERIAL" : "NO_SERIAL"
                                        };
                                        var infoItemPartner = new InfoItemPartner()
                                        {
                                            ItemNo = item.productCode,
                                            Qty = 1,
                                            UnitPrice = item.price,
                                            DiscountAmount = 0,
                                            LineAmountInclVAT = item.totalPrice,
                                            IsLoyalty = false
                                        };
                                        transLines.Add(ObjExample.MappingTransLine(mapItem, i, 0, infoItemPartner, remark));
                                    }

                                    //header
                                    resultObj.Data = new TransHeaderDto()
                                    {
                                        AppCode = request.PartnerCode,
                                        OrderNo = billData.header.billCode,
                                        OrderTime = billData.header.createdAt,
                                        CustNo = "",
                                        CustName = billData.header.customerName,
                                        CustPhone = billData.header.customerPhone,
                                        CustAddress = "",
                                        CustNote = billData.header.requireSerial == true ? "REQUIRED_SERIAL" : "NO_SERIAL",
                                        DeliveryType = 1,
                                        CardMember = "",
                                        TotalAmount = billData.lines.Sum(x => x.totalPrice),
                                        PaymentAmount = 0,
                                        Status = billData.paymentStatus,
                                        IsPromotion = false,
                                        RefNo = "",
                                        TransLine = transLines,
                                        TransPaymentEntry = null
                                    };
                                }

                                _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.OrderNo, checkSum, JsonConvert.SerializeObject(billData), ApiStatusEnum.OK.ToString());

                            }
                        }
                        else
                        {
                            resultObj = ResponseHelper.RspWarnByPartner(rpData.code, rpData.message);
                        }
                    }
                }
                else
                {
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, function, "", checkSum, JsonConvert.SerializeObject(request), ApiStatusEnum.Timeout.ToString());
                    resultObj.Meta = ResponseHelper.RspTimeOut(request.PartnerCode);
                }
                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
        public async Task<ResponseClient> GetSerialMBC_Triple(RequestTransaction request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (string.IsNullOrEmpty(userMBC.jwtToken))
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth(request.PartnerCode);
            }
            try
            {
                string checkSum = string.Empty;
                string function = "mb_getBillBySerial";
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsRequestSerial()
                    {
                        storeId = request.PosNo.Substring(0, 4),
                        serial = request.OrderNo,
                        username = userMBC.username,
                        accountId = userMBC.accountId
                    }
                };
                string strResponse = CallApiMBC(webApiInfo, userMBC, wsRequest, function, proxyHttp, byPass, ref checkSum);

                if (!string.IsNullOrEmpty(strResponse))
                {
                    var rpData = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                    if (rpData != null)
                    {
                        if (lstStatusAuth.Contains(rpData.code))
                        {
                            strResponse = CallApiMBC(webApiInfo, await _memoryCacheService.MBCTokenAsync(webApiInfo, proxyHttp, byPass, true), wsRequest, function, proxyHttp, byPass, ref checkSum);
                            rpData = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                        }
                        if (rpData.code == "S200")
                        {
                            resultObj.Meta = ResponseHelper.MetaOK(200, rpData.message);

                            var billData = JsonConvert.DeserializeObject<BillDetail>(TripleUtils.Decrypt(webApiInfo.PrivateKey, rpData.encData));
                            //var billData = JsonConvert.DeserializeObject<BillDetail>(JsonConvert.SerializeObject(rp.wsResponse));
                            if (billData != null)
                            {
                                List<TransLineDto> transLines = new List<TransLineDto>();
                                if (billData.lines.Count > 0)
                                {
                                    //var mapItem = itemDto.FirstOrDefault();
                                    int i = 0;
                                    foreach (var item in billData.lines)
                                    {
                                        i++;
                                        var mapItem = itemDto.Where(x => x.PartnerItem == item.productCode).FirstOrDefault();
                                        var remark = new Remark()
                                        {
                                            Remark1 = item.phoneNumber,
                                            Remark2 = item.productCode,
                                            Remark3 = item.productName,
                                            Remark4 = item.productNumber,
                                            Remark5 = item.requireSerial == true ? "REQUIRED_SERIAL" : "NO_SERIAL"
                                        };

                                        List<TransDiscountEntryDto> discountEntries = new List<TransDiscountEntryDto>();
                                        if (item.vouchers != null)
                                        {
                                            int j = 0;
                                            foreach (var v in item.vouchers)
                                            {
                                                j++;
                                                discountEntries.Add(new TransDiscountEntryDto()
                                                {
                                                    LineNo = j,
                                                    ParentLineNo = i,
                                                    OfferNo = v.vouchernumber,
                                                    DiscountAmount = 0,
                                                    Qty = 1,
                                                    OfferType = "VOUCHER",
                                                    Note = v.comp
                                                });
                                            }
                                        }

                                        var infoItemPartner = new InfoItemPartner()
                                        {
                                            ItemNo = item.productCode,
                                            UnitPrice = item.price,
                                            DiscountAmount = 0,
                                            LineAmountInclVAT = item.totalPrice,
                                            IsLoyalty = false,
                                            Qty = item.quantity
                                        };

                                        transLines.Add(ObjExample.MappingTransLine(mapItem, i,0, infoItemPartner, remark, discountEntries));
                                    }

                                    //header
                                    resultObj.Data = new TransHeaderDto()
                                    {
                                        AppCode = request.PartnerCode,
                                        OrderNo = billData.header.billCode,
                                        OrderTime = billData.header.createdAt,
                                        CustNo = "",
                                        CustName = billData.header.customerName,
                                        CustPhone = billData.header.customerPhone,
                                        CustAddress = "",
                                        CustNote = billData.header.requireSerial == true ? "REQUIRED_SERIAL" : "NO_SERIAL",
                                        DeliveryType = 1,
                                        CardMember = "",
                                        TotalAmount = billData.lines.Sum(x => x.totalPrice),
                                        PaymentAmount = 0,
                                        Status = billData.paymentStatus,
                                        IsPromotion = false,
                                        RefNo = "",
                                        TransLine = transLines,
                                        TransPaymentEntry = null
                                    };
                                }
                            }
                        }
                        else
                        {
                            resultObj = ResponseHelper.RspWarnByPartner(rpData.code, rpData.message);
                        }
                    }
                }
                else
                {
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, function, "", checkSum, JsonConvert.SerializeObject(request), ApiStatusEnum.Timeout.ToString());
                    resultObj.Meta = ResponseHelper.RspTimeOut(request.PartnerCode);
                }
                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
        public async Task<ResponseClient> UpdateStatusOrderMBC_Triple(RequestUpdateOrderStatus request, WebApiViewModel webApiInfo, UserMBC userMBC, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (string.IsNullOrEmpty(userMBC.jwtToken))
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth(request.PartnerCode);
            }
            try
            {
                List<listSim> listSim = new List<listSim>();
                if(request.Remark!= null && request.Remark.Count > 0)
                {
                    foreach (var item in request.Remark)
                        listSim.Add(new listSim()
                        {
                            code = item.Remark4,
                            serial = item.Remark5
                        });
                }

                string checkSum = string.Empty;
                string function = "mb_changePaymentStatus";
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsRequestUpdateStatus()
                    {
                        storeId = request.PosNo.Substring(0, 4),
                        billCode = request.OrderNo,
                        paymentStatus = request.Status,
                        username = userMBC.username,
                        accountId = userMBC.accountId,
                        listSim = listSim
                    }
                };
                string strResponse = CallApiMBC(webApiInfo, userMBC, wsRequest, function, proxyHttp, byPass, ref checkSum);
                if (!string.IsNullOrEmpty(strResponse))
                {
                    var rpData = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                    if (rpData != null)
                    {
                        if (lstStatusAuth.Contains(rpData.code))
                        {
                            strResponse = CallApiMBC(webApiInfo, await _memoryCacheService.MBCTokenAsync(webApiInfo, proxyHttp, byPass, true), wsRequest, function, proxyHttp, byPass, ref checkSum);
                            rpData = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                        }

                        if (rpData.code == "S200")
                        {
                            resultObj.Meta = ResponseHelper.MetaOK(200, rpData.message);
                            resultObj.Data = null;
                        }
                        else
                        {
                            resultObj = ResponseHelper.RspWarnByPartner(rpData.code, rpData.message);
                        }

                        _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.OrderNo, checkSum, JsonConvert.SerializeObject(rpData), OrderStatusEnum.Paid.ToString());
                    }
                }
                else
                {
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.OrderNo, checkSum, @"request timeout", ApiStatusEnum.Timeout.ToString());

                    resultObj.Meta = ResponseHelper.RspTimeOut(request.PartnerCode);
                }

                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
        public async Task<ResponseClient> TopupMBC_Triple(POSTopupRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (string.IsNullOrEmpty(userMBC.jwtToken))
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth(request.PartnerCode);
            }
            try
            {
                string requestId = Guid.NewGuid().ToString().Replace("-", "");
                string checkSum = string.Empty;
                string function = "mb_rechargeEwallet";
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsRequestTopupCard()
                    {
                        transId = requestId, // request.OrderNo,
                        username = userMBC.username,
                        accountId = userMBC.accountId,
                        msisdn = request.Receiver,
                        value = request.TopupValue,
                        shopId = webApiInfo.WebRoute.Where(x => x.Name == "Routing").FirstOrDefault().Notes.ToString(),
                        storeId = request.PosNo.Substring(0, 4)
                    }
                };
                string strResponse = CallApiMBC(webApiInfo, userMBC, wsRequest, function, proxyHttp, byPass, ref checkSum);
                //_logger.LogWarning(function + " response: " + strResponse);
                if (!string.IsNullOrEmpty(strResponse))
                {
                    var rpData = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                    if (rpData != null)
                    {
                        if (lstStatusAuth.Contains(rpData.code))
                        {
                            strResponse = CallApiMBC(webApiInfo, await _memoryCacheService.MBCTokenAsync(webApiInfo, proxyHttp, byPass, true), wsRequest, function, proxyHttp, byPass, ref checkSum);
                            rpData = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                        }
                        if (rpData.code == "S200")
                        {
                            resultObj.Meta = ResponseHelper.MetaOK(200, rpData.message);
                            var topupData = JsonConvert.DeserializeObject<ResponseTopup>(TripleUtils.Decrypt(webApiInfo.PrivateKey, rpData.encData));
                            if (topupData != null)
                            {
                                var mapItem = itemDto.Where(x => x.PartnerItem.ToUpper() == "TOPUP_CARD").FirstOrDefault();

                                var itemPartner = new InfoItemPartner()
                                {
                                    ItemNo = mapItem.ItemNo,
                                    UnitPrice = request.TopupValue,
                                    Qty = 1,
                                    IsLoyalty = false,
                                    DiscountAmount = 0,
                                    LineAmountInclVAT = request.TopupValue
                                };

                                List<TransLineDto> transLines = new List<TransLineDto>
                                {
                                    ObjExample.MappingTransLine(mapItem, 1, 0, itemPartner, null)
                                };

                                resultObj.Data = new TransHeaderDto()
                                {
                                    AppCode = request.PartnerCode,
                                    OrderNo = request.OrderNo,
                                    OrderTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    CustNo = "",
                                    CustName = "",
                                    CustPhone = request.Receiver,
                                    CustAddress = "",
                                    CustNote = "",
                                    DeliveryType = 1,
                                    CardMember = "",
                                    TotalAmount = request.TopupValue,
                                    PaymentAmount = 0,
                                    Status = 0,
                                    IsPromotion = false,
                                    RefNo = requestId,
                                    TransLine = transLines,
                                    TransPaymentEntry = null
                                };

                                _transService.AddTransRaw(new TransRaw()
                                {
                                    Id = Guid.NewGuid(),
                                    AppCode = request.PartnerCode,
                                    StoreNo = request.PosNo.Substring(0, 4),
                                    OrderNo = request.OrderNo,
                                    ServiceType = function,
                                    ReferenceNo = requestId,
                                    RawData = JsonConvert.SerializeObject(topupData),
                                    UpdateFlg = "N",
                                    CrtDate = DateTime.Now,
                                    CrtUser = request.PosNo,
                                    IPAddress = System.Environment.MachineName.ToString()
                                });
                            }
                        }
                        else
                        {
                            resultObj = ResponseHelper.RspWarnByPartner(rpData.code, rpData.message);
                        }
                    }
                }
                else
                {
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, function, "", checkSum, JsonConvert.SerializeObject(request), ApiStatusEnum.Timeout.ToString());
                    resultObj.Meta = ResponseHelper.RspTimeOut(request.PartnerCode);
                }
                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
        public async Task<ResponseClient> CancelOrderMBC_TripleAsync(RequestUpdateOrderStatus request, WebApiViewModel webApiInfo, UserMBC userMBC, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (string.IsNullOrEmpty(userMBC.jwtToken))
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth(request.PartnerCode);
            }
            try
            {
                string checkSum = string.Empty;
                string function = "tsc_cancelPurchaseOrder";
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsRequestCancelOrder()
                    {
                        storeId = request.PosNo.Substring(0, 4),
                        billCode = request.OrderNo,
                        username = userMBC.username,
                        accountId = userMBC.accountId,
                    }
                };
                string strResponse = CallApiMBC(webApiInfo, userMBC, wsRequest, function, proxyHttp, byPass, ref checkSum);
                if (!string.IsNullOrEmpty(strResponse))
                {
                    var rpData = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                    if (rpData != null)
                    {
                        if (lstStatusAuth.Contains(rpData.code))
                        {
                            strResponse = CallApiMBC(webApiInfo, await _memoryCacheService.MBCTokenAsync(webApiInfo, proxyHttp, byPass, true), wsRequest, function, proxyHttp, byPass, ref checkSum);
                            rpData = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                        }

                        if (rpData.code == "S200")
                        {
                            resultObj.Meta = ResponseHelper.MetaOK(200, rpData.message);
                            resultObj.Data = null;
                        }
                        else
                        {
                            resultObj = ResponseHelper.RspWarnByPartner(rpData.code, rpData.message);
                        }

                        _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.OrderNo, checkSum, JsonConvert.SerializeObject(rpData), OrderStatusEnum.Paid.ToString());
                    }
                }
                else
                {
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.OrderNo, checkSum, @"request timeout", ApiStatusEnum.Timeout.ToString());

                    resultObj.Meta = ResponseHelper.RspTimeOut(request.PartnerCode);
                }

                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogError("CancelOrderMBC_TripleAsync Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
    }
}
