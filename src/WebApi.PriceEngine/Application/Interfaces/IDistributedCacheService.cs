using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WebApi.PriceEngine.Models.API;

namespace WebApi.PriceEngine.Application.Interfaces
{
    public interface IDistributedCacheService
    {
        public Task<SalesPriceResponse> GetBarcodeSalePriceAsync(IDbConnection conn, CheckSalesPriceRequest salesPriceRequest, string key, bool isDelete = false);

    }
}
