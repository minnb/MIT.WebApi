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
using System.IO;
using VCM.Shared.API.Wintel;
using WebApi.Partner.ViewModels.MBC;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Diagnostics;
using WebApi.Partner.Application.Implementation;
using WebApi.Core.AppServices;
using WebApi.Core.Shared;
using Microsoft.AspNetCore.Components.Routing;

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
        private readonly IKibanaService _kibanaService;
        private readonly ILogger<MobiCastService> _logger;
        private readonly ITransService _transService;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly string[] lstStatusAuth = { "S401", "S902" };
        public MobiCastService(
          ILogger<MobiCastService> logger,
          IKibanaService kibanaService,
          ITransService transService,
          IMemoryCacheService memoryCacheService
          )
        {
            _logger = logger;
            _transService = transService;
            _memoryCacheService = memoryCacheService;
            _kibanaService = kibanaService;
        }
        private string CallApiMBC(WebApiViewModel webApiInfo, UserMBC userMBC, object wsRequest, string func, string proxyHttp, string[] byPass, ref string checkSum)
        {
            string requestId = StringHelper.InitRequestId();
            string result = string.Empty;
            if(webApiInfo == null)
            {
                return null;
            }

            var routeApi = webApiInfo.WebRoute.Where(x => x.Name == "Routing").FirstOrDefault();
            var url_request = webApiInfo.Host + routeApi.Route.ToString();
            string errMsg = "";
            try
            {
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

                _kibanaService.LogRequest(NetwordHelper.GetHostName(), null);

                long milliseconds = 0;
                var st1 = new Stopwatch();
                st1.Start();

                var rs = api.InteractWithApiResponse(ref errMsg);

                using Stream stream = rs.GetResponseStream();
                StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
                result = streamReader.ReadToEnd();

                milliseconds = st1.ElapsedMilliseconds;
                st1.Stop();

                _kibanaService.LogResponse(url_request, NetwordHelper.GetHostName(), errMsg + "@" + result, milliseconds);
            }
            catch (Exception ex)
            {
                result = ex.Message;
                _logger.LogError(requestId + " CallApiMBC Exception: " + ex.Message.ToString());
            }
            return result;
        } 
        public async Task<ResponseClient> PurchaseCardMBC_TripleAsync(POSPurchaseCardRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (userMBC == null)
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
                        storeId = request.PosNo[..4],
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
                                    StoreNo = request.PosNo[..4],
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
            if (userMBC == null)
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
                        storeId = request.PosNo[..4],
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
            if (userMBC == null)
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
                        storeId = request.PosNo[..4],
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
            if (userMBC == null)
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
            if (userMBC == null)
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth(request.PartnerCode);
            }
            try
            {
                string function = "mb_changePaymentStatus";
                List <listSim> listSim = new List<listSim>();
                if(request.Remark!= null && request.Remark.Count > 0)
                {
                    if(request.Remark.FirstOrDefault().Remark6 != null && request.Remark.FirstOrDefault().Remark6.ToUpper() == "KIT_WINTEL_WIN99")
                    {
                        function = request.Remark.FirstOrDefault().Remark6.ToUpper();
                    }

                    foreach (var item in request.Remark)
                        listSim.Add(new listSim()
                        {
                            code = item.Remark4 ?? "",
                            serial = item.Remark5 ?? ""
                        });
                }
                if(function == "KIT_WINTEL_WIN99")
                {
                    if(request.Remark.Count > 1)
                    {
                        bool flag = false;
                        Meta meta = new Meta();
                        string lstSerial = "";
                        foreach (var item in request.Remark)
                        {
                            var wsRequest = new TripleObj()
                            {
                                wsRequest = new wsRequestSerial()
                                {
                                    serial = item.Remark5 ?? "",
                                    storeId = request.PosNo[..4],
                                    username = userMBC.username,
                                    accountId = userMBC.accountId
                                }
                            };
                            var check = await UpdateKitStatusKYC_MBC_Triple(request, wsRequest, item.Remark5 ?? "", webApiInfo, userMBC, proxyHttp, byPass);
                            if(check.Meta.Code == 200)
                            {
                                flag = true;
                                lstSerial += item.Remark5 ?? "" + "|";
                            }
                            else
                            {
                                flag = false;
                                meta = check.Meta;
                                break;
                            }
                        }
                        if (flag)
                        {
                            resultObj.Meta = ResponseHelper.MetaOK(200, lstSerial + " cập nhật thành công");
                            resultObj.Data = null;
                            return resultObj;
                        }
                        else
                        {
                            return ResponseHelper.RspNotWarning(400, meta.Message);
                        }
                    }
                    else
                    {
                        var fistSerial = request.Remark.FirstOrDefault();
                        var wsRequest = new TripleObj()
                        {
                            wsRequest = new wsRequestSerial()
                            {
                                serial = fistSerial.Remark5 ?? "",
                                storeId = request.PosNo.Substring(0, 4),
                                username = userMBC.username,
                                accountId = userMBC.accountId
                            }
                        };
                        return await UpdateKitStatusKYC_MBC_Triple(request, wsRequest, fistSerial.Remark5 ?? "", webApiInfo, userMBC, proxyHttp, byPass);
                    }
                }
                else
                {
                    string checkSum = string.Empty;
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
            if (userMBC == null)
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
                        storeId = request.PosNo[..4]
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
            if (userMBC == null)
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
        public async Task<ResponseClient> ValidateKitStatus_MBC_Triple(ValidateKitStatusRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (userMBC == null)
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth(request.PartnerCode);
            }
            try
            {
                string checkSum = string.Empty;
                string function = "validateKitStatus";
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsRequestSerial()
                    {
                        storeId = request.PosNo[..4],
                        serial = request.Serial,
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
                            string encDataString = TripleUtils.Decrypt(webApiInfo.PrivateKey, rpData.encData);
                            _logger.LogWarning("===>TripleUtils.Decrypt: " + encDataString);
                            var serialKitInfo = JsonConvert.DeserializeObject<validateKitStatusRsp>(encDataString);
                            
                            if(serialKitInfo == null)
                            {

                                return ResponseHelper.RspWarnByPartner(rpData.code, "Không có thông tin PackageName từ Wintel");
                            }

                            var mapItem = itemDto.Where(x => x.PartnerItem.ToUpper() == serialKitInfo.packageName).FirstOrDefault();

                            if (mapItem == null)
                            {

                                return ResponseHelper.RspWarnByPartner(rpData.code, serialKitInfo.packageName + " chưa cấu hình hoặc đã ngừng bán");
                            }

                            var remark = new Remark()
                            {
                                Remark1 = "",
                                Remark2 = "",
                                Remark3 = "",
                                Remark4 = request.Serial,
                                Remark5 = request.Serial,
                                Remark6 = "KIT_WINTEL_" + serialKitInfo.packageName,
                                Remark7 = serialKitInfo.packageName,
                            };
                            var infoItemPartner = new InfoItemPartner()
                            {
                                ItemNo = mapItem.ItemNo,
                                Qty = 1,
                                UnitPrice = 0,
                                DiscountAmount = 0,
                                LineAmountInclVAT = 0,
                                IsLoyalty = false
                            };
                            var item = ObjExample.MappingTransLine(mapItem, 1, 0, infoItemPartner, remark);

                            resultObj = ResponseHelper.RspOK(item);
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
                _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.Serial, request.Serial, JsonConvert.SerializeObject(resultObj), resultObj.Meta.Code == 200 ? OrderStatusEnum.Paid.ToString() : OrderStatusEnum.Failed.ToString());
                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
        public async Task<ResponseClient> UpdateKitStatusKYC_MBC_Triple(RequestUpdateOrderStatus request, TripleObj wsRequest, string serialNo, WebApiViewModel webApiInfo, UserMBC userMBC, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                string checkSum = string.Empty;
                string function = "updateKitStatusKYC";
                
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
                        _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.OrderNo, serialNo, JsonConvert.SerializeObject(resultObj), resultObj.Meta.Code == 200 ? OrderStatusEnum.OK.ToString() : OrderStatusEnum.Failed.ToString());
                    }
                }
                else
                {
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.OrderNo, serialNo, @"request timeout", ApiStatusEnum.Timeout.ToString());
                    strResponse = CallApiMBC(webApiInfo, userMBC, wsRequest, function, proxyHttp, byPass, ref checkSum);
                    if (!string.IsNullOrEmpty(strResponse))
                    {
                        var rpDataTry = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                        if (rpDataTry != null)
                        {
                            if (rpDataTry.code == "S200")
                            {
                                resultObj.Meta = ResponseHelper.MetaOK(200, rpDataTry.message);
                                resultObj.Data = null;
                            }
                            else if(rpDataTry.code == "700007")
                            {
                                resultObj.Meta = ResponseHelper.MetaOK(200, rpDataTry.message);
                                resultObj.Data = null;
                            }
                            else
                            {
                                resultObj = ResponseHelper.RspWarnByPartner(rpDataTry.code, rpDataTry.message);
                            }
                            _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.OrderNo, serialNo, JsonConvert.SerializeObject(resultObj), resultObj.Meta.Code == 200 ? OrderStatusEnum.OK.ToString() : OrderStatusEnum.Failed.ToString());
                        }
                    }
                    else
                    {
                        resultObj.Meta = ResponseHelper.RspTimeOut(request.PartnerCode);
                    }
                }
                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception UpdateKitStatusKYC_MBC_Triple: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
        public async Task<ResponseClient> CheckExtendSubscriberInfo_MBC_Triple(ValidateKitStatusRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (userMBC == null || (userMBC != null && string.IsNullOrEmpty(userMBC.jwtToken)))
            {
                userMBC = await _memoryCacheService.MBCTokenAsync(webApiInfo,proxyHttp,byPass);
                if(userMBC == null || (userMBC != null && string.IsNullOrEmpty(userMBC.jwtToken)))
                {
                    return ResponseHelper.RspNotAuth(request.PartnerCode);
                }
            }
            try
            {
                string checkSum = string.Empty;
                string function = "getSubscriberInfo";
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsRequestCheckExtendSubscriberInfo()
                    {
                        isdn = request.Serial,
                        displayedAll = true,
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
                            _logger.LogWarning(TripleUtils.Decrypt(webApiInfo.PrivateKey, rpData.encData));
                            var serialKitInfo = JsonConvert.DeserializeObject<subscriberInfoRsp>(TripleUtils.Decrypt(webApiInfo.PrivateKey, rpData.encData));
                            if (serialKitInfo == null)
                            {
                                resultObj = ResponseHelper.RspWarnByPartner(rpData.code, "Không có thông tin PackageName từ Wintel");
                            }

                            resultObj = ResponseHelper.RspOK(serialKitInfo);
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
                //_transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.Serial, request.Serial, JsonConvert.SerializeObject(resultObj), resultObj.Meta.Code == 200 ? OrderStatusEnum.Paid.ToString() : OrderStatusEnum.Failed.ToString());
                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("CheckExtendSubscriberInfo_MBC_Triple Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
        public async Task<ResponseClient> GetSingleIsdn_MBC_Triple(ValidateKitStatusRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (userMBC == null || string.IsNullOrEmpty(userMBC.jwtToken))
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth("Lỗi đăng nhập API Wintel");
            }
            try
            {
                string checkSum = string.Empty;
                string function = "mb_getSingleIsdnForPosWcm";
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsRequestUser()
                    {
                        username = userMBC.username,
                        accountId = userMBC.accountId,
                        storeId = request.PosNo[..4]
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
                            string encDataString = TripleUtils.Decrypt(webApiInfo.PrivateKey, rpData.encData);
                            _logger.LogWarning("===>TripleUtils.Decrypt: " + encDataString);
                            var dataRsp = JsonConvert.DeserializeObject<IsdnInfoResponse>(encDataString);

                            if (dataRsp == null)
                            {
                                return ResponseHelper.RspWarnByPartner(rpData.code, "Không có thông tin số điện thoại Esim");
                            }

                            resultObj = ResponseHelper.RspOK(dataRsp.isdnInfoResponse);
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
                //_transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.Serial, request.Serial, JsonConvert.SerializeObject(resultObj), resultObj.Meta.Code == 200 ? OrderStatusEnum.Paid.ToString() : OrderStatusEnum.Failed.ToString());
                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("GetSingleIsdn_MBC_Triple Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
        public async Task<ResponseClient> KeepIsdn_MBC_Triple(ValidateKitStatusRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (userMBC == null || string.IsNullOrEmpty(userMBC.jwtToken))
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth("Lỗi đăng nhập API Wintel");
            }
            if (string.IsNullOrEmpty(request.Serial))
            {
                return ResponseHelper.RspNotWarning(201, "Vui lòng chọn số điện thoại Esim");
            }
            try
            {
                string checkSum = string.Empty;
                string function = "mb_keepIsdn";
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsKeepIsdn()
                    {
                        username = userMBC.username,
                        accountId = userMBC.accountId,
                        storeId = request.PosNo.Substring(0, 4),
                        isdn = request.Serial
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
                            resultObj.Meta = ResponseHelper.MetaOK(200, rpData.code + "-" + rpData.message);
                            resultObj.Data = null;
                        }
                        else
                        {
                            resultObj = ResponseHelper.RspNotAuth(rpData.code + "-" + rpData.message);                           
                        }
                    }
                }
                else
                {
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, function, "", checkSum, JsonConvert.SerializeObject(request), ApiStatusEnum.Timeout.ToString());
                    resultObj.Meta = ResponseHelper.RspTimeOut(request.PartnerCode);
                }
                //_transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.Serial, request.Serial, JsonConvert.SerializeObject(resultObj), resultObj.Meta.Code == 200 ? OrderStatusEnum.Paid.ToString() : OrderStatusEnum.Failed.ToString());
                return resultObj;
            }
            catch (Exception ex) 
            {
                _logger.LogWarning("KeepIsdn_MBC_Triple Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
        public async Task<ResponseClient> CreateEsimOrder_MBC_Triple(CreateEsimOrderPosWcmRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (userMBC == null || string.IsNullOrEmpty(userMBC.jwtToken))
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth("Lỗi đăng nhập API Wintel");
            }
            if (string.IsNullOrEmpty(request.Serial))
            {
                return ResponseHelper.RspNotWarning(201, "Vui lòng chọn số điện thoại Esim");
            }
            try
            {
                string checkSum = string.Empty;
                string function = "mb_createEsimOrderPosWcm";
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsCreateEsimOrder()
                    {
                        username = userMBC.username,
                        accountId = userMBC.accountId,
                        storeId = request.PosNo.Substring(0, 4),
                        isdn = request.Serial,
                        packageId = request.PackageId,
                        email = request.CustEmail??"",
                        phone = request.CustPhone??""
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
                            string encDataString = TripleUtils.Decrypt(webApiInfo.PrivateKey, rpData.encData);
                            _logger.LogWarning("===>TripleUtils.Decrypt: " + encDataString);
                            var dataRsp = JsonConvert.DeserializeObject<CreateEsimOrderPosWcmRsp>(encDataString);

                            if (dataRsp == null)
                            {
                                return ResponseHelper.RspWarnByPartner(rpData.code, "Không tìm thấy QRCode cho Esim");
                            }
                            _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.OrderNo??"", checkSum, JsonConvert.SerializeObject(dataRsp), ApiStatusEnum.OK.ToString());
                            resultObj = ResponseHelper.RspOK(dataRsp);
                        }
                        else
                        {
                            resultObj = ResponseHelper.RspWarnByPartner(rpData.code, rpData.message);
                        }
                    }
                }
                else
                {
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.OrderNo ?? "", checkSum, JsonConvert.SerializeObject(request), ApiStatusEnum.Timeout.ToString());
                    resultObj.Meta = ResponseHelper.RspTimeOut(request.PartnerCode);
                }
                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("CreateEsimOrder_MBC_Triple Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
        public async Task<ResponseClient> GetListPackage_MBC_Triple(ValidateKitStatusRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (userMBC == null || string.IsNullOrEmpty(userMBC.jwtToken))
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth("Lỗi đăng nhập API Wintel");
            }
            try
            {
                string checkSum = string.Empty;
                string function = "mb_getListPackagePosWcm";
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsRequestUser()
                    {
                        username = userMBC.username,
                        accountId = userMBC.accountId,
                        storeId = request.PosNo.Substring(0, 4)
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
                            string encDataString = TripleUtils.Decrypt(webApiInfo.PrivateKey, rpData.encData);
                            _logger.LogWarning("===>TripleUtils.Decrypt: " + encDataString);
                            var lstPackage = JsonConvert.DeserializeObject<ListPackagePosWcm>(encDataString);

                            if (lstPackage == null || lstPackage.listPackagePosWcm == null)
                            {
                                return ResponseHelper.RspWarnByPartner(rpData.code, "Không có thông tin PackageName từ Wintel");
                            }
                            
                            List<DataListPackagePosWcmRsp> listPackagePOS = new List<DataListPackagePosWcmRsp>();
                            foreach(var p in lstPackage.listPackagePosWcm)
                            {
                                var checkMappingEsim = itemDto.Where(x => x.RefNo == p.packageId.ToString()).FirstOrDefault();
                                if(checkMappingEsim != null)
                                {
                                    p.itemNo = checkMappingEsim.ItemNo;
                                    p.barcode = checkMappingEsim.Barcode??"";
                                    listPackagePOS.Add(p);
                                }
                            }
                            resultObj = ResponseHelper.RspOK(listPackagePOS);
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
                //_transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.Serial, request.Serial, JsonConvert.SerializeObject(resultObj), resultObj.Meta.Code == 200 ? OrderStatusEnum.Paid.ToString() : OrderStatusEnum.Failed.ToString());
                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("GetListPackage_MBC_Triple Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }

        ////////////////////////////////////////////////////============================================////////////////////////////////////////////////////
        //Sim Vật lý
        public async Task<ResponseClient> GetListPackagePhysicalSim_Wintel_Triple(ValidateKitStatusRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (userMBC == null || string.IsNullOrEmpty(userMBC.jwtToken))
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth("Lỗi đăng nhập API Wintel");
            }
            try
            {
                string checkSum = string.Empty;
                string function = "mb_getListPackagePosWcm";
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsRequestUser()
                    {
                        username = userMBC.username,
                        accountId = userMBC.accountId,
                        storeId = request.PosNo.Substring(0, 4)
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
                            string encDataString = TripleUtils.Decrypt(webApiInfo.PrivateKey, rpData.encData);
                            _logger.LogWarning("===>TripleUtils.Decrypt: " + encDataString);
                            var lstPackage = JsonConvert.DeserializeObject<ListPackagePosWcm>(encDataString);

                            if (lstPackage == null || lstPackage.listPackagePosWcm == null)
                            {
                                return ResponseHelper.RspWarnByPartner(rpData.code, "Không có thông tin PackageName từ Wintel");
                            }

                            List<DataListPackagePosWcmRsp> listPackagePOS = new List<DataListPackagePosWcmRsp>();
                            foreach (var p in lstPackage.listPackagePosWcm)
                            {
                                var checkMappingEsim = itemDto.Where(x => x.PartnerItem.Contains("PSIM")).FirstOrDefault();
                                if (checkMappingEsim != null)
                                {
                                    p.itemNo = checkMappingEsim.ItemNo;
                                    p.barcode = checkMappingEsim.Barcode ?? "";
                                    listPackagePOS.Add(p);
                                }
                            }
                            resultObj = ResponseHelper.RspOK(listPackagePOS);
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
                //_transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.Serial, request.Serial, JsonConvert.SerializeObject(resultObj), resultObj.Meta.Code == 200 ? OrderStatusEnum.Paid.ToString() : OrderStatusEnum.Failed.ToString());
                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("GetListPackage_MBC_Triple Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
        public async Task<ResponseClient> GetSingleIsdnNoneEsim_MBC_Triple(ValidateKitStatusRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (userMBC == null || string.IsNullOrEmpty(userMBC.jwtToken))
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth("Lỗi đăng nhập API Wintel");
            }
            try
            {
                string checkSum = string.Empty;
                string function = "mb_getSingleIsdnNoneEsimForPosWcm";
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsRequestUser()
                    {
                        username = userMBC.username,
                        accountId = userMBC.accountId,
                        storeId = request.PosNo.Substring(0, 4)
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
                            string encDataString = TripleUtils.Decrypt(webApiInfo.PrivateKey, rpData.encData);
                            _logger.LogWarning("===>TripleUtils.Decrypt: " + encDataString);
                            var dataRsp = JsonConvert.DeserializeObject<IsdnInfoResponse>(encDataString);

                            if (dataRsp == null)
                            {
                                return ResponseHelper.RspWarnByPartner(rpData.code, "Không có thông tin số điện thoại sim vật lý");
                            }

                            resultObj = ResponseHelper.RspOK(dataRsp.isdnInfoResponse);
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
                //_transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.Serial, request.Serial, JsonConvert.SerializeObject(resultObj), resultObj.Meta.Code == 200 ? OrderStatusEnum.Paid.ToString() : OrderStatusEnum.Failed.ToString());
                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("GetSingleIsdnNoneEsim_MBC_Triple Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
        public async Task<ResponseClient> ValidateSimStatu_MBC_Triple(CreatePhysicalSimOrderPosWcmRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (userMBC == null || string.IsNullOrEmpty(userMBC.jwtToken))
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth("Lỗi đăng nhập API Wintel");
            }
            try
            {
                string checkSum = string.Empty;
                string function = "mb_validateSimStatus";

                var wsRequest = new TripleObj()
                {
                    wsRequest = new validateSimStatus()
                    {
                        username = userMBC.username,
                        accountId = userMBC.accountId,
                        storeId = request.PosNo.Substring(0, 4),
                        serial = request.SerialSimKIT,
                        type = "0"
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
                            strResponse = CallApiMBC(webApiInfo, await _memoryCacheService.MBCTokenAsync(webApiInfo, proxyHttp, byPass, true), request, function, proxyHttp, byPass, ref checkSum);
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
                        _transService.LoggingApi(request.PartnerCode, request.PosNo, function, "validateSimStatus", request.Serial, JsonConvert.SerializeObject(resultObj), resultObj.Meta.Code == 200 ? OrderStatusEnum.OK.ToString() : OrderStatusEnum.Failed.ToString());
                    }
                }
                else
                {
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, function, "", request.Serial, @"request timeout", ApiStatusEnum.Timeout.ToString());
                    strResponse = CallApiMBC(webApiInfo, userMBC, request, function, proxyHttp, byPass, ref checkSum);
                    if (!string.IsNullOrEmpty(strResponse))
                    {
                        var rpDataTry = JsonConvert.DeserializeObject<ResponseEncData>(strResponse);
                        if (rpDataTry != null)
                        {
                            if (rpDataTry.code == "S200")
                            {
                                resultObj.Meta = ResponseHelper.MetaOK(200, rpDataTry.message);
                                resultObj.Data = null;
                            }
                            else if (rpDataTry.code == "700007")
                            {
                                resultObj.Meta = ResponseHelper.MetaOK(200, rpDataTry.message);
                                resultObj.Data = null;
                            }
                            else
                            {
                                resultObj = ResponseHelper.RspWarnByPartner(rpDataTry.code, rpDataTry.message);
                            }
                            _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.Serial, request.Serial, JsonConvert.SerializeObject(resultObj), resultObj.Meta.Code == 200 ? OrderStatusEnum.OK.ToString() : OrderStatusEnum.Failed.ToString());
                        }
                    }
                    else
                    {
                        resultObj.Meta = ResponseHelper.RspTimeOut(request.PartnerCode);
                    }
                }
                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception UpdateKitStatusKYC_MBC_Triple: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
        public async Task<ResponseClient> CreatePhysicalSimOrder_MBC_Triple(CreatePhysicalSimOrderPosWcmRequest request, WebApiViewModel webApiInfo, UserMBC userMBC, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            if (userMBC == null || string.IsNullOrEmpty(userMBC.jwtToken))
            {
                await Task.Delay(1);
                return ResponseHelper.RspNotAuth("Lỗi đăng nhập API Wintel");
            }
            if (string.IsNullOrEmpty(request.Serial))
            {
                return ResponseHelper.RspNotWarning(201, "Vui lòng chọn số điện thoại");
            }
            try
            {
                string checkSum = string.Empty;
                string function = "mb_createPhysicalSimOrderPosWcm";
                var wsRequest = new TripleObj()
                {
                    wsRequest = new wsCreatePhysicalSimOrder()
                    {
                        username = userMBC.username,
                        accountId = userMBC.accountId,
                        storeId = request.PosNo.Substring(0, 4),
                        isdn = request.Serial??"",
                        mainPackId = request.PackageId,
                        serial = request.SerialSimKIT ?? ""
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
                            string encDataString = TripleUtils.Decrypt(webApiInfo.PrivateKey, rpData.encData);
                            _logger.LogWarning("===>TripleUtils.Decrypt: " + encDataString);
                            _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.OrderNo ?? "", request.Serial, encDataString, ApiStatusEnum.OK.ToString());

                            resultObj = ResponseHelper.RspOK(JsonConvert.DeserializeObject<rpCreatePhysicalSimOrder>(encDataString));
                        }
                        else
                        {
                            resultObj = ResponseHelper.RspWarnByPartner(rpData.code, rpData.message);
                        }
                    }
                }
                else
                {
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.OrderNo ?? "", checkSum, JsonConvert.SerializeObject(request), ApiStatusEnum.Timeout.ToString());
                    resultObj.Meta = ResponseHelper.RspTimeOut(request.PartnerCode);
                }
                return resultObj;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("CreatePhysicalSimOrder_MBC_Triple Exception: " + ex.Message.ToString());
                return ResponseHelper.RspTryCatch(ex);
            }
        }
    }
}
