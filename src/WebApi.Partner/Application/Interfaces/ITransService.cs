using System.Collections.Generic;
using System.Threading.Tasks;
using VCM.Partner.API.ViewModels.Partner;
using VCM.Shared.API;
using VCM.Shared.Entity.Partner;


namespace VCM.Partner.API.Application.Interfaces
{
    public interface ITransService
    {
        void LoggingApi(string partner, string posNo, string serviceType, string orderNo, string reference, string rawData, string status);
        public TransRaw AddTransRaw(TransRaw transRaw);
        public Task<RawData> AddRawDataAsync(string appCode, string storeNo, string orderNo, string requestId, string jsonData, string status);
        public string AddStoreAndKios(StoreAndKios data);
        public Task<ResponseClient> GetStoreAndKiosAsync(GetStoreKiosPaging filter);
    }
}
