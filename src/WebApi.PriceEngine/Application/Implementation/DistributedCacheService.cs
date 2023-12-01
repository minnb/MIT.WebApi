using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCM.Shared.Const;
using WCM.EntityFrameworkCore.EntityFrameworkCore.PriceEngine;
using WebApi.Core.Common.Extentions;
using WebApi.PriceEngine.Application.Interfaces;
using WebApi.PriceEngine.Models.API;

namespace WebApi.PriceEngine.Application.Implementation
{
    public class DistributedCacheService : IDistributedCacheService
    {
        private readonly ILogger<DistributedCacheService> _logger;
        private readonly PriceEngineDbContext _databaseContextWCM;
        private readonly PriceEnginePLHDbContext _databaseContextPLH;
        private readonly IDistributedCache _distributeCache;
        public DistributedCacheService(
            ILogger<DistributedCacheService> logger,
            PriceEngineDbContext databaseContextWCM,
            PriceEnginePLHDbContext databaseContextPLH,
            IDistributedCache distributeCache
        )
        {
            _logger = logger;
            _distributeCache = distributeCache;
            _databaseContextWCM = databaseContextWCM;
            _databaseContextPLH = databaseContextPLH;
        }

        public async Task<SalesPriceResponse> GetBarcodeSalePriceAsync(IDbConnection conn, CheckSalesPriceRequest salesPriceRequest, string key, bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(key);
            }
            var data = await _distributeCache.GetAsync(key);
            if (data != null)
            {
                //_logger.LogWarning("===> get from cache: " + Encoding.UTF8.GetString(data).ToString());
                return JsonConvert.DeserializeObject<SalesPriceResponse>(Encoding.UTF8.GetString(data));
            }
            else 
            {
                var dataPrice = await conn.QueryAsync<SalesPriceResponse>("EXEC [SP_API_SALES_PRICE_BARCODE] @AppCode, @StoreNo, @Barcode, @Quantity", new
                {
                    salesPriceRequest.AppCode,
                    salesPriceRequest.StoreNo,
                    salesPriceRequest.Barcode,
                    salesPriceRequest.Quantity
                }, commandType: CommandType.Text).ConfigureAwait(false);

                var result = dataPrice?.FirstOrDefault();
                await DistributedCacheExtention.SetDistributedCacheAsync(_distributeCache, RedisConst.PriceEngine_cache_minute_time, key, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result)));
                
                //_logger.LogWarning("===> get from db: " + JsonConvert.SerializeObject(dataPrice));
                return result;
            }
        }

    }
}
