using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCM.Common.Database;
using VCM.Common.Helpers;
using VCM.Common.Validation;
using VCM.Shared.API;
using VCM.Shared.API.PhucLongV2;
using VCM.Shared.Entity.PriceEngine;
using VCM.Shared.Enums;
using WCM.EntityFrameworkCore.Dtos;
using WCM.EntityFrameworkCore.EntityFrameworkCore.PriceEngine;
using WebApi.PriceEngine.Application.Interfaces;
using WebApi.PriceEngine.Enums;
using WebApi.PriceEngine.Helpers;
using WebApi.PriceEngine.Models.API;

namespace WebApi.PriceEngine.Application.Implementation
{
    public class TransactionService : ITransactionService
    {
        private readonly ILogger<TransactionService> _logger;
        private readonly IDistributedCacheService _redisCacheService;
        private readonly IRedisCacheService _redisService;
        private readonly IConfiguration _configuration;
        private readonly string _connectionStringsWCM = string.Empty;
        private readonly string _connectionStringsPLH = string.Empty;
        private readonly string _connectionStringsPLF = string.Empty;
        private readonly PriceEngineDbContext _databaseContextWCM;
        private readonly PriceEnginePLHDbContext _databaseContextPLH;
        private readonly PriceEnginePLFDbContext _databaseContextPLF;
        public TransactionService(
              ILogger<TransactionService> logger,
              IDistributedCacheService redisCacheService,
              IConfiguration configuration,
              PriceEngineDbContext databaseContextWCM,
              PriceEnginePLHDbContext databaseContextPLH,
              PriceEnginePLFDbContext databaseContextPLF,
              IRedisCacheService redisService
          )
        {
            _logger = logger;
            _redisCacheService = redisCacheService;
            _configuration = configuration;
            _connectionStringsWCM = _configuration["ConnectionStrings:Default"].ToString();
            _connectionStringsPLH = _configuration["ConnectionStrings:PLH"].ToString();
            _connectionStringsPLF = _configuration["ConnectionStrings:PLF"].ToString();
            _databaseContextWCM = databaseContextWCM;
            _databaseContextPLH = databaseContextPLH;
            _databaseContextPLF = databaseContextPLF;
            _redisService = redisService;
        }
        public async Task<ResponseClient> CreateOrderAsync(TransactionRequest transactionRequest)
        {
            ResponseClient responseClient = new ResponseClient();
            transactionRequest.OrderNo = RandomOrderHelper.RandomStringOrder(3).PadLeft(3, '0') + transactionRequest.OrderNo;
            try
            {
                TransactionResponse result = new TransactionResponse();
                DateTime orderDate = DateTime.ParseExact(transactionRequest.OrderDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                if (await InsertTmpTransAsync(transactionRequest, orderDate))
                {
                    List<QueryResults> resultOK = new List<QueryResults>();
                    if (transactionRequest.AppCode.ToUpper() == AppCodeEnum.WCM.ToString())
                    {
                        resultOK = _databaseContextWCM.QueryResults.FromSqlRaw("SP_API_ORDER_PROCESSING {0}, {1}, {2}, {3}",
                                                    transactionRequest.AppCode, transactionRequest.StoreNo, transactionRequest.OrderNo, orderDate.ToString("yyyyMMdd")).ToList();

                    }
                    else if (transactionRequest.AppCode.ToUpper() == AppCodeEnum.PLH.ToString())
                    {
                        resultOK = await _databaseContextPLH.QueryResults.FromSqlRaw("SP_API_ORDER_PROCESSING {0}, {1}, {2}, {3}",
                                                    transactionRequest.AppCode, transactionRequest.StoreNo, transactionRequest.OrderNo, orderDate.ToString("yyyyMMdd")).ToListAsync();

                    }
                    else if (transactionRequest.AppCode.ToUpper() == AppCodeEnum.PLF.ToString())
                    {
                        resultOK = await _databaseContextPLF.QueryResults.FromSqlRaw("SP_API_ORDER_PROCESSING {0}, {1}, {2}, {3}",
                                                    transactionRequest.AppCode, transactionRequest.StoreNo, transactionRequest.OrderNo, orderDate.ToString("yyyyMMdd")).ToListAsync();

                    }

                    if (resultOK.Count > 0 && !string.IsNullOrEmpty(resultOK.FirstOrDefault().Results)) 
                    { 
                        result = GetOrderDetailAsync(transactionRequest.AppCode.ToUpper(), resultOK.FirstOrDefault().Results);
                        responseClient = ResponseHelper.RspMessageEnum(PriceEngineEnum.OrderSuccessfull, (int)PriceEngineEnum.OrderSuccessfull, result);
                    }
                    else
                    {
                        responseClient = ResponseHelper.RspMessageEnum(PriceEngineEnum.OrderErrorCalc, (int)PriceEngineEnum.OrderErrorCalc, null);
                    }
                }
                else
                {
                    responseClient = ResponseHelper.RspMessageEnum(PriceEngineEnum.OrderInsDatabase, (int)PriceEngineEnum.OrderInsDatabase, null);
                }

            }
            catch (Exception ex)
            {
                _logger.LogWarning(transactionRequest.RequestId + " Exception CreateOrderAsync:" + ex.Message.ToString());
                responseClient = ResponseHelper.RspMessageEnum(PriceEngineEnum.OrderException, (int)PriceEngineEnum.OrderException, null);
            }
            return responseClient;
        }
        private async Task<bool> InsertTmpTransAsync(TransactionRequest transactionRequest, DateTime orderDate)
        {
            try
            {
                var tmpTransHeader = new TmpTransHeader()
                {
                    OrderNo = transactionRequest.OrderNo,
                    AppCode = transactionRequest.AppCode,
                    StoreNo = transactionRequest.StoreNo,
                    OrderDate = orderDate.Date,
                    CrtDate = DateTime.Now,
                    RequestId = transactionRequest.RequestId.ToString(),
                    IsLoyalty = transactionRequest.IsLoyalty,
                    WinCode = transactionRequest.WinCode??""
                };

                var tmpTransLine = new List<TmpTransLine>();
                foreach(var item in transactionRequest.Items)
                {
                    tmpTransLine.Add(new TmpTransLine()
                    {
                        OrderNo = transactionRequest.OrderNo,
                        LineNo = item.LineNo,
                        ParentLineNo = item.ParentLineNo,
                        ItemNo = item.ItemNo,
                        ItemName = item.ItemName,
                        Uom = item.Uom,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity,
                        DiscountAmount = item.UnitPrice * item.Quantity - item.UnitPrice * item.Quantity,
                        LineAmountInVAT = item.UnitPrice * item.Quantity,
                        TaxGroupCode = item.TaxGroupCode,
                        VATPercent = item.VatPercent,
                        Barcode = item.Barcode,
                        PluCode = BarcodeHelper.GetPluCode(item.Barcode),
                        CupType = item.CupType,
                        Size = item.Size,
                        IsTopping = item.IsTopping,
                        IsCombo = item.IsCombo,
                        ComboNo = item.ComboNo
                    });
                }

                if (transactionRequest.AppCode.ToUpper() == AppCodeEnum.WCM.ToString())
                {
                    _databaseContextWCM.TmpTransHeader.Add(tmpTransHeader);
                    tmpTransLine.ForEach(n => _databaseContextWCM.TmpTransLine.Add(n));
                    await _databaseContextWCM.SaveChangesAsync();
                }
                else if (transactionRequest.AppCode.ToUpper() == AppCodeEnum.PLH.ToString())
                {
                    _databaseContextPLH.TmpTransHeader.Add(tmpTransHeader);
                    tmpTransLine.ForEach(n => _databaseContextPLH.TmpTransLine.Add(n));
                    await _databaseContextPLH.SaveChangesAsync();
                }
                else if (transactionRequest.AppCode.ToUpper() == AppCodeEnum.PLF.ToString())
                {
                    _databaseContextPLF.TmpTransHeader.Add(tmpTransHeader);
                    tmpTransLine.ForEach(n => _databaseContextPLF.TmpTransLine.Add(n));
                    await _databaseContextPLF.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(transactionRequest.RequestId + " Exception InsertTmpTransAsync:" + ex.Message.ToString());
                return false;
            }
        }
        private TransactionResponse GetOrderDetailAsync(string AppCode, string OrderNo)
        {
            TmpTransHeader transHeader = new TmpTransHeader();
            List<TmpTransLine> transLine = new List<TmpTransLine>();
            List<TmpTransDiscount> transDiscount = new List<TmpTransDiscount>();
            if (AppCode == AppCodeEnum.WCM.ToString())
            {
                transHeader = _databaseContextWCM.TmpTransHeader.Where(x => x.OrderNo == OrderNo).FirstOrDefault();
                transLine = _databaseContextWCM.TmpTransLine.Where(x => x.OrderNo == OrderNo).ToList();
                transDiscount = _databaseContextWCM.TmpTransDiscount.Where(x => x.OrderNo == OrderNo).ToList();
            }
            else if (AppCode == AppCodeEnum.PLH.ToString())
            {
                transHeader = _databaseContextPLH.TmpTransHeader.Where(x => x.OrderNo == OrderNo).FirstOrDefault();
                transLine = _databaseContextPLH.TmpTransLine.FromSqlRaw(@"SELECT * FROM TmpTransLine (NOLOCK) WHERE OrderNo = '" + OrderNo + "'").ToList();
                transDiscount = _databaseContextPLH.TmpTransDiscount.Where(x => x.OrderNo == OrderNo).ToList();
            }
            else if (AppCode == AppCodeEnum.PLF.ToString())
            {
                transHeader = _databaseContextPLF.TmpTransHeader.Where(x => x.OrderNo == OrderNo).FirstOrDefault();
                transLine = _databaseContextPLF.TmpTransLine.FromSqlRaw(@"SELECT * FROM TmpTransLine (NOLOCK) WHERE OrderNo = '" + OrderNo + "'").ToList();
                transDiscount = _databaseContextPLF.TmpTransDiscount.Where(x => x.OrderNo == OrderNo).ToList();
            }
            //_logger.LogWarning(JsonConvert.SerializeObject(transLine));

            List<ItemResponse> items = new List<ItemResponse>();
            List<DiscountResponse> discountEntry = new List<DiscountResponse>();
            foreach (var item in transLine)
            {
                var dists = transDiscount.Where(x => x.OrderLineNo == item.LineNo).ToList();
                decimal discountAmount = 0;
                if(dists.Count > 0)
                {
                    discountAmount = dists.Sum(x => x.DiscountAmount);
                    foreach(var d in dists)
                    {
                        discountEntry.Add(new DiscountResponse()
                        {
                            LineNo = d.LineNo,
                            OrderLineNo = d.OrderLineNo,
                            Quantity = d.Quantity,
                            DiscountAmount = d.DiscountAmount,
                            OfferNo = d.OfferNo,
                            OfferType = d.OfferType,
                        });
                    }
                }

                if(discountAmount > item.UnitPrice * item.Quantity)
                {
                    discountAmount = item.UnitPrice * item.Quantity;
                }

                items.Add(new ItemResponse()
                {
                    LineNo = item.LineNo,
                    ParentLineNo = item.ParentLineNo,
                    Barcode = item.Barcode,
                    ItemNo = item.ItemNo,
                    ItemName = item.ItemName,
                    Uom = item.Uom,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    DiscountAmount = discountAmount,
                    TaxGroupCode = item.TaxGroupCode,
                    VatPercent = item.VATPercent,
                    CupType = item.CupType,
                    Size = item.Size,
                    IsCombo = item.IsCombo,
                    IsTopping = item.IsTopping,
                    ComboNo = item.ComboNo,
                    DiscountEntry = discountEntry.ToList()
                });
                discountEntry.Clear();
            }
            return new TransactionResponse()
            {
                AppCode = transHeader.AppCode,
                OrderDate = transHeader.OrderDate.ToString("yyyyMMdd"),
                OrderNo = transHeader.OrderNo.Substring(3, transHeader.OrderNo.Length - 3),
                StoreNo = transHeader.StoreNo,
                IsLoyalty = transHeader.IsLoyalty,
                WinCode = transHeader.WinCode,
                Items = items

            };
        }
        public async Task<IList<SalesPriceResponse>> CheckMultiBarcodeSalePriceAsync(BarcodeSalesPriceRequest salesPriceRequest)
        {
            try
            {
                List<SalesPriceResponse> salesPriceResponses = new List<SalesPriceResponse>();
                var dapper = new DapperContext(_connectionStringsWCM);
                using IDbConnection conn = dapper.CreateConnDB;
                conn.Open();

                var list = salesPriceRequest
                    .SKUs
                    .Select(x => x.Barcode.Substring(0, 2) == "26"
                    || x.Barcode.Substring(0, 2) == "11"
                    && x.Barcode.Length == 13 ? x.Barcode.Substring(0, 7) + "000000" : x.Barcode)
                    .ToArray();

                string Barcodes = "";
                foreach(var b in list)
                {
                    Barcodes = Barcodes + "," + b.ToString();
                }
                var dataPrice = await conn.QueryAsync<SalesPriceResponse>("EXEC [SP_API_SALESPRICE_BARCODES] @AppCode, @StoreNo, @Barcodes", new
                {
                    salesPriceRequest.AppCode,
                    salesPriceRequest.StoreNo,
                    Barcodes
                }, commandType: CommandType.Text).ConfigureAwait(false);

                _logger.LogWarning(JsonConvert.SerializeObject(dataPrice));

                var listItems = dataPrice?.ToList(); 
                foreach(var item in listItems)
                {
                    salesPriceResponses.Add(new SalesPriceResponse()
                    {
                        ItemNo = item.ItemNo,
                        Barcode = item.Barcode,
                        ItemName = item.ItemName,
                        Uom = item.Uom,
                        Quantity = salesPriceRequest.SKUs.Where(x=>x.PluCode == item.Barcode).FirstOrDefault().Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountAmount = item.DiscountAmount,
                        TaxGroupCode = item.TaxGroupCode,
                        VatPercent = item.VatPercent,
                        OfferNo = item.OfferNo,
                        OfferType = item.OfferType,
                        DiscountType = item.DiscountType
                    });
                }

                return salesPriceResponses;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> CheckBarcodeSalePriceAsync Exception: " + ex.Message.ToString());
                return null;
            }
        }
        public async Task<IList<SalesPriceResponse>> CheckItemSalePriceAsync(ItemSalesPriceRequest salesPriceRequest)
        {
            try
            {
                string connectStringDB = "";
                if(salesPriceRequest.AppCode.ToUpper() == "WCM")
                {
                    connectStringDB = _connectionStringsWCM;
                }
                else if(salesPriceRequest.AppCode.ToUpper() == "PLH")
                {
                    connectStringDB = _connectionStringsPLH;
                }
                else if (salesPriceRequest.AppCode.ToUpper() == "PLF")
                {
                    connectStringDB = _connectionStringsPLF;
                }

                var dapper = new DapperContext(connectStringDB);
                using IDbConnection conn = dapper.CreateConnDB;
                conn.Open();
               
                var Items = GetListItems(salesPriceRequest);
                //_logger.LogWarning("===> Items: " + Items);

                var items = await conn.QueryAsync<SalesPriceResponse>("EXEC [SP_API_SALESPRICE_ITEMS] @AppCode, @StoreNo, @Items", new
                {
                    salesPriceRequest.AppCode,
                    salesPriceRequest.StoreNo,
                    Items
                }, commandType: CommandType.Text).ConfigureAwait(false);

                _logger.LogWarning("===> result: " + JsonConvert.SerializeObject(items));
                return items?.ToList(); ;
            }
            catch(Exception ex)
            {
                _logger.LogWarning("===> CheckItemSalePriceAsync Exception: " + ExceptionHelper.ExptionMessage(ex));
                return null;
            }
        }
        public async Task<SalesPriceResponse> CheckBarcodeSalePriceAsync(CheckSalesPriceRequest salesPriceRequest)
        {
            try
            {
                string key = @"PRICE_" + salesPriceRequest.AppCode + "_" + salesPriceRequest.StoreNo +"_"+ salesPriceRequest.Barcode + "_" + salesPriceRequest.Quantity.ToString();
                var dapper = new DapperContext(_connectionStringsWCM);
                using IDbConnection conn = dapper.CreateConnDB;
                conn.Open();               
                return await _redisCacheService.GetBarcodeSalePriceAsync(conn, salesPriceRequest, key, false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> CheckBarcodeSalePriceAsync Exception: " + ex.Message.ToString());
                return null;
            }
        }
        public async Task<ResponseClient> SaveTmpTransRawAsync(string UserName, RawDataRequest rawData)
        {
            Meta meta = new Meta();
            try
            {
                if(rawData.AppCode == "TVB")
                {
                    if(SaveTransRawTVB(UserName, rawData))
                    {
                        meta.Code = (int)PriceEngineEnum.OrderSuccessfull;
                        meta.Message = EnumHelper.ToEnumString(PriceEngineEnum.OrderSuccessfull);
                    }
                    else
                    {
                        meta.Code = (int)PriceEngineEnum.OrderException;
                        meta.Message = EnumHelper.ToEnumString(PriceEngineEnum.OrderException);
                    }
                }
                else
                {
                    var check = _databaseContextWCM.TmpTransRaw.Where(x => x.OrderNo == rawData.OrderNo && x.AppCode == rawData.AppCode && x.StoreNo == rawData.StoreNo).FirstOrDefault();
                    if (check == null && rawData != null)
                    {
                        var tmpTransRaw = new TmpTransRaw()
                        {
                            PartnerCode = rawData.PartnerCode,
                            AppCode = rawData.AppCode,
                            StoreNo = rawData.StoreNo,
                            OrderNo = rawData.OrderNo,
                            RequestId = rawData.RequestId,
                            JsonData = rawData.JsonData,
                            UpdateFlg = "N",
                            CrtDate = DateTime.Now,
                            CrtUser = UserName,
                            HostName = System.Environment.MachineName.ToString()
                        };
                        _databaseContextWCM.TmpTransRaw.Add(tmpTransRaw);
                        await _databaseContextWCM.SaveChangesAsync();
                        meta.Code = (int)PriceEngineEnum.OrderSuccessfull;
                        meta.Message = EnumHelper.ToEnumString(PriceEngineEnum.OrderSuccessfull);
                    }
                    else if (check != null)
                    {
                        meta.Code = (int)PriceEngineEnum.OrderAlreadyExist;
                        meta.Message = EnumHelper.ToEnumString(PriceEngineEnum.OrderAlreadyExist);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning("===> SaveTmpTransRaw Exception: " + ExceptionHelper.ExptionMessage(ex));
                meta.Code = (int)PriceEngineEnum.OrderInsDatabase;
                meta.Message = EnumHelper.ToEnumString(PriceEngineEnum.OrderInsDatabase);
            }

            return new ResponseClient() 
            { 
                Meta = meta
            }; 
        }
        public async Task<ResponseClient> PutOrderDataAsync(string UserName, OrderRequestBody orderRequestBody)
        {
            var rawData = new RawDataRequest()
            {
                AppCode = orderRequestBody.AppCode,
                OrderNo = orderRequestBody.OrderNo,
                StoreNo = orderRequestBody.StoreNo,
                RequestId = orderRequestBody.StoreNo,
                JsonData = JsonConvert.SerializeObject(orderRequestBody)
            };
            return await SaveTmpTransRawAsync(UserName, rawData);
        }
        private bool SaveTransRawTVB(string UserName, RawDataRequest rawData)
        {
            string function = "TanVietBook";
            var sysConfig = _redisService.GetDataSysConfigAsync().Result?.Where(x => x.AppCode == rawData.AppCode && x.Name.ToUpper() == function.ToUpper()).FirstOrDefault();
            if (sysConfig != null)
            {
                try
                {
                    DapperContext dapperDbContext = new DapperContext(sysConfig.Prefix);
                    using var conn = dapperDbContext.CreateConnDB;
                    conn.Open();
                    using var transaction = conn.BeginTransaction();
                    try
                    {
                        var queryIns = @"INSERT INTO [dbo].[TransRaw]([AppCode],[StoreNo],[OrderNo],[JsonData],[UpdateFlg],[HostName],[CrtUser]) "
                                        + "VALUES (@AppCode, @StoreNo, @OrderNo, @JsonData, 'N', 'API','TVB');";
                        conn.Execute(queryIns, rawData, transaction);
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        FileHelper.WriteLogs("SaveTransRawTVB Exception.transaction: " + ex.Message.ToString());
                        return false;
                    }

                }
                catch(Exception ex)
                {
                    _logger.LogWarning("===> SaveTransRawTVB.Exception: " + ex.Message.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private string GetListItems(ItemSalesPriceRequest salesPriceRequest)
        {
            StringBuilder Items = new StringBuilder();
            foreach (var item in salesPriceRequest.SKUs)
            {
                if (!String.IsNullOrEmpty(item.ItemNo))
                {
                    Items.Append(item.ItemNo.ToString() + item.Uom.ToString() + ",");
                }
            }
            //remove last ','
            if (Items.Length > 0)
            {
                Items.Remove(Items.Length - 1, 1);
            }

            return Items.ToString();
        }
    }
}
