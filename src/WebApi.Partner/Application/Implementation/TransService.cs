using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.Common.Const;
using VCM.Partner.API.Common.Helpers;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Partner.API.ViewModels.Partner;
using VCM.Shared.API;
using VCM.Shared.API.PhucLongV2;
using VCM.Shared.Dtos.BLUEPOS;
using VCM.Shared.Entity.Crownx;
using VCM.Shared.Entity.Partner;
using VCM.Shared.SeedWork;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Dapper;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Partner;
using WebApi.Partner.ViewModels.Campaigns;
using WebApi.Partner.ViewModels.Transaction;

namespace VCM.Partner.API.Application.Implementation
{
    public class TransService : ITransService
    {
        private readonly PartnerDbContext _dbConext;
        private readonly ILogger<TransService> _logger;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IOrderRedisService _orderRedisService;
        public TransService(
                PartnerDbContext dbConext,
                 ILogger<TransService> logger,
                 IMemoryCacheService memoryCacheService,
                 IOrderRedisService orderRedisService
            )
        {
            _dbConext = dbConext;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
            _orderRedisService = orderRedisService;
        }


        public TransRaw AddTransRaw(TransRaw transRaw)
        {
            try
            {
                _dbConext.TransRaw.Add(transRaw);
                _dbConext.SaveChanges();
                return transRaw;
            }
            catch
            {
                return null;
            }
        }
        public string AddStoreAndKios(StoreAndKios data)
        {
            try
            {
                _dbConext.StoreAndKios.Add(data);
                _dbConext.SaveChanges();
                return "OK";
            }
            catch (Exception ex)
            {
                string messageError = ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    messageError = ex.InnerException.Message.ToString();
                }
                return messageError;
            }
        }
        public async Task<ResponseClient> GetStoreAndKiosAsync(GetStoreKiosPaging query)
        {
            var filter = await _dbConext.StoreAndKios.ToListAsync();

            if (!string.IsNullOrEmpty(query.SearchKeyword))
                filter = filter.Where(s => s.LocationName.Contains(query.SearchKeyword)).ToList();

            if (!string.IsNullOrEmpty(query.PosOdoo))
                filter = filter.Where(s => s.PosOdoo.Contains(query.PosOdoo)).ToList();

            if (!string.IsNullOrEmpty(query.StoreNo))
                filter = filter.Where(s => s.StoreNo == query.StoreNo).ToList();

            var totalRow = filter.Count();

            var items =  filter
                .OrderByDescending(x => x.CrtDate)
                .Skip((query.PageIndex - 1) * query.PageSize)
                .Take(query.PageSize).ToList();

            var result = new PagedList<StoreAndKios>(items, totalRow, query.PageIndex, query.PageSize);

            if(result != null)
            {
                return ResponseHelper.RspOK(result);
            }
            else
            {
                return ResponseHelper.RspNotFoundData("Không tìm thấy dữ liệu");
            }
        }
        public void LoggingApi(string partner, string posNo, string serviceType, string orderNo, string reference, string rawData, string status)
        {
            this.AddTransRaw(new TransRaw()
            {
                Id = Guid.NewGuid(),
                AppCode = partner,
                StoreNo = posNo.Substring(0, 4),
                OrderNo = orderNo,
                ServiceType = serviceType,
                ReferenceNo = reference,
                RawData = rawData,
                UpdateFlg = status,
                CrtDate = DateTime.Now,
                CrtUser = posNo,
                IPAddress = System.Environment.MachineName.ToString()
            });
        }
        public async Task<RawData> AddRawDataAsync(string partnerCode, string appCode, string storeNo, string orderNo, string requestId, string jsonData, string status, bool isOverwrite)
        {
            var dataUpdate = _dbConext.RawData.Where(x => x.AppCode == appCode && x.OrderNo == orderNo).FirstOrDefault();
            var rawData = new RawData()
            {
                PartnerCode = partnerCode,
                AppCode = appCode,
                StoreNo = storeNo,
                OrderNo = orderNo,
                RequestId = requestId,
                JsonData = jsonData,
                UpdateFlg = status,
                CrtDate = DateTime.Now,
                CrtUser = appCode,
                HostName = System.Environment.MachineName.ToString()
            };

            if (dataUpdate != null)
            {
                if(isOverwrite == true && dataUpdate.UpdateFlg == "N")
                {
                    dataUpdate.JsonData = jsonData;
                    dataUpdate.HostName = rawData.HostName;
                    dataUpdate.CrtDate = rawData.CrtDate;
                    _dbConext.Update(dataUpdate);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                _dbConext.RawData.Add(rawData);
            }

            await _dbConext.SaveChangesAsync();
            return rawData;
        }
        public async Task<List<RawData>> GetRawDataAsync(string appCode, string storeNo, string updateFlg)
        {
            var filter = await _dbConext.RawData.Where(x=>x.UpdateFlg == updateFlg.ToUpper() && x.StoreNo == storeNo).ToListAsync();

            //if (!string.IsNullOrEmpty(storeNo))
            //    filter = filter.Where(s => s.OrderNo == storeNo).ToList();

            return filter;
        }
        public  RawData GetRawDataOrderAsync(string storeNo, string orderNo)
        {
            return  _dbConext.RawData.Where(x => x.StoreNo == storeNo && x.OrderNo == orderNo).FirstOrDefault();
        }
        public RawData UpdateRawDataOrderAsync(string storeNo, string orderNo, ref string message)
        {
            try
            {
                var rawData =  GetRawDataOrderAsync(storeNo, orderNo);
                if(rawData.UpdateFlg == "Y")
                {
                    message = @"Đơn hàng đã được thanh toán";
                    return rawData;
                }
                else
                {
                    rawData.UpdateFlg = "Y";
                    _dbConext.Update(rawData);
                     _dbConext.SaveChangesAsync();
                    message = @"OK";
                    return rawData;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> UpdateRawDataOrderAsync.Exception " + ex.Message.ToString());
                return null;
            }
        }
        public async Task<ResponseClient> GetOrderDetailCheck(string appCode, string orderNo, WebApiViewModel webApiInfo)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                string function = "PhucLongStaging";
                var sysConfig = _memoryCacheService.GetDataSysConfigAsync().Result?.Where(x => x.AppCode == appCode && x.Name.ToUpper() == function.ToUpper()).FirstOrDefault();
                if (sysConfig != null)
                {
                    DapperContext dapperDbContext = new DapperContext(sysConfig.Prefix);
                    using var conn = dapperDbContext.CreateConnDB;
                    conn.Open();
                    var data = await conn.QueryAsync<ResponseCheckOrder>(@"EXEC SP_API_ORDER_CHECK " + orderNo.ToString()).ConfigureAwait(false);
                    var result = data?.FirstOrDefault();
                    if (result != null)
                    {
                        resultObj = ResponseHelper.RspOK(result);
                    }
                    else
                    {
                        return ResponseHelper.RspNotFoundData("Không tìm thấy đơn hàng " + orderNo);
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
        public ResponseClient CountOrderNoByStore(CountOrderRequest request, string redis_server, string port)
        {
            ResponseClient result = new ResponseClient()
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
                    Counted = _orderRedisService.CoutedOrderByStoreAsync(redis_server, port, request.PartnerCode, request.AppCode, request.StoreNo)
                }
            };

            return result;
        }
        public DataTest GetDataTestAsync(string AppCode, string ItemNo)
        {
            try
            {
                var data = _dbConext.DataTest.Where(x => x.AppCode == x.AppCode && x.ItemNo == ItemNo).FirstOrDefault();
                if (data == null)
                {
                    return null;
                }
                else
                {
                    return data;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message.ToString());
                return null;
            }
        }
        public DataTest UpdateDataTestAsync(string AppCode, string ItemNo, string DataUpdate, int TestField)
        {
            try
            {
                var data = _dbConext.DataTest.Where(x => x.AppCode == AppCode && x.ItemNo == ItemNo).FirstOrDefault();
                if (data == null)
                {
                    return null;
                }
                else
                {
                    switch (TestField)
                    {
                        case 1:
                            data.Test1 = DataUpdate;
                            break;
                        case 2:
                            data.Test2 = DataUpdate;
                            break;
                        case 3:
                            data.Test3 = DataUpdate;
                            break;
                        case 4:
                            data.Test4 = DataUpdate;
                            break;
                        case 5:
                            data.Test5 = DataUpdate;
                            break;
                        case 6:
                            data.Test6 = DataUpdate;
                            break;
                        case 7:
                            data.Test7 = DataUpdate;
                            break;
                        case 8:
                            data.Test8 = DataUpdate;
                            break;
                        case 9:
                            data.Test9 = DataUpdate;
                            break;
                        case 10:
                            data.Test10 = DataUpdate;
                            break;
                    }
                    data.ChgeDate = DateTime.Now;
                    _dbConext.Update(data);
                    _dbConext.SaveChangesAsync();
                    return data;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message.ToString());
                return null;
            }
        }
        public async Task<bool> SaveSalesReturnWebOnline(SalesReturnWebOnline salesReturn)
        {
            try
            {
                var check = _dbConext.SalesReturnWebOnline.Where(x => x.InvoiceNo == salesReturn.InvoiceNo && x.AppCode == salesReturn.AppCode && x.PartnerCode == salesReturn.PartnerCode).FirstOrDefault();
                if(check == null)
                {
                    _dbConext.SalesReturnWebOnline.Add(salesReturn);
                    await _dbConext.SaveChangesAsync();
                }
                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError("LogError SaveSalesReturnWebOnline:" + ex.Message.ToString());    
                return false;
            }
        }
        public async Task<ResponseClient> CheckOrderDetail(string appCode, string orderNo)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                string function = "CentralMD";
                var sysConfig = _memoryCacheService.GetDataSysConfigAsync().Result?.Where(x=>x.AppCode == appCode && x.Name.ToUpper() == function.ToUpper()).FirstOrDefault();
                if (sysConfig != null)
                {
                    DapperContext dapperDbContext = new DapperContext(sysConfig.Prefix);
                    using var conn = dapperDbContext.CreateConnDB;
                    conn.Open();
                    var data = await conn.QueryAsync<GetOrderDetailFromDB>(@"EXEC SP_API_ORDER_CHECK_DETAIL " + orderNo).ConfigureAwait(false);
                    var result = data?.ToList();
                    if (result != null && result.Count > 0) 
                    {
                        OrderDetailResponse resultData =new OrderDetailResponse()
                        {
                            OrderNo = orderNo,
                            OrderDate = result.FirstOrDefault().OrderDate,
                            StoreNo = result.FirstOrDefault().StoreNo,
                            OrderType = result.FirstOrDefault().OrderType,
                        };
                        List<OrderDetailData> lstItem = new List<OrderDetailData>();
                        foreach (var item in result)
                        {
                            lstItem.Add(new OrderDetailData()
                            {
                                LineNo = item.LineNo,
                                ItemNo = item.ItemNo,
                                ItemName = item.ItemName,
                                Barcode = item.Barcode,
                                UnitOfMeasure = item.UnitOfMeasure,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice,
                                DiscountAmount = item.DiscountAmount,
                                LineAmountIncVAT = item.LineAmountIncVAT,
                                VatCode = item.VatCode
                            });
                        }
                        resultData.Items = lstItem;
                        resultObj = ResponseHelper.RspOK(resultData);
                    }
                    else
                    {
                        return ResponseHelper.RspNotFoundData(string.Format("Không tìm thấy Đơn hàng {0}", orderNo));
                    }
                }
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning("CheckOrderDetail Exception: " + ex.Message.ToString());
            }
            return resultObj;
        }
        public async Task<ResponseClient> CheckCampaign(string appCode, string orderNo)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                string function = "Partner";
                var sysConfig = _memoryCacheService.GetDataSysConfigAsync().Result?.Where(x => x.AppCode.ToUpper() == appCode.ToUpper() && x.Name.ToUpper() == function.ToUpper()).FirstOrDefault();
                if (sysConfig != null)
                {
                    string channel = "";
                    var lstStore = _memoryCacheService.GetAllStoreAsync(appCode).Result;
                    var subStore = StringHelper.Left(orderNo.Trim().ToLower(), 4);
                    var checkStore = lstStore.FirstOrDefault(x => x.StoreNo.ToLower() == subStore.ToString());
                    _logger.LogWarning(JsonConvert.SerializeObject(checkStore));
                    if (checkStore != null)
                    {
                        channel =  checkStore.Channel??"";  
                    }
                    if (string.IsNullOrEmpty(channel)) 
                    {
                        return ResponseHelper.RspNotFoundData(string.Format("Đơn hàng {0} không thuộc kênh VMP/VMT", orderNo));
                    }
                    DapperContext dapperDbContext = new DapperContext(sysConfig.Prefix);
                    using var conn = dapperDbContext.CreateConnDB;
                    conn.Open();
                    var data = await conn.QueryAsync<GetCampaignData>(@"EXEC SP_API_ORDER_CHECK_CAMPAIGN '" + orderNo.ToString() + "','" + channel +"'").ConfigureAwait(false);
                    var result = data?.ToList();
                    if (result != null && result.Count > 0)
                    {
                        CheckCampaignResponse resultData = new CheckCampaignResponse()
                        {
                            Channel = channel,
                            StoreNo = result.FirstOrDefault().StoreNo,
                            OrderNo = orderNo,
                            OrderDate = result.FirstOrDefault().OrderDate,
                            PhoneNumber = result.FirstOrDefault().PhoneNumber??""
                        };
                        List<CampaignData> lstItem = new List<CampaignData>();
                        foreach (var item in result)
                        {
                            lstItem.Add(new CampaignData()
                            {
                                Campaign = item.Campaign
                            });
                        }
                        resultData.Campaigns = lstItem;
                        resultObj = ResponseHelper.RspOK(resultData);
                    }
                    else
                    {
                        return ResponseHelper.RspNotFoundData(string.Format("Không tìm thấy Đơn hàng {0}", orderNo));
                    }
                }
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning(JsonConvert.SerializeObject(ex));
                _logger.LogWarning("CheckCampaigns Exception: " + ex.Message.ToString());
            }
            return resultObj;
        }
        public async Task<ResponseClient> TotalSalesByDate(string orderDate)
        {
            string keyRedis = "Redis_TotalSales";
            ResponseClient resultObj = new ResponseClient();
            try
            {
                var checkRedis = _memoryCacheService.GetRedisValueAsync(keyRedis).Result;
                if (!string.IsNullOrEmpty(checkRedis))
                {
                    resultObj = ResponseHelper.RspOK(JsonConvert.DeserializeObject<List<SalesByDateDto>>(checkRedis));
                }
                else
                {
                    string function = "Partner";
                    var sysConfig = _memoryCacheService.GetDataSysConfigAsync().Result?.Where(x => x.AppCode.ToUpper() == "WCM" && x.Name.ToUpper() == function.ToUpper()).FirstOrDefault();
                    if (sysConfig != null)
                    {
                        DapperContext dapperDbContext = new DapperContext(sysConfig.Prefix);
                        using var conn = dapperDbContext.CreateConnDB;
                        conn.Open();
                        var data = await conn.QueryAsync<SalesByDateDto>(@"EXEC SP_SUMMARY_AMOUNT_WCM;").ConfigureAwait(false);
                        resultObj = ResponseHelper.RspOK(data?.ToList());
                        await _memoryCacheService.SetRedisKeyByTimeAsync(keyRedis, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data?.ToList())), 5);
                    }
                }
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning("TotalSalesByDate Exception: " + ex.Message.ToString());
            }
            return resultObj;
        }
        public ResponseClient GetOrderDetail(string appCode, string orderNo)
        {
            ResponseClient resultObj = new ResponseClient();
            try
            {
                int month = 1;
                string monthOfOrder = StringHelper.Right(StringHelper.Left(orderNo, 10), 4);
                string monthInDB = StringHelper.GetMonth(month * (-1), "yyMM");
                if (int.Parse(monthOfOrder) < int.Parse(monthInDB))
                {
                    return ResponseHelper.RspNotFoundData(string.Format("Đơn hàng {0} không hợp lệ do quá thời gian {1} tháng", orderNo, month + 1));
                }

                string function = "CentralSales";
                var sysConfig = _memoryCacheService.GetDataSysConfigAsync().Result?.Where(x => x.AppCode.ToUpper() == appCode.ToUpper() && x.Name.ToUpper() == function.ToUpper()).FirstOrDefault();
                if (sysConfig != null)
                {
                    var lstStore = _memoryCacheService.GetAllStoreAsync(appCode).Result;
                    var subStore = StringHelper.Left(orderNo.Trim().ToLower(), 4);
                    var checkStore = lstStore.FirstOrDefault(x => x.StoreNo.ToLower() == subStore.ToString());
                    _logger.LogWarning(JsonConvert.SerializeObject(checkStore));

                    if (checkStore == null)
                    {
                        return ResponseHelper.RspNotFoundData(string.Format("Đơn hàng {0} không thuộc kênh VMP/VMT", orderNo));
                    }

                    var channel = checkStore.Channel ?? "";
                    string connectstringDB = string.Format(@"server={0}" + sysConfig.Prefix, checkStore.ServerIP);

                    DapperContext dapperDbContext = new DapperContext(connectstringDB);
                    using var conn = dapperDbContext.CreateConnDB;
                    conn.Open();
                    var transHeader = conn.Query<TransHeader>("SELECT * FROM TransHeader (NOLOCK) WHERE OrderNo = @OrderNo;", new {OrderNo = orderNo}, commandTimeout: 30).FirstOrDefault();
                    if(transHeader != null)
                    {
                        transHeader.Channel = channel;
                        transHeader.OrderDate = transHeader.OrderDate.Date;
                        var transLine = conn.Query<TransLine>("SELECT * FROM TransLine (NOLOCK) WHERE [LineType] = 0 AND DocumentNo = @DocumentNo;", new { DocumentNo = orderNo }, commandTimeout:30).ToList();
                        var transPayment = conn.Query<TransPaymentEntry>("SELECT * FROM TransPaymentEntry (NOLOCK) WHERE OrderNo = @OrderNo;", new { OrderNo = orderNo }, commandTimeout: 30).ToList();
                        transHeader.TransLines = transLine.ToList();
                        transHeader.TransPaymentEntry = transPayment.ToList();
                        resultObj = ResponseHelper.RspOK(transHeader);
                    }
                    else
                    {
                        resultObj = ResponseHelper.RspNotFoundData(string.Format("Không tìm thấy đơn hàng {0}", orderNo));
                    }
                }
                else
                {
                    resultObj = ResponseHelper.RspNotFoundData(string.Format("AppCode {0} không đúng hoặc chưa cấu hình WebApi", appCode));
                }
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning(JsonConvert.SerializeObject(ex));
                _logger.LogWarning("CheckCampaigns Exception: " + ex.Message.ToString());
            }
            return resultObj;
        }

    }
}
