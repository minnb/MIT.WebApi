using System.Collections.Generic;
using System.Threading.Tasks;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Partner.API.ViewModels.Partner;
using VCM.Shared.API;
using VCM.Shared.API.PhucLongV2;
using VCM.Shared.Entity.Crownx;
using VCM.Shared.Entity.Partner;
using WebApi.Partner.ViewModels.Campaigns;

namespace VCM.Partner.API.Application.Interfaces
{
    public interface ITransService
    {
        ResponseClient CountOrderNoByStore(CountOrderRequest request, string redis_server, string port);
        void LoggingApi(string partner, string posNo, string serviceType, string orderNo, string reference, string rawData, string status);
        public TransRaw AddTransRaw(TransRaw transRaw);
        public Task<RawData> AddRawDataAsync(string partnerCode, string appCode, string storeNo, string orderNo, string requestId, string jsonData, string status, bool isOverwrite = false);
        public string AddStoreAndKios(StoreAndKios data);
        public Task<ResponseClient> GetStoreAndKiosAsync(GetStoreKiosPaging filter);
        public Task<List<RawData>> GetRawDataAsync(string appCode, string storeNo, string updateFlg);
        public RawData GetRawDataOrderAsync(string storeNo, string orderNo);
        public RawData UpdateRawDataOrderAsync(string storeNo, string orderNo, ref string message);
        Task<ResponseClient> GetOrderDetailCheck(string AppCode, string orderNo, WebApiViewModel webApiInfo);
        public DataTest GetDataTestAsync(string AppCode, string ItemNo);
        public DataTest UpdateDataTestAsync(string AppCode, string ItemNo, string DataUpdate, int TestField);
        public Task<bool> SaveSalesReturnWebOnline(SalesReturnWebOnline salesReturn);
        Task<ResponseClient> CheckOrderDetail(string appCode, string orderNo);
        Task<ResponseClient> CheckCampaign(string appCode, string orderNo);
    }
}
