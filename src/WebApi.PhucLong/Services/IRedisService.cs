using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VCM.Shared.Dtos.POS;
using VCM.Shared.Entity.PhucLong;

namespace WebApi.PhucLong.Services
{
    public interface IRedisService
    {
        public Task<string> GetRedisValueAsync(string key);
        public Task SetRedisKeyAsync(string key, string value);
        public Task RemoveRedisKeyAsync(string key);
        Task<List<ResPartnerOdooDto>> GetMemberRedis(bool isDelete = false);
        Task<List<StoreInfoOdooDto>> GetPosConfigRedis(bool isDelete = false);
        Task<List<EmployeeOdooDto>> GetEmployeeRedis(bool isDelete = false);
        Task<List<PaymentMethodOdooDto>> GetPaymentMethodRedis(bool isDelete = false);
        Task<IEnumerable<ProductOdooDto>> GetProductProductRedis(bool isDelete = false);
        Task<IEnumerable<PromoHeaderOdooDto>> GetPromoHeaderRedis(bool isDelete = false);
        Task<IEnumerable<Pos_Staging>> GetListOrderRedis(string redis_server, string port, int location_id);
        Task<List<int>> GetPayment_VCM_Redis(List<int> method, bool isDelete = false);
    }
}
