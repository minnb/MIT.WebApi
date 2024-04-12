using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Shared.API;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Partner.API.ViewModels.WMT;
using VCM.Shared.Entity.Partner;
using VCM.Partner.API.Common.Const;
using VCM.Shared.Dtos.POS;
using VCM.Shared.API.PhucLongV2;
using VCM.Shared.Enums;
using VCM.Common.Validation;
using WebApi.Partner.ViewModels.Partner;
using VCM.Shared.Entity.Crownx;
using WebApi.Core.Common.Constants;
using Microsoft.EntityFrameworkCore.Internal;
using WebApi.Core.AppServices;
using WebApi.Core.Shared;
using System.Diagnostics;

namespace VCM.Partner.API.Application.Implementation
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly ILogger<ShoppingCartService> _logger;
        private readonly ITransService _transService;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IKibanaService _kibanaService;
        public ShoppingCartService
        (
              ILogger<ShoppingCartService> logger,
              ITransService transService,
              IMemoryCacheService memoryCacheService,
              IKibanaService kibanaService
        )
        {
            _logger = logger;
            _transService = transService;
            _memoryCacheService = memoryCacheService;
            _kibanaService = kibanaService;
    }
        private async Task<IRestResponse> CallApi_CRX_Async(WebApiViewModel webApiInfo, string function, Method method, object bodyData, string param)
        {
            IRestResponse response = new RestResponse();
            string router = webApiInfo.WebRoute.Where(x => x.Name == function).FirstOrDefault().Route;
            if (!string.IsNullOrEmpty(param))
            {
                router += param;
            }

            try
            {
                RestClient client = new RestClient(webApiInfo.Host)
                {
                    Timeout = 45000
                };

                RestRequest restRequest = new RestRequest(router, method);
                restRequest.AddHeader("Accept", "application/json");
                restRequest.AddHeader("Authorization", webApiInfo.Description + " " + Convert.ToBase64String(Encoding.UTF8.GetBytes(webApiInfo.UserName + ":" + webApiInfo.Password)));

                if (bodyData != null)
                {
                    restRequest.AddJsonBody(bodyData);
                }

                long responseTime = 0;
                var st1 = new Stopwatch();
                st1.Start();

                response = await client.ExecuteAsync(restRequest);

                responseTime = st1.ElapsedMilliseconds;
                st1.Stop();

                _kibanaService.LogResponse(webApiInfo.Host + router, NetwordHelper.GetHostName(), response.Content, responseTime);               
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> Exception CallApi_CRX_Async " + webApiInfo.AppCode + "-" + function + ": " + response.Content.ToString());
                response.ResponseStatus = ResponseStatus.Error;
                response.ErrorMessage = ex.Message;
                response.ErrorException = ex;
            }           
            return response;
        }
        public async Task<ResponseClient> GetListBill_WCM_Async(RequestListOrderPOS request, WebApiViewModel webApiInfo, string proxyHttp, string[] byPass, string requestId = "")
        {
            ResponseClient resultObj = new ResponseClient();
            string[] statusOrder = { "New", "Confirmed" };
            string function = "ListOrder";
            var bodyData = new RqListOrderWCM()
            {
                OrderCode = "",
                StoreId = request.PosNo[..4].ToString(),
                ChannelId = request.AppCode.ToUpper(),
                ChainId = request.AppCode.ToUpper(),
                PageNumber = 1,
                PageSize = 100,
                OrderByDesc = true,
                OrderBy = ""

            };
            //call api
            var response = await CallApi_CRX_Async(webApiInfo, function, Method.POST, bodyData, null);
            try
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var rpData = JsonConvert.DeserializeObject<RspListOrderWMT>(response.Content);
                    List<ResponseListOrderPOS> rspListOrders = new List<ResponseListOrderPOS>();
                    if (rpData.Data.Count > 0)
                    {
                        if (rpData.Data.Count > 0)
                        {
                            foreach (var item in rpData.Data)
                            {
                                if (item.Status == "Confirmed")
                                {
                                    rspListOrders.Add(new ResponseListOrderPOS()
                                    {
                                        PartnerCode = request.PartnerCode,
                                        StoreNo = request.PosNo[..4],
                                        OrderNo = item.OrderCode,
                                        OrderDate = item.OrderTime,
                                        CustName = item.ReceiverName,
                                        CustPhone = item.ReceiverPhone,
                                        CustAddress = item.ReceiverAddress,
                                        Status = StatusConst.StatusWebWCM()[item.Status],
                                        CashierId = item.Status ?? "",
                                        CashierName = "",
                                        TotalItem = 0,
                                        PaymentAmount = item.TotalPayment,
                                        Remark = null
                                    });
                                }
                            }

                        }
                    }
                    resultObj = ResponseHelper.RspOK(rspListOrders);
                }
                else
                {
                    resultObj = RestResponseNotOK(response, requestId);
                }
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                resultObj.Data = null;
                _logger.LogWarning("GetListBill_WCM_Async Exception requestId: " + requestId + " |Message: " + ex.Message.ToString());
            }
            return resultObj;
        }
        public async Task<ResponseClient> GetBillDetail_WMT_Async(RequestTransaction request, WebApiViewModel webApiInfo, string proxyHttp, string[] byPass, string requestId = "")
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                string[] lstVinID = { "8888", "6666" };
                string[] statusOrder = { "Confirmed" };

                string function = "OrderDetail";
                string param = "/" + request.OrderNo;
                var bodyData = new RqDetailOrderWMT()
                {
                    StoreId = request.PosNo.Substring(0, 4),
                    Status = statusOrder,
                    ChannelId = "VMP"
                };

                var response = await CallApi_CRX_Async(webApiInfo, function, Method.GET, null, param);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var rpData = JsonConvert.DeserializeObject<RspOrderDetailWMT>(response.Content);
                    _logger.LogWarning(webApiInfo.AppCode + "-" + function + ": " + JsonConvert.SerializeObject(rpData));
                    if (rpData != null)
                    {
                        List<TransLineDto> transLines = new List<TransLineDto>();
                        if (rpData.Data != null & rpData.Data.Items.Count > 0)
                        {
                            if (rpData.Data.Items.Count > 0)
                            {
                                int i = 0;
                                foreach (var item in rpData.Data.Items)
                                {
                                    i++;
                                    decimal vatPercent = VatConst.MappingTax()[item.VatGroup];
                                    decimal netAmount = MathHelper.CalcNetAmount(item.Amount, vatPercent);

                                    string barcode = "AAAAAAAA";
                                    if (!string.IsNullOrEmpty(item.BarcodeNo))
                                    {
                                        barcode = item.BarcodeNo;
                                    }

                                    transLines.Add(new TransLineDto()
                                    {
                                        LineNo = i,
                                        ParentLineNo = i,
                                        ItemNo = item.ItemNo,
                                        ItemName = item.ItemName,
                                        Barcode = barcode,
                                        Uom = item.UOM,
                                        UnitPrice = item.Price,
                                        Qty = item.Quantity,
                                        DiscountAmount = item.DiscountAmount,
                                        VatGroup = item.VatGroup,
                                        VatPercent = vatPercent,
                                        VatAmount = item.Amount - netAmount,
                                        LineAmountExcVAT = netAmount,
                                        LineAmountIncVAT = item.Amount,
                                        IsLoyalty = true,
                                        ItemType = item.ItemType,
                                        Remark = null,
                                        TransDiscountEntry = null
                                    });
                                }
                            }

                            string vinIdNumber = "";
                            if (!string.IsNullOrEmpty(rpData.Data.VINIDNumber) && rpData.Data.VINIDNumber.ToString().Length >= 4)
                            {
                                vinIdNumber = lstVinID.Contains(rpData.Data.VINIDNumber.ToString().Substring(0, 4)) ? rpData.Data.VINIDNumber.ToString() : "";
                            }
                            resultObj.Meta = new Meta()
                            {
                                Code = 200,
                                Message = "Successfully"
                            };
                            resultObj.Data = new TransHeaderDto()
                            {
                                AppCode = request.PartnerCode,
                                OrderNo = rpData.Data.OrderCode,
                                OrderTime = rpData.Data.OrderTime,
                                CustNo = "",
                                CustName = rpData.Data.ReceiverName,
                                CustPhone = rpData.Data.ReceiverPhone,
                                CustAddress = rpData.Data.ReceiverAddress,
                                CustNote = "",
                                DeliveryType = 1,
                                CardMember = vinIdNumber,
                                TotalAmount = rpData.Data.BillAmount,
                                PaymentAmount = 0,
                                Status = StatusConst.StatusWebWCM()[rpData.Data.Status],
                                RefNo = "",
                                TransLine = transLines,
                                TransPaymentEntry = null
                            };
                        }
                        else
                        {
                            resultObj.Meta = new Meta()
                            {
                                Code = 201,
                                Message = "Không tìm thấy đơn hàng"
                            };
                        }
                        _transService.AddTransRaw(new TransRaw()
                        {
                            Id = Guid.NewGuid(),
                            AppCode = request.PartnerCode,
                            StoreNo = request.PosNo.Substring(0, 4),
                            OrderNo = request.OrderNo,
                            ServiceType = function,
                            ReferenceNo = "",
                            RawData = JsonConvert.SerializeObject(rpData),
                            UpdateFlg = "N",
                            CrtDate = DateTime.Now,
                            CrtUser = request.PosNo,
                            IPAddress = ""
                        });
                    }
                }
                else
                {
                    resultObj = RestResponseNotOK(response);
                }
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning("Exception: " + ex.Message.ToString());
            }
            return resultObj;
        }
        public async Task<ResponseClient> UpdateStatusOrder_WCM_Async(RequestUpdateOrderStatus request, WebApiViewModel webApiInfo, string proxyHttp, string[] byPass, string requestId = "")
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                string param = @"/" + request.OrderNo + @"/update-status";
                string function = "UpdateStatusOrder";

                var bodyData = new RqUpdateStatusOrderWMT()
                {
                    Status = request.Status == 1 ? "Delivered" : "Canceled"
                };

                var response = await CallApi_CRX_Async(webApiInfo, function, Method.PUT, bodyData, param);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        var rpData = JsonConvert.DeserializeObject<RspUpdateStatusOrderCRX>(response.Content);
                        _logger.LogWarning(webApiInfo.AppCode + "-" + function + ": " + JsonConvert.SerializeObject(rpData));
                        if (rpData != null)
                        {
                            resultObj.Meta = new Meta()
                            {
                                Code = 200,
                                Message = rpData.Message
                            };
                            resultObj.Data = null;
                            _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.OrderNo, request.OrderNo, JsonConvert.SerializeObject(resultObj), resultObj.Meta.Code == 200 ? OrderStatusEnum.OK.ToString() : OrderStatusEnum.Failed.ToString());
                        }
                    }
                }
                else
                {
                    resultObj = RestResponseNotOK(response, requestId);
                    _logger.LogWarning("UpdateStatusOrder_WCM_Async ERROR: " + response.StatusDescription.ToString());
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, function, request.OrderNo, request.OrderNo, response.StatusDescription.ToString(), OrderStatusEnum.Failed.ToString());
                }
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning("UpdateStatusOrder_WCMAsync Exception: " + ex.Message.ToString());
            }
            return resultObj;
        }
        public async Task<ResponseClient> GetBillDetail_WCM_Async(RequestTransaction request, WebApiViewModel webApiInfo, List<VAT> vATs, string proxyHttp, string[] byPass, string requestId = "")
        {
            ResponseClient resultObj = new ResponseClient();
            string function = "OrderDetail";
            string param = "/" + request.OrderNo;
            var response = await CallApi_CRX_Async(webApiInfo, function, Method.GET, null, param);
            try
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogWarning(webApiInfo.AppCode + "-" + function + " OrderNo: " + request.OrderNo + " response: " + response.Content.ToString());
                    var rpData = JsonConvert.DeserializeObject<RspOrderDetail_WCM>(response.Content);
                    string error_message = string.Empty;
                    _logger.LogWarning(JsonConvert.SerializeObject(rpData));
                    if (!OrderValidation.OrderResponse(rpData.Data, ref error_message))
                    {
                        return ResponseHelper.RspNotWarning(201, error_message);
                    }

                    //_logger.LogWarning(webApiInfo.AppCode + "-" + function + ": " + JsonConvert.SerializeObject(rpData));
                    if (rpData.Data != null)
                    {
                        _logger.LogWarning(rpData.Data.OrderNo + " response: " + JsonConvert.SerializeObject(rpData));
                        List<TransLineDto> transLines = new List<TransLineDto>();
                        if (rpData.Data.Items != null & rpData.Data.Items.Count > 0)
                        {
                            int i = 0;
                            foreach (var item in rpData.Data.Items)
                            {
                                i++;
                                decimal netAmount = MathHelper.CalcNetAmount(item.LineAmount, item.VatPercent);
                                
                                //Không có barcode thì báo lỗi
                                if (string.IsNullOrEmpty(item.Barcode))
                                {
                                    return new ResponseClient()
                                    {
                                        Meta = new Meta()
                                        {
                                            Code = 201,
                                            Message = string.Format("Mã sản phẩm {0} - đơn vị tính {1} không có Barcode", item.ItemNo, item.Uom)
                                        },
                                        Data = null
                                    };
                                }
                                string barcode = item.Barcode;
                                List<TransDiscountEntryDto> discountEntry = new List<TransDiscountEntryDto>();
                                if (item.DiscountEntry != null && item.DiscountEntry.Count > 0)
                                {
                                    foreach (var d in item.DiscountEntry)
                                    {
                                        discountEntry.Add(new TransDiscountEntryDto()
                                        {
                                            LineNo = d.LineId,
                                            ParentLineNo = d.LineId,
                                            OfferNo = d.OfferNo ?? "WEB_WMP",
                                            OfferType = (d.OfferType ?? "WEB_WMP") != "WEB_WMP" ? d.OfferType : "WEB_WMP",
                                            DiscountAmount = d.DiscountAmount,
                                            Qty = d.Quantity,
                                            Note = d.Note ?? "ONLINE_WEB_WMP"
                                        });
                                    }
                                }

                                var isLoyalty = false;
                                if (item.Loyalty != null && item.Loyalty.Count > 0)
                                {
                                    isLoyalty = true;
                                }

                                if (!vATs.Select(x => x.TaxGroupCode).ToArray().Contains(item.TaxGroupCode))
                                {
                                    return new ResponseClient()
                                    {
                                        Meta = new Meta()
                                        {
                                            Code = 201,
                                            Message = string.Format("TaxGroupCode = {0} của sản phẩm không đúng", item.TaxGroupCode)
                                        },
                                        Data = null
                                    };
                                }

                                transLines.Add(new TransLineDto()
                                {
                                    LineNo = i,
                                    ParentLineNo = i,
                                    ItemNo = item.ItemNo,
                                    ItemName = item.ItemName,
                                    ItemName2 = item.ItemName,
                                    Barcode = barcode,
                                    Uom = item.Uom,
                                    UnitPrice = item.UnitPrice,
                                    Qty = item.Quantity,
                                    DiscountAmount = item.DiscountAmount,
                                    VatGroup = item.TaxGroupCode,
                                    VatPercent = vATs.FirstOrDefault(x=>x.TaxGroupCode == item.TaxGroupCode).VatPercent,
                                    VatAmount = item.LineAmount - netAmount,
                                    LineAmountExcVAT = netAmount,
                                    LineAmountIncVAT = item.LineAmount,
                                    IsLoyalty = isLoyalty,
                                    ItemType = item.ArticleType ?? "",
                                    Remark = null,
                                    TransDiscountEntry = discountEntry
                                });
                            }

                            List<TransPaymentEntryDto> payments = new List<TransPaymentEntryDto>();
                            if (rpData.Data.Payments != null && rpData.Data.Payments.Count > 0)
                            {
                                foreach (var rp in rpData.Data.Payments)
                                {
                                    payments.Add(new TransPaymentEntryDto()
                                    {
                                        LineNo = rp.LineId,
                                        TenderType = rp.PaymentMethod,
                                        PaymentAmount = rp.AmountTendered,
                                        PayForOrderNo = rpData.Data.OrderNo ?? "",
                                        TransactionNo = rp.TransactionNo ?? "",
                                        ApprovalCode = rp.TransactionNo ?? "",
                                        TraceCode = rp.TraceCode ?? "",
                                        TransactionId = rp.ReferenceId ?? "",
                                        ReferenceNo = rp.TransactionNo ?? ""
                                    });
                                }
                            }

                            BillingInfo billingInfo = new BillingInfo();
                            if (rpData.Data.HasVatInvoice && rpData.Data.BillingInfo != null)
                            {
                                billingInfo.TaxID = rpData.Data.BillingInfo.TaxID;
                                billingInfo.CustName = rpData.Data.BillingInfo.CustName;
                                billingInfo.CompanyName = rpData.Data.BillingInfo.CompanyName;
                                billingInfo.Address = rpData.Data.BillingInfo.Address;
                                billingInfo.Phone = rpData.Data.BillingInfo.Phone ?? "";
                                billingInfo.Email = rpData.Data.BillingInfo.Email ?? "";
                                billingInfo.Note = rpData.Data.BillingInfo.Note ?? "";
                            }

                            var cardMember = "";
                            if(rpData.Data.MembershipCard != null && rpData.Data.MembershipCard.FirstOrDefault() != null)
                            {
                                cardMember = rpData.Data.MembershipCard.FirstOrDefault().PhoneNumber;
                            }

                            resultObj.Data = new TransHeaderDto()
                            {
                                AppCode = request.AppCode,
                                OrderNo = rpData.Data.OrderNo,
                                OrderTime = rpData.Data.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                CustNo = rpData.Data.StoreNo ?? "",
                                CustName = rpData.Data.ShippingInfo != null ? (rpData.Data.ShippingInfo.ReceiverName ?? "") : "",
                                CustPhone = rpData.Data.ShippingInfo != null ? (rpData.Data.ShippingInfo.ReceiverPhone ?? "") : "",
                                CustAddress = rpData.Data.ShippingInfo != null ? (rpData.Data.ShippingInfo.ReceiverAddress ?? "") : "",
                                CustNote = rpData.Data.Note ?? "",
                                DeliveryType = rpData.Data.SaleTypeId == 10 ? 0 : 1,
                                CardMember = cardMember,
                                TotalAmount = rpData.Data.TotalAmount,
                                PaymentAmount = rpData.Data.PaymentAmount,
                                IsPromotion = false,
                                Status = rpData.Data.Status == 3 ? 0 : 1,
                                CashierId = "",
                                PromoName = "",
                                PromoAmount = 0,
                                StoreInfo = null,
                                RefNo = "",
                                BillingInfo = rpData.Data.HasVatInvoice == true ? billingInfo : null,
                                TransLine = transLines,
                                TransPaymentEntry = payments
                            };

                            if(resultObj.Data != null)
                            {
                                resultObj.Meta = new Meta()
                                {
                                    Code = 200,
                                    Message = "Successfully"
                                };
                            }
                            else
                            {
                                resultObj.Meta = new Meta()
                                {
                                    Code = 400,
                                    Message = "Lỗi dữ liệu " + request.OrderNo.ToString()
                                };
                            }
                        }
                        else
                        {
                            resultObj.Meta = new Meta()
                            {
                                Code = 400,
                                Message = "Không tìm thấy đơn hàng " + request.OrderNo.ToString()
                            };
                        }
                    }
                }
                else
                {
                    resultObj = RestResponseNotOK(response, requestId);
                    _logger.LogWarning("GetBillDetail_WCM_Async ERROR requestId: " + requestId + " |ErrorMessage: " + response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("GetBillDetail_WCM_Async Exception: " + ex.Message.ToString());
                resultObj = ResponseHelper.RspNotFoundData(ex.Message.ToString());
            }
            return resultObj;
        }
        private ResponseClient RestResponseNotOK(IRestResponse response, string requestId = "")
        {
            string message = "";
            if (!string.IsNullOrEmpty(response.StatusDescription))
            {
                message = response.StatusDescription.ToString();
            }
            else if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                message = response.ErrorMessage;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                if (!string.IsNullOrEmpty(response.Content))
                {
                    var rpData = JsonConvert.DeserializeObject<RspUpdateStatusOrderCRX>(response.Content);
                    if (rpData != null)
                    {
                        message = rpData.Message != null ? rpData.Message.ToString() : message;
                    }
                }
            }
            if (!string.IsNullOrEmpty(message))
            {
                _logger.LogWarning("ERROR requestId: " + requestId + " StatusCode:" + response.StatusCode.ToString() + " | mess: " + message);
            }

            return new ResponseClient()
            {
                Meta = new Meta()
                {
                    Code = (int)response.StatusCode,
                    Message = message
                },
                Data = null
            };
        }
        public async Task<ResponseClient> CountedOrder_WCM(CountOrderRequest request, WebApiViewModel webApi, string requestId = "")
        {
            await Task.Delay(1);
            string storeNo = request.StoreNo;
            int counted = 0;
            var kios = _memoryCacheService.GetStoreAndKiosAsync().Result?.Where(x => x.StoreNo == request.StoreNo).FirstOrDefault();
            if (kios == null)
            {
                return ResponseHelper.RspNotExistsStoreNo("StoreNo: " + request.StoreNo + @" chưa được khai báo dữ liệu");
            }
            else
            {
                string function = "CountedOrder";
                var bodyData = new CountedOrderWebPLH()
                {
                    Status = "New",
                    StoreId = storeNo,
                    ChannelId = "VMP"
                };
                var response = await CallApi_CRX_Async(webApi, function, Method.POST, bodyData, null);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var rpData = JsonConvert.DeserializeObject<CountedOrderRsp>(response.Content);
                    counted = rpData.Data.CountOrder;
                }
                else
                {
                    var rpData = JsonConvert.DeserializeObject<RspBadRequest>(response.Content);
                    _logger.LogWarning(JsonConvert.SerializeObject(rpData));
                }

                return new ResponseClient()
                {
                    Meta = new Meta()
                    {
                        Code = 200,
                        Message = "OK"
                    },
                    Data = new CountOrderResponse()
                    {
                        PartnerCode = request.PartnerCode,
                        AppCode = request.AppCode,
                        StoreNo = request.StoreNo,
                        Counted = counted
                    }
                };
            }
        }
        public async Task<ResponseClient> RefundDetails_WebOnline(SalesReturnRequest request, WebApiViewModel webApi, string requestId = "")
        {
            await Task.Delay(1);
            try
            {
                string storeNo = request.PosNo.Substring(0, 4);
                string function = "RefundDetails";
                string merchantId = "VCM";
                
                if(webApi.WebRoute.Where(x => x.Name == function).FirstOrDefault() == null)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = "OK"
                        },
                        Data = null
                    };
                }

                if (request.PartnerCode == "PLH")
                {
                    merchantId = "PLH";
                }
                request.MerchantId = merchantId;
                request.StoreNo = storeNo;

                request.RefundItems.ForEach(x => x.VatPercent = decimal.Parse(x.VatPercent.ToString("n0")));

                SalesReturnWebOnline dataSaveToDB = new SalesReturnWebOnline()
                {
                    PartnerCode = request.PartnerCode,
                    AppCode = request.AppCode,
                    InvoiceNo = request.OrderNo,
                    OrgInvoiceNo = request.ReturnedOrderNo,
                    OrgOrderNo = request.OrgOrderNo,
                    TransactionDatetime = request.TransactionDatetime,
                    MerchantId = merchantId,
                    StoreNo = storeNo,
                    Note = request.Note,
                    RefundAmount = request.RefundAmount,
                    TenderType = request.TenderType,
                    ReRefundItems = JsonConvert.SerializeObject(request.RefundItems),
                    Message = "",
                    UpdateFlg = "N",
                    CreateTime = DateTime.Now
                };

                var getTenderType = await _memoryCacheService.GetTenderTypeSetupAsync();
                var checkTenderType = getTenderType.Where(x => x.TenderType == request.TenderType && x.PartnerCode == request.PartnerCode && x.AppCode == request.AppCode).FirstOrDefault();
                if(checkTenderType == null)
                {
                    dataSaveToDB.Message = "Chưa khai báo TenderType isRefund";
                    await _transService.SaveSalesReturnWebOnline(dataSaveToDB);
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = "OK"
                        },
                        Data = null
                    };
                }
                else
                {
                    var response = await CallApi_CRX_Async(webApi, function, Method.POST, request, null);
                    _logger.LogWarning("RefundDetails_WebOnline request: " + JsonConvert.SerializeObject(request));
                    _logger.LogWarning("RefundDetails_WebOnline response: " + JsonConvert.SerializeObject(response.Content??""));
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var rpData = JsonConvert.DeserializeObject<RspBadRequest>(response.Content);
                        dataSaveToDB.UpdateFlg = "Y";
                        dataSaveToDB.Message = rpData.Message;
                        await _transService.SaveSalesReturnWebOnline(dataSaveToDB);
                        return new ResponseClient()
                        {
                            Meta = new Meta()
                            {
                                Code = 200,
                                Message = rpData.Message
                            },
                            Data = null
                        };
                    }
                    else
                    {
                        var rpData = JsonConvert.DeserializeObject<RspBadRequest>(response.Content);
                        _logger.LogWarning(JsonConvert.SerializeObject(rpData));
                        dataSaveToDB.UpdateFlg = "E";
                        dataSaveToDB.Message = rpData.Message?? response.StatusCode.ToString();
                        await _transService.SaveSalesReturnWebOnline(dataSaveToDB);
                        return new ResponseClient()
                        {
                            Meta = new Meta()
                            {
                                Code = 201,
                                Message = response.StatusCode.ToString() + " - " + rpData.Message ?? response.StatusCode.ToString()
                            },
                            Data = null
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("RefundDetails_WebOnline Exception: " + ex.Message.ToString());
                return new ResponseClient()
                {
                    Meta = new Meta()
                    {
                        Code = 401,
                        Message = ex.Message.ToString()
                    },
                    Data = null
                };
            }
        }
    }
}
