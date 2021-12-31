using System.Collections.Generic;
using System.Threading.Tasks;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Partner.API.ViewModels.MBC;
using VCM.Shared.Entity.Partner;

namespace VCM.Partner.API.Application.Interfaces
{
    public interface IMemoryCacheService
    {
        public Task<UserMBC> MBCTokenAsync(WebApiViewModel webApiInfo, string proxyHttp, string[] byPass, bool isDelete = false);
        public Task<List<WebApiViewModel>> GetDataWebApiAsync(bool isDelete = false);
        public Task<List<Item>> GetItemAsync(bool isDelete = false);
        public Task<List<StoreAndKios>> GetStoreAndKiosAsync(bool isDelete = false);
        public Task SetRedisKeyAsync(string key, string value);
        public Task<string> GetRedisValueAsync(string key);
    }
}
