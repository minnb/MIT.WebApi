using System.Collections.Generic;
using System.Threading.Tasks;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Partner.API.ViewModels.MBC;
using VCM.Shared.API.O2;
using VCM.Shared.API.Shopee.Dish;
using VCM.Shared.Dtos.PhucLong;
using VCM.Shared.Dtos.POS;
using VCM.Shared.Entity.Partner;
using VCM.Shared.Entity.SalesPartner;

namespace VCM.Partner.API.Application.Interfaces
{
    public interface IMemoryCacheService
    {
        public Task SetRedisKeyByTimeAsync(string key, byte[] value, int minute);
        public Task SetRedisKeyAsync(string key, string value);
        public Task<string> GetRedisValueAsync(string key);
        public Task RemoveRedisValueAsync(string key);
        public Task<List<NotifyConfig>> GetNotifyConfig();
        public Task<List<User>> GetUsers();
        public Task<UserMBC> MBCTokenAsync(WebApiViewModel webApiInfo, string proxyHttp, string[] byPass, bool isDelete = false);
        public Task<List<WebApiViewModel>> GetDataWebApiAsync(bool isDelete = false);
        public Task<List<SysConfig>> GetDataSysConfigAsync(bool isDelete = false);
        public Task<List<Item>> GetItemAsync(bool isDelete = false);
        public Task<List<VAT>> GetVATCodeAsync(bool isDelete = false);
        public Task<List<StoreAndKios>> GetStoreAndKiosAsync(bool isDelete = false);
        public Task<List<UserRoles>> GetUserRolesAsync(bool isDelete = false);
        public Task<List<CpnVchBOMHeaderDto>> GetCpnVchBOMHeaderAsync(bool isDelete, string appCode, string function);
        public Task<List<ItemDto>> GetItemPhucLongAsync(bool isDelete = false);
        public Task<List<TenderTypeSetup>> GetTenderTypeSetupAsync(bool isDelete = false);
        public Task<List<ShopeeRestaurant>> GetShopeeRestaurantAsync(bool isDelete = false);
        public Task<TokenO2> TokenO2Async(WebApiViewModel webApiInfo, bool isDelete = false);
        Task<List<StoreMaster>> GetAllStoreAsync(string appCode, bool isDelete = false);
    }
}
