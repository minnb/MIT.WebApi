using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Shared.API;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Dapper;
using WCM.EntityFrameworkCore.EntityFrameworkCore.PriceEngine;
using WebApi.PriceEngine.Application.Interfaces;
using WebApi.PriceEngine.Helpers;
using WebApi.PriceEngine.Models.Master;

namespace WebApi.PriceEngine.Application.Implementation
{
    public class MasterDataService : IMasterDataService
    {
        private readonly ILogger<MasterDataService> _logger;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IConfiguration _configuration;
        private readonly PriceEngineDbContext _databaseContext;
        private string _connectionStrings = string.Empty;
        public MasterDataService(
              ILogger<MasterDataService> logger,
              IRedisCacheService redisCacheService,
              IConfiguration configuration,
              PriceEngineDbContext databaseContext
          )
        {
            _logger = logger;
            _databaseContext = databaseContext;
            _redisCacheService = redisCacheService;
            _configuration = configuration;
            _connectionStrings = _configuration["ConnectionStrings:Default"].ToString();
        }
        public IDbConnection CreateConnDapperDB
        {
            get
            {
                return new SqlConnection(_connectionStrings);
            }
        }
        public async Task<IList<object>> SyncGetDataByTable(string AppCode, string TableName, int maxCounter)
        {
            IList<object> result = null;
            try
            {
                var sysTable = _databaseContext.SysDataTable.Where(x => x.AppCode == AppCode && x.TableName.ToUpper() == TableName.ToUpper()).FirstOrDefault();
                if(sysTable != null)
                {
                    string function = "CentralMD";
                    string ColumnOrderBy = sysTable.OrderBy.ToString();
                    int POSLastCounter = maxCounter;

                    var sysConfig = _redisCacheService.GetDataSysConfigAsync().Result?.Where(x => x.AppCode == AppCode && x.Name.ToUpper() == function.ToUpper()).FirstOrDefault();
                    
                    if (sysConfig != null)
                    {
                        DapperContext dapperDbContext = new DapperContext(sysConfig.Prefix);
                        using var conn = dapperDbContext.CreateConnDB;
                        conn.Open();

                        IEnumerable<object> data = null;
                        if (string.IsNullOrEmpty(sysTable.StoreProcedures))
                        {
                             data = await conn.QueryAsync<object>(@"EXEC SyncGetDataByTable @TableName, @ColumnOrderBy, @POSLastCounter",
                                new { sysTable.TableName, ColumnOrderBy, POSLastCounter }).ConfigureAwait(false);
                        }
                        else
                        {
                            string procedure = @"EXEC " + sysTable.StoreProcedures + " @AppCode, @POSLastCounter";
                             data = await conn.QueryAsync<object>(procedure, new { AppCode, POSLastCounter }).ConfigureAwait(false);
                        }
                        result = data?.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception: " + ex.Message.ToString());
                result = null;
            }
            return result;
        }
        public async Task<FileContentResult> GetFileMasterDataAsync(string appCode, string tableName, int maxCounter, string path, string contentType)
        {
            string file_name = appCode + "_" + tableName + "_" +  DateTime.Now.ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("HHmmssfff") + RandomOrderHelper.RandomNumber(3).PadLeft(3, '0');
            var result = await SyncGetDataByTable(appCode, tableName, maxCounter);

            return new FileContentResult(System.IO.File.ReadAllBytes(FileHelper.CreateZipFile(result!= null ?JsonConvert.SerializeObject(result) : "[]", path, file_name, ".txt")), contentType)
            {
                FileDownloadName = $"{file_name}.zip"
            };
        }
        public ResponseClient GetDataTableMaster()
        {
            try
            {
                var sysTable = _databaseContext.SysDataTable.Where(x => x.Blocked == false).OrderBy(x=>x.AppCode).ToList();
                List<TableNameMasterModel> result = new List<TableNameMasterModel>();
                if(sysTable.Count > 0)
                {
                    foreach(var item in sysTable)
                    {
                        result.Add(new TableNameMasterModel()
                        {
                            AppCode = item.AppCode,
                            TableName = item.TableName,
                            MaxCounter = item.MaxCounter
                        });
                    }
                }
                return new ResponseClient() 
                        { 
                            Meta = new Meta() { Code = 200, Message = "OK"},
                            Data = result
                        };
            }
            catch (Exception ex)
            {
                return ResponseHelper.RspNotWarning(404, ex.Message.ToString());
            }
        }
        public async Task<ResponseClient> GetFeaturedItemMasterAsync(string AppCode, string StoreNo, bool IsPromotion)
        {
            List<FeaturedItemResponse> rs = new List<FeaturedItemResponse>();
            try
            {
                string function = "PriceEngine";
                var sysConfig = _redisCacheService.GetDataSysConfigAsync().Result?.Where(x => x.AppCode == AppCode && x.Name.ToUpper() == function.ToUpper()).FirstOrDefault();

                if (sysConfig != null)
                {
                    DapperContext dapperDbContext = new DapperContext(sysConfig.Prefix);
                    using var conn = dapperDbContext.CreateConnDB;
                    conn.Open();
                    string procedure = string.Empty;
                    if (!IsPromotion)
                    {
                        procedure = @"EXEC SP_GET_MASTER_DATA_ITEM_FEATURED @AppCode, @StoreNo";
                    }
                    else
                    {
                        procedure = @"EXEC SP_GET_MASTER_DATA_ITEM_PROMOTION @AppCode, @StoreNo";
                    }
                    
                    var data = await conn.QueryAsync<FeaturedItemResponse>(procedure, new { AppCode, StoreNo }).ConfigureAwait(false);
                    return ResponseHelper.RspOK(data?.ToList());
                }
                else
                {
                    return ResponseHelper.RspNotWarning(404, "Không tìm thấy dữ liệu");
                }
            }
            catch (Exception ex)
            {
                return ResponseHelper.RspNotWarning(404, ExceptionHelper.ExptionMessage(ex));
            }
        }
        public async Task<ResponseClient> GetComboItemMasterAsync(string AppCode, string StoreNo)
        {
            List<ComboModel> lstCombo = new List<ComboModel>();
            try
            {
                string function = "PriceEngine";
                var sysConfig = _redisCacheService.GetDataSysConfigAsync().Result?.Where(x => x.AppCode == AppCode && x.Name.ToUpper() == function.ToUpper()).FirstOrDefault();
                if (sysConfig != null)
                {
                    DapperContext dapperDbContext = new DapperContext(sysConfig.Prefix);
                    using var conn = dapperDbContext.CreateConnDB;
                    conn.Open();
                    string procedure = @"EXEC PLH_API_GET_COMBO @StoreNo, 0"; 
                    var data = await conn.QueryAsync<ComboHeader>(procedure, new { StoreNo }).ConfigureAwait(false);
                    if(data != null)
                    {
                        var comboHeader = data?.ToList();
                        if(comboHeader.Count > 0)
                        {
                            foreach (var item in comboHeader)
                            {
                                ComboModel comboModel = new ComboModel();
                                var comboItems = conn.Query<ComboItems>(@"PLH_API_GET_COMBO_ITEMS @ComboNo, @StoreNo", new { item.ComboNo, StoreNo }).ToList();
                                if(comboItems.Count > 0)
                                {
                                    var itemBuy = comboItems.Where(x => x.ItemType == "BUY").ToList();
                                    var itemGet = comboItems.Where(x => x.ItemType == "GET").ToList();

                                    comboModel.StoreNo = item.StoreNo;
                                    comboModel.ComboNo = item.ComboNo;
                                    comboModel.ComboName = item.ComboName;
                                    comboModel.StartingDate = item.StartingDate;
                                    comboModel.EndingDate = item.EndingDate;
                                    comboModel.FromTime = item.FromTime;
                                    comboModel.ToTime = item.ToTime;
                                    comboModel.IsMember = item.IsMember;
                                    
                                    if(itemBuy.Count > 0)
                                    {
                                        comboModel.BuyQty = itemBuy.GroupBy(x => x.LineGroup).Count();
                                        List<ComboItemBuy> comboItemBuys = new List<ComboItemBuy>();
                                        foreach(var buy in itemBuy)
                                        {
                                            comboItemBuys.Add(new ComboItemBuy()
                                            {
                                                ItemNo = buy.ItemNo,
                                                Uom = buy.Uom,
                                                Quantity = buy.Quantity,
                                                UnitPrice = buy.UnitPrice,
                                                DiscountAmount = buy.DiscountAmount,
                                                LineGroup = buy.LineGroup
                                            });
                                        }
                                        comboModel.ItemBuy = comboItemBuys.ToList();
                                        comboItemBuys.Clear();
                                    }
                                    else
                                    {
                                        comboModel.BuyQty = 0;
                                        comboModel.ItemBuy = null;
                                    }

                                    if (itemGet.Count > 0)
                                    {
                                        comboModel.GetQty = itemGet.GroupBy(x => x.LineGroup).Count();
                                        List<ComboItemGet> comboItemGets = new List<ComboItemGet>();
                                        foreach (var get in itemGet)
                                        {
                                            comboItemGets.Add(new ComboItemGet()
                                            {
                                                ItemNo = get.ItemNo,
                                                Uom = get.Uom,
                                                Quantity = get.Quantity,
                                                UnitPrice = get.UnitPrice,
                                                DiscountAmount = get.DiscountAmount,
                                                LineGroup = get.LineGroup
                                            });
                                        }
                                        comboModel.ItemGet = comboItemGets.ToList();
                                        comboItemGets.Clear();
                                    }
                                    else
                                    {
                                        comboModel.GetQty = 0;
                                        comboModel.ItemGet = null;
                                    }
                                    lstCombo.Add(comboModel);
                                }
                            }
                        }
                    }   
                    return ResponseHelper.RspOK(lstCombo);
                }
                else
                {
                    return ResponseHelper.RspNotWarning(404, "Không tìm thấy dữ liệu");
                }
            }
            catch (Exception ex)
            {
                return ResponseHelper.RspNotWarning(404, ExceptionHelper.ExptionMessage(ex));
            }
        }
    }
}
