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

namespace VCM.Partner.API.Application.Implementation
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly ILogger<ShoppingCartService> _logger;
        private readonly ITransService _transService;
        public ShoppingCartService
        (
              ILogger<ShoppingCartService> logger,
              ITransService transService
        )
        {
            _logger = logger;
            _transService = transService;
        }
        private async Task<IRestResponse> CallApi_WMTAsync(WebApiViewModel webApiInfo, string function, Method method, object bodyData, string param)
        {
            IRestResponse response = new RestResponse();
            try
            {
                RestClient client = new RestClient(webApiInfo.Host)
                {
                    Timeout = 30000
                };

                string router = webApiInfo.WebRoute.Where(x => x.Name == function).FirstOrDefault().Route;
                if (!string.IsNullOrEmpty(param))
                {
                    router += param;
                }
                RestRequest restRequest = new RestRequest(router, method);
                restRequest.AddHeader("Accept", "application/json");
                restRequest.AddHeader("Authorization", webApiInfo.Description + " " + Convert.ToBase64String(Encoding.UTF8.GetBytes(webApiInfo.UserName + ":" + webApiInfo.Password)));

                if (bodyData != null)
                {
                    restRequest.AddJsonBody(bodyData);
                }
                _logger.LogWarning(webApiInfo.AppCode + "-" + function + ": " + webApiInfo.Host + router);
                response = await client.ExecuteAsync(restRequest);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(webApiInfo.AppCode + "-" + function + ": " + response.Content.ToString());
                response.ResponseStatus = ResponseStatus.Error;
                response.ErrorMessage = ex.Message;
                response.ErrorException = ex;
            }

            return response;
        } 
        public async Task<ResponseClient> GetListBill_WMTAsync(RequestListOrderPOS request, WebApiViewModel webApiInfo, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                string[] statusOrder = {"Confirmed"};
                string function = "ListOrder";
                var bodyData = new RqListOrderWMT()
                {
                    OrderNo = "",
                    StoreId = request.PosNo.Substring(0,4).ToString(),
                    Status = statusOrder,
                    ChannelId = "VMP"
                };

                var response = await CallApi_WMTAsync(webApiInfo, function, Method.POST, bodyData, null);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var rpData = JsonConvert.DeserializeObject<RspListOrderWMT>(response.Content);
                    _logger.LogWarning(webApiInfo.AppCode + "-" + function + ": " + JsonConvert.SerializeObject(rpData));
                    if (rpData.Data.Count > 0)
                    {
                        List<ResponseListOrderPOS> rspListOrders = new List<ResponseListOrderPOS>();
                        if (rpData.Data.Count > 0)
                        {
                            resultObj.Meta = new Meta()
                            {
                                Code = 200,
                                Message = "Successfully"
                            };

                            foreach (var item in rpData.Data)
                            {
                                rspListOrders.Add(new ResponseListOrderPOS()
                                {
                                    PartnerCode = request.PartnerCode,
                                    StoreNo = request.PosNo.Substring(0, 4),
                                    OrderNo = item.OrderCode,
                                    OrderDate = item.OrderTime,
                                    CustName = item.ReceiverName,
                                    CustPhone = item.ReceiverPhone,
                                    CustAddress = item.ReceiverAddress,
                                    Status = StatusConst.StatusDichoWMT()[item.Status],
                                    CashierId = "",
                                    CashierName = "",
                                    TotalItem = 0,
                                    PaymentAmount = item.TotalPayment,
                                    Remark = null
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
                        resultObj.Meta = new Meta()
                        {
                            Code = 200,
                            Message = "Không có dữ liệu"
                        };
                        resultObj.Data = rpData.Data;
                    }
                }
                else
                {
                    var rpData = JsonConvert.DeserializeObject<RspBadRequest>(response.Content);
                    resultObj.Meta = new Meta()
                    {
                        Code = 400,
                        Message = rpData.Message
                    };
                }
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning("Exception: " + ex.Message.ToString());
            }
            return resultObj;
        }
        public async Task<ResponseClient> GetBillDetail_WMTAsync(RequestTransaction request, WebApiViewModel webApiInfo, string proxyHttp, string[] byPass)
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

                var response = await CallApi_WMTAsync(webApiInfo,function, Method.GET, null, param);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var rpData = JsonConvert.DeserializeObject<RspOrderDetailWMT>(response.Content);
                    _logger.LogWarning(webApiInfo.AppCode + "-" + function + ": " + JsonConvert.SerializeObject(rpData));
                    if (rpData != null)
                    {
                        List<TransLineDto> transLines = new List<TransLineDto>();
                        if (rpData.Data != null & rpData.Data.Items.Count > 0)
                        {
                            if(rpData.Data.Items.Count > 0)
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
                                Status = StatusConst.StatusDichoWMT()[rpData.Data.Status],
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
                    var rpData = JsonConvert.DeserializeObject<RspBadRequest>(response.Content);
                    resultObj.Meta = new Meta()
                    {
                        Code = 400,
                        Message = rpData.Message.ToString()
                    };
                    resultObj.Data = null;
                }
             }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning("Exception: " + ex.Message.ToString());
            }
            return resultObj;
        }
        public async Task<ResponseClient> UpdateStatusOrder_WMTAsync(RequestUpdateOrderStatus request, WebApiViewModel webApiInfo, string proxyHttp, string[] byPass)
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

                var response = await CallApi_WMTAsync(webApiInfo, function, Method.PUT, bodyData, param);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        var rpData = JsonConvert.DeserializeObject<RspUpdateStatusOrderlWMT>(response.Content);
                        _logger.LogWarning(webApiInfo.AppCode + "-" + function + ": " + JsonConvert.SerializeObject(rpData));
                        if (rpData != null)
                        {
                            resultObj.Meta = new Meta()
                            {
                                Code = 200,
                                Message = rpData.Message
                            };
                            resultObj.Data = null;

                            _transService.AddTransRaw(new TransRaw()
                            {
                                Id = Guid.NewGuid(),
                                AppCode = request.PartnerCode,
                                StoreNo = request.PosNo.Substring(0, 4),
                                OrderNo = request.OrderNo,
                                ServiceType = "UpdateStatusOrder",
                                ReferenceNo = "",
                                RawData = JsonConvert.SerializeObject(rpData),
                                UpdateFlg = "N",
                                CrtDate = DateTime.Now,
                                CrtUser = request.PosNo,
                                IPAddress = ""
                            });
                        }
                    }
                }
                else
                {
                    var rpData = JsonConvert.DeserializeObject<RspBadRequest>(response.Content);
                    resultObj.Meta = new Meta()
                    {
                        Code = 400,
                        Message = rpData.Message.ToString()
                    };
                    resultObj.Data = null;
                }              
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning("Exception: " + ex.Message.ToString());
            }
            return resultObj;
        }

    }
}
