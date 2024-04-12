using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Common.Validation;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.Common.Const;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Partner.API.ViewModels.WMT;
using VCM.Shared.API;
using VCM.Shared.API.PhucLongV2;
using VCM.Shared.Dtos.PhucLong;
using VCM.Shared.Entity.Partner;
using VCM.Shared.Enums;
using WebApi.Core.AppServices;
using WebApi.Core.Shared;

namespace VCM.Partner.API.Application.Implementation
{
    public class PhucLongV2Service : IPhucLongV2Service
    {
        private readonly ITransService _transService;
        private readonly ILogger<PhucLongV2Service> _logger;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IOrderRedisService _orderRedisService;
        private readonly IKibanaService _kibanaService;
        public PhucLongV2Service(
                ILogger<PhucLongV2Service> logger,
                ITransService transService,
                IMemoryCacheService memoryCacheService,
                IOrderRedisService orderRedisService,
                IKibanaService kibanaService
            )
        {
            _transService = transService;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
            _orderRedisService = orderRedisService;
            _kibanaService = kibanaService;
        }
        public async Task<ResponseClient> CreateOrderAsync(OrderRequestBody orderRequestBody)
        {
            try
            {
                _logger.LogWarning("===> CreateOrderAsync request: " + JsonConvert.SerializeObject(orderRequestBody));
                string error_message = string.Empty;
                if(!OrderValidation.OrderRequest(orderRequestBody, ref error_message))
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 409,
                            Message = error_message
                        },
                        Data = null
                    };
                }
                string orderNo = InitOrderNoRedis(orderRequestBody.PartnerCode, orderRequestBody.AppCode, orderRequestBody.StoreNo, orderRequestBody.OrderNo);
                var result = await _transService.AddRawDataAsync(orderRequestBody.PartnerCode,orderRequestBody.AppCode, orderRequestBody.StoreNo, orderRequestBody.OrderNo, orderRequestBody.OrderNo, JsonConvert.SerializeObject(orderRequestBody), "N", true);
                if (result != null)
                {
                    string jsData = await _orderRedisService.GetOrderRedisAsync(orderNo);
                    if (!String.IsNullOrEmpty(jsData))
                    {
                        await _orderRedisService.DelOrderRedisAsync(orderNo);
                    }
                    await _orderRedisService.SetOrderRedisAsync(orderNo, JsonConvert.SerializeObject(orderRequestBody).ToString());
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = "Successfully"
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = "Đơn hàng đã thanh toán hoặc không tồn tại"
                        },
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteExptionError("===> PhucLongV2Service.CreateOrderAsync", ex);
                return new ResponseClient()
                {
                    Meta = new Meta()
                    {
                        Code = (int)ApiStatusEnum.Failed,
                        Message = ExceptionHelper.ExptionMessage(ex)
                    },
                    Data = null
                };
            }
        }
        public async Task<ResponseClient> GetOrderDetail(RequestTransaction request, List<Item> itemDto)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                var kios = _memoryCacheService.GetStoreAndKiosAsync().Result?.Where(x => x.StoreNo == request.PosNo.Substring(0, 4)).FirstOrDefault();
                if (kios == null)
                {
                    resultObj = ResponseHelper.RspNotExistsStoreNo(request.PosNo.Substring(0, 4) + @" chưa được khai báo ");
                }
                else
                {
                    //string orderNo = InitOrderNoRedis(request.PartnerCode, request.AppCode, kios.PosOdoo, request.OrderNo);
                    string orderNo = request.PartnerCode + "_" + request.StoreNo + "_" + request.AppCode + "_" + request.OrderNo;
                    _logger.LogWarning("===> orderNo: " + orderNo);
                    OrderResponseBody result = null;
                    string jsData = await _orderRedisService.GetOrderRedisAsync(orderNo);
                    if (!string.IsNullOrEmpty(jsData))
                    {
                        result = MappingOrderBody(jsData, request.PartnerCode, itemDto);
                    }
                    else
                    {
                        var rawData = _transService.GetRawDataOrderAsync(kios.PosOdoo, request.OrderNo);
                        if(rawData != null)
                        {
                            if(rawData.UpdateFlg == "N")
                            {
                                //result = JsonConvert.DeserializeObject<OrderResponseBody>(rawData.JsonData);
                                result = MappingOrderBody(rawData.JsonData, request.PartnerCode, itemDto);
                            }
                            else
                            {
                                return ResponseHelper.RspNotFoundData("Đơn hàng " + request.OrderNo +" đã được thanh toán");
                            }
                        }
                    }
                    if(result != null)
                    {
                        resultObj = ResponseHelper.RspOK(result);
                    }
                    else
                    {
                        resultObj = ResponseHelper.RspNotFoundData("Không tìm thấy dữ liệu");
                    }                   
                }
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning("Exception: " + ex.Message.ToString());
            }

            return resultObj;
        }
        public async Task<ResponseClient> GetOrderList(RequestListOrderPOS request)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                string storeNo = request.PosNo.Substring(0, 4);
                var kios = _memoryCacheService.GetStoreAndKiosAsync().Result?.Where(x => x.StoreNo == storeNo).FirstOrDefault();
                if (kios == null)
                {
                    resultObj = ResponseHelper.RspNotExistsStoreNo("StoreNo: " + storeNo + @" chưa được khai báo dữ liệu");
                }
                else
                {
                    var lstData = await _transService.GetRawDataAsync(request.PartnerCode, storeNo, "N");

                    var lstOrder = new List<ResponseOrderList>();
                    if (lstData.Count > 0)
                    {
                        foreach (var item in lstData)
                        {
                            var dataOrder = JsonConvert.DeserializeObject<OrderResponseBody>(item.JsonData);
                            if(dataOrder!= null)
                            {
                                lstOrder.Add(new ResponseOrderList()
                                {
                                    PartnerCode = request.PartnerCode,
                                    AppCode = dataOrder.AppCode,
                                    StoreNo = item.StoreNo,
                                    OrderDate = dataOrder.OrderDate.ToString("yyyy-MM-dd"),
                                    OrderNo = item.OrderNo,
                                    CustName = dataOrder.CustName,
                                    CustAddress = dataOrder.CustAddress,
                                    CustPhone = dataOrder.CustPhone,
                                    TotalItem = (int)dataOrder.Items.Sum(x=>x.Quantity),
                                    PaymentAmount = dataOrder.Items.Sum(x=>x.LineAmount),
                                    CashierId = item.CrtUser,
                                    CashierName = item.CrtUser,
                                    Status = 0,
                                    Remark = null
                                });
                            }
                        }
                    }
                    resultObj = ResponseHelper.RspOK(lstOrder);
                }              
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                ExceptionHelper.WriteExptionError("===> PhucLongV2Service.GetOrderListAsync", ex);
            }
            return resultObj;
        }
        public async Task<ResponseClient> UpdateStatusOrder(RequestUpdateOrderStatus request, WebApiViewModel _webApiAirPayInfo, string _proxy, string[] _bypass)
        {
            ResponseClient resultObj = new ResponseClient();
            string result = string.Empty;
            try
            {
                var kios = _memoryCacheService.GetStoreAndKiosAsync().Result?.Where(x => x.StoreNo == request.PosNo.Substring(0, 4)).FirstOrDefault();
                if (kios == null)
                {
                    resultObj = ResponseHelper.RspNotFoundData(request.PosNo.Substring(0, 4) + @" chưa được khai báo ");
                }
                else
                {
                    var rawData =  _transService.UpdateRawDataOrderAsync(kios.StoreNo, request.OrderNo, ref result);
                    string orderNo = InitOrderNoRedis(request.PartnerCode, rawData.AppCode, kios.StoreNo, request.OrderNo);
                    if (result == "OK")
                    {
                        await _orderRedisService.DelOrderRedisAsync(orderNo);
                        _logger.LogWarning("===> delete from redis: " + orderNo);

                        var result_api = await UpdateStatusOrderPLH(request, _webApiAirPayInfo, _proxy, _bypass);
                        _logger.LogWarning(request.OrderNo + " response: " + JsonConvert.SerializeObject(result_api));
                        
                        resultObj.Meta = ResponseHelper.MetaOK(200, "Thanh toán đơn hàng thành công");
                    }
                    else
                    {
                        resultObj = ResponseHelper.RspNotFoundData(request.PosNo.Substring(0, 4) + @"|" + result);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteExptionError("PhucLongV2Service.UpdateStatusOrder", ex);
                resultObj = ResponseHelper.RspNotFoundData(request.OrderNo + @"|" + ExceptionHelper.ExptionMessage(ex));
            }
            return resultObj;
        }
        private OrderResponseBody MappingOrderBody(string jsonData, string parterCode, List<Item> itemDto)
        {
            string[] cupType = new string[] { "PLASTIC", "PAPER" };
            var data = JsonConvert.DeserializeObject<OrderResponseBody>(jsonData);
            string memberCard = data.MembershipCard.FirstOrDefault().PhoneNumber;
            if(string.IsNullOrEmpty(memberCard))
            {
                memberCard = data.CustPhone;
            }
            
            data.MembershipCard.FirstOrDefault().MemberCardNumber = memberCard;

            if (data.Status == 1)
            {
                data.Status = 0;
            }
            if(data.Items.Count > 0)
            {
                int i = 1;
                foreach (var item in data.Items)
                {
                    var lstLineOption = new List<RspOrderLineOptionDto>();
                    if(item.OptionEntry != null && item.OptionEntry.Count > 0)
                    {
                        foreach(var option in item.OptionEntry)
                        {
                            lstLineOption.Add(new RspOrderLineOptionDto()
                            {
                                LineId = option.LineId,
                                Type = "Option",
                                ItemNo = "DUMMY000",
                                Uom = "DV",
                                OptionType = option.OptionType.ToUpper(),
                                OptionName = option.OptionName,
                                Description = option.ItemNo,
                                Note = option.ItemNo,
                                Qty = option.Qty,
                                ItemNoRef = ""
                            });
                            i++;
                        }
                    }
                    if (cupType.Contains(item.CupType))
                    {
                        var itemCUP = itemDto.Where(x => x.ItemType == item.CupType.ToUpper() && x.RefNo == item.Size).FirstOrDefault();
                        lstLineOption.Add(new RspOrderLineOptionDto()
                        {
                            LineId = i,
                            Type = "Cup",
                            ItemNo = itemCUP.Barcode,
                            Uom = itemCUP.Uom,
                            OptionType = "",
                            OptionName = "",
                            Description = itemCUP.ItemName,
                            Note = item.Size,
                            Qty = item.Quantity,
                            ItemNoRef = itemCUP.ItemNo
                        });
                    }
                    item.OptionEntry = lstLineOption;
                    i = 1;
                }
            }
            return data;
        }
        private async Task<ResponseClient> UpdateStatusOrderPLH(RequestUpdateOrderStatus request, WebApiViewModel webApiInfo, string proxyHttp, string[] byPass)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                string function = "UpdateStatusOrder";
                string billPOS = "";
                if(request.Remark.FirstOrDefault() != null)
                {
                    billPOS = !string.IsNullOrEmpty(request.Remark.FirstOrDefault().Remark1) ? request.Remark.FirstOrDefault().Remark1.ToString() : request.OrderNo;
                }
                var bodyData = new UpdateStatusOrderPHL()
                {
                   OrderNo = request.OrderNo,
                   Status = (int)OrderStateEnumPLH.Success,
                   BillPOS = billPOS
                };
                var response = await CallApi_PLH_Async(webApiInfo, function, Method.PUT, bodyData, null);
                _logger.LogWarning("Response from Website PLH: " + request.OrderNo + @" POS: " + billPOS  + " @" + JsonConvert.SerializeObject(response));
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        var rpData = JsonConvert.DeserializeObject<UpdateStatusRspPLH>(response.Content);
                        _logger.LogWarning("===> " + webApiInfo.AppCode + "-" + function + ": " + JsonConvert.SerializeObject(rpData));
                        if (rpData != null)
                        {
                            _logger.LogWarning(JsonConvert.SerializeObject(rpData));
                            if(rpData.Data.Status == 0) 
                            {
                                resultObj = ResponseHelper.RspOK(null);
                            }
                            else
                            {
                                resultObj.Meta = new Meta()
                                {
                                    Code = rpData.Data.Status,
                                    Message = rpData.Data.Message.ToString()
                                };
                                resultObj.Data = null;
                            }
                        }
                    }
                }
                else
                {
                    var rpData = JsonConvert.DeserializeObject<RspBadRequest>(response.Content);
                    resultObj.Meta = new Meta()
                    {
                        Code = (int)response.StatusCode,
                        Message = rpData.Message.ToString()
                    };
                    resultObj.Data = null;
                    _logger.LogWarning(request.OrderNo + @" POS: " + billPOS + " @" + JsonConvert.SerializeObject(rpData));
                }
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning("===> UpdateStatusOrderPLH.Exception: " + ex.Message.ToString());
            }
            return resultObj;
        }
        private async Task<IRestResponse> CallApi_PLH_Async(WebApiViewModel webApiInfo, string function, Method method, object bodyData, string param)
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
                _logger.LogWarning("===> CallApi_PLH_Async.Exception: " + webApiInfo.AppCode + "-" + function + ": " + response.Content.ToString());
                response.ResponseStatus = ResponseStatus.Error;
                response.ErrorMessage = ex.Message;
                response.ErrorException = ex;
            }

            return response;
        }
        private string InitOrderNoRedis(string partnerCode, string appCode, string storeNo, string orderNo)
        {
            return partnerCode + "_" + storeNo + "_" + appCode + "_" + orderNo;
        }
        public async Task<ResponseClient> CountedOrder(CountOrderRequest request, WebApiViewModel webApi)
        {
            ResponseClient resultObj = new ResponseClient() ;
            string storeNo = request.StoreNo;
            int counted = 0;
            try
            {
                var kios = _memoryCacheService.GetStoreAndKiosAsync().Result?.Where(x => x.StoreNo == storeNo).FirstOrDefault();
                if (kios == null)
                {
                    return ResponseHelper.RspNotExistsStoreNo("StoreNo: " + storeNo + @" chưa được khai báo dữ liệu");
                }

                string function = "CountedOrder";
                var bodyData = new CountedOrderWebPLH()
                {
                    Status = "New",
                    StoreId = storeNo,
                    ChannelId = "PLG"
                };
                var response = await CallApi_PLH_Async(webApi, function, Method.POST, bodyData, null);
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
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning("===> CountedOrder.Exception: " + ex.Message.ToString());
            }

            resultObj = new ResponseClient()
            {
                Meta = new Meta()
                {
                    Code = 200,
                    Message = "OK"
                },
                Data = new CountOrderResponse()
                {
                    PartnerCode = request.PartnerCode,
                    AppCode = "CRX",
                    StoreNo = storeNo,
                    Counted = counted
                }
            };
            return resultObj;
        }
    }
    public class UpdateStatusOrderPHL
    {
        public string OrderNo { get; set; }
        public int Status { get; set; }
        public string BillPOS { get; set; }
    }
    public class UpdateStatusRspPLH
    {
        public string Message { get; set; }
        public DataRspPLH Data { get;set; }
    }
    public class DataRspPLH
    {
        public int Status { get; set; }
        public string Message { get; set; }
    }
    public class CountedOrderWebPLH
    {
        public string Status { get; set; }
        public string StoreId { get; set; }
        public string ChannelId { get; set; }
    }
    public class CountedOrderRsp
    {
        public CountedOrderObj Data { get; set; }
    }
    public class CountedOrderObj
    {
        public int CountOrder { get; set; }
        public string Status { get; set; }
    }
}
