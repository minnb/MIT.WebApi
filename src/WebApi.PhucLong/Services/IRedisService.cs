using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VCM.Shared.Dtos.POS;
using VCM.Shared.Entity.Partner;
using VCM.Shared.Entity.PhucLong;
using VCM.Shared.Entity.SalesPartner;
using WebApi.PhucLong.Models;

namespace WebApi.PhucLong.Services
{
    public interface IRedisService
    {
        public Task<List<ShopeeRestaurant>> GetShopeeRestaurantAsync(bool isDelete = false);
        public Task<List<WebApiModel>> GetDataWebApiAsync(bool isDelete = false);
        public Task<string> GetRedisValueAsync(string key);
        public Task SetRedisKeyAsync(string key, string value);
        public Task RemoveRedisKeyAsync(string key);
        Task<List<ResPartnerOdooDto>> GetMemberRedis(int set, bool isDelete = false);
        Task<List<StoreInfoOdooDto>> GetPosConfigRedis(int set, bool isDelete = false);
        Task<List<EmployeeOdooDto>> GetEmployeeRedis(int set, bool isDelete = false);
        Task<List<PaymentMethodOdooDto>> GetPaymentMethodRedis(int set, bool isDelete = false);
        Task<IEnumerable<ProductOdooDto>> GetProductProductRedis(int set, bool isDelete = false);
        Task<IEnumerable<PromoHeaderOdooDto>> GetPromoHeaderRedis(int set, bool isDelete = false);
        Task<IEnumerable<Pos_Staging>> GetListOrderRedis(string redis_server, string port, int location_id);
        Task<List<int>> GetPayment_VCM_Redis(List<int> method, bool isDelete = false);
        Task<List<int>> GetPayment_VCM_Detail_Redis(List<int> method, bool isDelete = false);
        public Task<List<CpnVchBOMHeaderDto>> GetCpnVchBOMHeaderAsync(bool isDelete = false);
        public Task<List<CpnVchBOMLineDto>> GetCpnVchBOMLineAsync(bool isDelete = false);
        public Task<bool> DeleteRedisCachePartnerAsync(WebApiModel _webApiAirPayInfo, string keyRedis);
    }
}
