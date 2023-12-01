using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VCM.Shared.Partner;
using VCM.Shared.API;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Common.Helpers;
using VCM.Shared.API.PLG;
using VCM.Shared.Entity.Partner;
using VCM.Partner.API.Common.Helpers;
using VCM.Partner.API.Common.Const;
using VCM.Shared.Entity.PhucLong;
using VCM.Shared.Dtos.Odoo.Queries;
using VCM.Shared.Dtos.POS;
using VCM.Shared.Const;
using VCM.Shared.Entity.PhucLong.Dtos;
using VCM.Shared.Enums;

namespace VCM.Partner.API.Application.Implementation
{
    public class PhucLongService : IPhucLongService
    {
        private readonly ILogger<PhucLongService> _logger;
        private readonly ITransService _transService;
        private readonly IMemoryCacheService _memoryCacheService;
        public PhucLongService
        (
              ILogger<PhucLongService> logger,
              ITransService transService,
              IMemoryCacheService memoryCacheService
        )
        {
            _logger = logger;
            _transService = transService;
            _memoryCacheService = memoryCacheService;
        }
        public async Task<ResponseClient> GetOrderDetailPhucLong(RequestTransaction request, WebApiViewModel webApiInfo, List<Item> itemDto)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                string function = "OrderDetail";
                var kios = _memoryCacheService.GetStoreAndKiosAsync().Result ?.Where(x => x.StoreNo == request.PosNo.Substring(0, 4)).FirstOrDefault();
                if(kios == null)
                {
                    resultObj = ResponseHelper.RspNotFoundData(request.PosNo.Substring(0, 4) + @" chưa được khai báo ");
                }
                else
                {
                    string payment_method_filter = webApiInfo.WebRoute.FirstOrDefault(x => x.Name == function).Notes.ToString();
                    List<int> payment_method_detail = ConvertHelper.ListStringToInt(payment_method_filter, ";");

                    if (payment_method_detail != null && payment_method_detail.Count > 0)
                    {
                        await _memoryCacheService.SetRedisKeyAsync(RedisConst.Redis_cache_odoo_payment_method_detail_vcm, JsonConvert.SerializeObject(payment_method_detail));
                    }

                    string param = "?order_no=" + request.OrderNo + "&location_id=" + kios.LocationId + "&set=" + kios.Subset.ToString();
                    var response = await RestSharpHelper.InteractWithApi(webApiInfo, function, Method.GET, param);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogWarning("response: " + response.Content);
                        var trans = JsonConvert.DeserializeObject<ResponseOrderDetail>(response.Content);

                        if (kios.LocationId != trans.StoreInfo.StoreId)
                        {
                            resultObj = ResponseHelper.RspNotFoundData(@"Hóa đơn không hợp lệ " + request.OrderNo);
                        }
                        else
                        {
                            List<TransLineDto> transLines = new List<TransLineDto>();
                            foreach (var item in trans.TransLine)
                            {
                                var infoItemPartner = new InfoItemPartner()
                                {
                                    ItemNo = item.ItemNo,
                                    ItemName = item.ItemName,
                                    Qty = item.Qty,
                                    UnitPrice = item.UnitPrice,
                                    DiscountAmount = item.DiscountAmount,
                                    LineAmountInclVAT = item.LineAmountIncVAT,
                                    IsLoyalty = false
                                };
                                transLines.Add(ObjExample.MappingTransLine(itemDto.Where(x=>x.AppCode.ToUpper() == request.PartnerCode.ToUpper()).FirstOrDefault(), item.LineNo, item.ParentLineNo, infoItemPartner, item.Remark, item.TransDiscountEntry));
                            }
                            trans.TransLine = transLines;
                            //trans.StoreInfo.QRCode = kios.QRCode.ToString();
                            resultObj = ResponseHelper.RspOK(trans);
                        }
                    }
                    else
                    {
                        resultObj = ResponseHelper.RspNotFoundData(@"Không có dữ liệu " + request.OrderNo);
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
        public async Task<ResponseClient> GetOrderListPhucLong(RequestListOrderPOS request, WebApiViewModel webApiInfo)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                var kios = _memoryCacheService.GetStoreAndKiosAsync().Result ? .Where(x => x.StoreNo == request.PosNo.Substring(0, 4)).FirstOrDefault();
                if(kios == null)
                {
                    resultObj = ResponseHelper.RspNotFoundData("StoreNo: " + request.PosNo.Substring(0, 4) + @" chưa được khai báo dữ liệu");
                }
                else
                {
                    string function = "ListOrder";

                    string filter = webApiInfo.WebRoute.FirstOrDefault(x => x.Name == function).Notes.ToString();
                    
                    List<int> payment_method = ConvertHelper.ListStringToInt(filter, ";");
                    
                    if(payment_method != null && payment_method.Count > 0)
                    {
                       await _memoryCacheService.SetRedisKeyAsync(RedisConst.Redis_cache_odoo_payment_method_vcm, JsonConvert.SerializeObject(payment_method));
                    }

                    var bodyData = new RequestOrderList()
                    {
                        location_id = kios.LocationId,
                        warehouse_id = kios.WarehouseId,
                        payment_date = request.FromDate,
                        payment_method = payment_method.ToArray(),
                        set = kios.Subset
                    };

                    var response = await RestSharpHelper.InteractWithApi(webApiInfo, function, Method.POST, null, null, bodyData);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var lstOrder = JsonConvert.DeserializeObject<List<ResponseOrderList>>(response.Content);
                        foreach(var item in lstOrder)
                        {
                            item.StoreNo = kios.StoreNo;
                        }
                        resultObj = ResponseHelper.RspOK(lstOrder);
                    }
                    else
                    {
                        resultObj = ResponseHelper.RspNotFoundData("Kios " + request.PosNo.Substring(0, 4) + @" không có dữ liệu");
                    }
                }
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning("GetOrderListPhucLong Exception: " + ex.Message.ToString());
            }
            return resultObj;
        }
        public async Task<ResponseClient> MappingStoreAndKios(WebApiViewModel webApiInfo, bool IsMapping, string StoreNo, string PosOdoo)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                if (!IsMapping)
                {
                    var response = await RestSharpHelper.InteractWithApi(webApiInfo, "PosConfig", Method.GET, null, null);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        resultObj = ResponseHelper.RspOK(JsonConvert.DeserializeObject<List<GetPosConfig>>(response.Content));
                    }
                    else
                    {
                        resultObj = ResponseHelper.RspNotFoundData("Không có dữ liệu");
                    }
                }
                else
                {
                    Dictionary<string, string> headers = new Dictionary<string, string>
                    {
                        { "pos_name", PosOdoo }
                    };

                    var response = await RestSharpHelper.InteractWithApi(webApiInfo, "PosConfig", Method.GET, null, headers);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var lstPosOdoo = JsonConvert.DeserializeObject<List<GetPosConfig>>(response.Content).FirstOrDefault();
                        if(lstPosOdoo != null)
                        {
                            StoreAndKios storeAndKios = new StoreAndKios()
                            {
                                LocationId = lstPosOdoo.stock_location_id,
                                WarehouseId = lstPosOdoo.warehouse_id,
                                PosOdoo = lstPosOdoo.pos_no,
                                LocationName = lstPosOdoo.location_name,
                                StoreName = StoreNo,
                                StoreNo = StoreNo,
                                Blocked = false,
                                Ver = "V1",
                                CrtDate = DateTime.Now
                            };

                            string result = _transService.AddStoreAndKios(storeAndKios);

                            if (result == @"OK")
                            {
                                resultObj = ResponseHelper.RspOK(storeAndKios);
                            }
                            else
                            {
                                resultObj = ResponseHelper.RspNotFoundData(result);
                            }
                        }
                        else
                        {
                            resultObj = ResponseHelper.RspNotFoundData("Mapping không thành công");
                        }
                    }
                    else
                    {
                        resultObj = ResponseHelper.RspNotFoundData("Không có dữ liệu");
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
        public async Task<ResponseClient> UpdateStatusOrderPhucLong(RequestUpdateOrderStatus request, WebApiViewModel webApiInfo)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                //LoyaltyDto loyalty = null; // await GetLoyaltyPLG(request);
                //call API
                var kios = _memoryCacheService.GetStoreAndKiosAsync().Result?.Where(x => x.StoreNo == request.PosNo.Substring(0, 4)).FirstOrDefault();

                string function = "UpdateStatusOrder";
                string param = "?order_no=" + request.OrderNo + "&status=" + request.Status + "&set=" + kios.Subset.ToString();
                var response = await RestSharpHelper.InteractWithApi(webApiInfo, function, Method.PUT, param);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    resultObj = ResponseHelper.RspOK(null);
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, "UpdateStatusOrder", request.OrderNo, "", ApiStatusEnum.OK.ToString(), OrderStatusEnum.Paid.ToString());
                }
                else
                {
                    resultObj = ResponseHelper.RspNotFoundData(@"Không có dữ liệu đơn hàng " + request.OrderNo);
                    _transService.LoggingApi(request.PartnerCode, request.PosNo, "UpdateStatusOrder", request.OrderNo, "", "timeout", ApiStatusEnum.Timeout.ToString());
                }

            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning("Exception: " + ex.Message.ToString());
            }
            return resultObj;
        }
        public async Task<ResponseClient> UpdateStatusVoucherPhucLong(RequestVoucher request, WebApiViewModel webApiInfo)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                string function = "UpdateStatusVoucher";
                var bodyData = new RequestVoucherPLG()
                {
                    serial_number = request.SerialNumber,
                    status = request.Status,
                    effective_date_from = request.EffectiveDateFrom,
                    effective_date_to = request.EffectiveDateTo,
                    amount = request.VoucherAmount,
                    pos_reference = request.OrderReference
                };

                var response = await RestSharpHelper.InteractWithApi(webApiInfo, function, Method.POST, null, null, bodyData);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var dataVoucher = JsonConvert.DeserializeObject<VoucherInfoDto>(response.Content);
                    var result = new ResponseVoucher();
                    if (dataVoucher != null)
                    {
                       result = new ResponseVoucher()
                        {
                            SerialNumber = dataVoucher.serial_number,
                            Type = dataVoucher.type.ToUpper(),
                            Status = GetStatusVoucherPLG(dataVoucher),
                            PublishDate = dataVoucher.publish_date.ToString("yyyyMMdd"),
                            EffectiveDateFrom = StringHelper.DateToString(dataVoucher.effective_date_from),
                            EffectiveDateTo = StringHelper.DateToString(dataVoucher.effective_date_to),
                            DateUsed = StringHelper.DateToString(dataVoucher.date_used),
                            VoucherAmount = dataVoucher.voucher_amount,
                            UsedOn = dataVoucher.used_on
                        };

                        resultObj = GetRspVoucherPLG(dataVoucher, result);

                        _transService.LoggingApi(request.PartnerCode, request.PosNo, function, dataVoucher.serial_number, request.Status.ToString(), JsonConvert.SerializeObject(dataVoucher), dataVoucher.update_status.ToString());

                    }
                    else
                    {
                        resultObj = ResponseHelper.RspNotFoundData("Serial number " + request.SerialNumber + @" không tìm thấy");
                    }
                }
                else
                {
                    resultObj = ResponseHelper.RspNotFoundData("Serial_number " + request.SerialNumber + @" không tìm thấy");
                }
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning("Exception: " + ex.Message.ToString());
            }

            return resultObj;
        }
        private string GetStatusVoucherPLG(VoucherInfoDto voucher)
        {
            if(voucher.state == VoucherStateEnumPLG.Create.ToString())
            {
                return VoucherStatusEnum.SOLD.ToString();
            }
            else
            {
                return VoucherStatusEnum.REDE.ToString();
            }
        }
        private ResponseClient GetRspVoucherPLG(VoucherInfoDto voucher, ResponseVoucher responseVoucher)
        {
            ResponseClient response = new ResponseClient();
            if (voucher.update_status == VoucherUpdateStatusEnum.Success.ToString())
            {
                response.Meta = new Meta
                {
                    Code = 200,
                    Message = @"Kích hoạt thành công"
                };
                response.Data = responseVoucher;
            }
            else if(voucher.update_status == VoucherUpdateStatusEnum.Activated.ToString())
            {
                response.Meta = new Meta
                {
                    Code = 201,
                    Message = voucher.type.ToUpper() + @" đã kích hoạt"
                };
                response.Data = responseVoucher;
            }
            else if (voucher.update_status == VoucherUpdateStatusEnum.UsedOrExpired.ToString())
            {
                response.Meta = new Meta
                {
                    Code = 201,
                    Message = voucher.type.ToUpper() + @" đã sử dụng hoặc hết hạn"
                };
                response.Data = responseVoucher;
            }
            else 
            {
                response.Meta = new Meta
                {
                    Code = 201,
                    Message = voucher.type.ToUpper() + @" không tìm thấy"
                };
                response.Data = null;
            }

            return response;
        }
    }
}
