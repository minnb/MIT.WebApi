

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using VCM.Common.Helpers;
using VCM.Shared.Partner;
using VCM.Shared.API;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Partner.API.ViewModels.Phano;
using VCM.Shared.Entity.Partner;
using VCM.Partner.API.Common.Const;
using VCM.Shared.Dtos.POS;
using VCM.Shared.Enums;

namespace VCM.Partner.API.Application.Implementation
{
    public class PhanoService : IPhanoService
    {
        private readonly ILogger<PhanoService> _logger;
         private readonly ITransService _transService;

        public PhanoService(
           ILogger<PhanoService> logger,
           ITransService transService
           )
        {
            _logger = logger;
            _transService = transService;
        }
        public ResponseClient GetSalesOrderPhano(RequestTransaction request, WebApiViewModel webApiInfo, List<Item> itemDto, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                var url_request = webApiInfo.Host + webApiInfo.WebRoute.Where(x => x.Name == "GetSOInfoBySerial").FirstOrDefault().Route.ToString();
                url_request = url_request + @"?serial=" + request.OrderNo.ToString();
                ApiHelper api = new ApiHelper(
                    url_request,
                    "",
                    webApiInfo.Description + ":" + webApiInfo.UserName + ":" + webApiInfo.Password,
                    "GET",
                    null,
                    true,
                    proxyHttp,
                    byPass
                    );

                string strResponse = api.InteractWithApi();
                _logger.LogWarning("Response: " + strResponse);
                if (!string.IsNullOrEmpty(strResponse))
                {
                    var rp = JsonConvert.DeserializeObject<RespSalesOrderPNP>(strResponse);
                    if (rp != null)
                    {
                        if(rp.Status == 200 || rp.Status == 300)
                        {
                            resultObj.Meta = new Meta()
                            {
                                Code = rp.Status,
                                Message = rp.Description
                            };

                            int statusOrder = 0;
                            if(rp.Data.Type == "Sale")
                            {
                                statusOrder = rp.Data.Paid == false ? 0 : 1;
                            }
                            else if(rp.Data.Type == "Return")
                            {
                                statusOrder = 70;
                            }
                            var itemPartner = new InfoItemPartner()
                            {
                                ItemNo = "",
                                UnitPrice = rp.Data.TotalAmount, 
                                Qty = 1,
                                IsLoyalty = false, 
                                DiscountAmount = 0,
                                LineAmountInclVAT = rp.Data.TotalAmount
                            };

                            List<TransLineDto> transLines = new List<TransLineDto>
                            {
                                ObjExample.MappingTransLine(itemDto.FirstOrDefault(), 1,0, itemPartner, null)
                            };

                            resultObj.Data = new TransHeaderDto()
                            {
                                AppCode = request.PartnerCode,
                                OrderNo = rp.Data.Serial,
                                OrderTime = rp.Data.DocumentTime,
                                CustNo = "",
                                CustName = rp.Data.CustomerName,
                                CustPhone = rp.Data.Phone,
                                CustAddress = rp.Data.Address,
                                CustNote = "",
                                DeliveryType = 1,
                                CardMember = "",
                                TotalAmount = rp.Data.TotalAmount,
                                PaymentAmount = 0,
                                Status = statusOrder,
                                IsPromotion = false,
                                RefNo = rp.Data.Type == "Return" ? rp.Data.Ref : "",
                                TransLine = transLines,
                                TransPaymentEntry = null
                            };
                        }
                        else
                        {
                            resultObj.Meta = new Meta()
                            {
                                Code = rp.Status,
                                Message = rp.Description
                            };
                        }
                        
                    }
                }
            }
            catch(Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
            }
            return resultObj;
        }
        public ResponseClient UpdateSalesOrderPhano(RequestUpdateOrderStatus request, WebApiViewModel webApiInfo, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                var url_request = webApiInfo.Host + webApiInfo.WebRoute.Where(x => x.Name == "UpdatePaidBySerial").FirstOrDefault().Route.ToString();
                url_request = url_request + @"?serial=" + request.OrderNo.ToString();
                ApiHelper api = new ApiHelper(
                    url_request,
                    "",
                    webApiInfo.Description + ":" + webApiInfo.UserName + ":" + webApiInfo.Password,
                    "PUT",
                    null,
                    true,
                    proxyHttp,
                    byPass
                    );
                string strResponse = api.InteractWithApi();
                
                _logger.LogWarning("Response: " + strResponse);

                if (!string.IsNullOrEmpty(strResponse))
                {
                    resultObj.Meta = ResultUpdateStatusOrder(strResponse, false);

                    if(resultObj.Meta.Code == 200)
                    {
                        _transService.LoggingApi(request.PartnerCode, request.PosNo, "UpdatePaidBySerial", request.OrderNo, "", strResponse, OrderStatusEnum.Paid.ToString());
                    }
                }
                else
                {
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, "UpdatePaidBySerial", request.OrderNo, "", "request timeout", ApiStatusEnum.Timeout.ToString());
                    
                    resultObj.Meta = ResultUpdateStatusOrder(strResponse, true);

                    if (resultObj.Meta.Code == 200)
                    {
                        _transService.LoggingApi(request.PartnerCode, request.PosNo, "UpdatePaidBySerial", request.OrderNo, "", strResponse, OrderStatusEnum.Paid.ToString());
                    }
                    else
                    {
                        resultObj.Meta = ResponseHelper.RspTimeOut(request.PartnerCode);
                        _transService.LoggingApi(request.PartnerCode, request.PosNo, "UpdatePaidBySerial", request.OrderNo, "", "request timeout", ApiStatusEnum.Timeout.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
            }
            resultObj.Data = null;
            return resultObj;
        }
        
        private Meta ResultUpdateStatusOrder(string strResponse, bool isRetry)
        {
            var rp = JsonConvert.DeserializeObject<RespUpdateSalesOrderPNP>(strResponse);
            if (rp != null)
            {
                if (rp.Status == 200 && rp.Data == true)
                {
                    return new Meta()
                    {
                        Code = 200,
                        Message = rp.Description
                    };
                }
                else
                {
                    if(isRetry && rp.Status == 300)
                    {
                        return new Meta()
                        {
                            Code = 200,
                            Message = rp.Description
                        };
                    }
                    else
                    {
                        return new Meta()
                        {
                            Code = rp.Status,
                            Message = rp.Description
                        };
                    }
                }
            }
            else
            {
                return new Meta()
                {
                    Code = (int)ApiStatusEnum.Timeout,
                    Message = rp.Description
                };
            }
        }
    }
}
