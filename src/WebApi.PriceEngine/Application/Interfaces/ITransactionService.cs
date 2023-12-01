using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VCM.Shared.API;
using VCM.Shared.API.PhucLongV2;
using VCM.Shared.Entity.PriceEngine;
using WebApi.PriceEngine.Models.API;

namespace WebApi.PriceEngine.Application.Interfaces
{
    public interface ITransactionService
    {
        public Task<ResponseClient> PutOrderDataAsync(string UserName, OrderRequestBody orderRequestBody);
        public Task<ResponseClient> SaveTmpTransRawAsync(string UserName, RawDataRequest rawData);
        public Task<ResponseClient> CreateOrderAsync(TransactionRequest transactionRequest);
        public Task<SalesPriceResponse> CheckBarcodeSalePriceAsync(CheckSalesPriceRequest salesPriceRequest);
        public Task<IList<SalesPriceResponse>> CheckMultiBarcodeSalePriceAsync(BarcodeSalesPriceRequest salesPriceRequest);
        public Task<IList<SalesPriceResponse>> CheckItemSalePriceAsync(ItemSalesPriceRequest salesPriceRequest);
    }
}
