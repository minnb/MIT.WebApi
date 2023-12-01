using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Partner.API.Application.Interfaces
{
    public interface IOrderRedisService
    {
        Task DelOrderRedisAsync(string orderNo);
        public Task SetOrderRedisAsync(string orderNo, string value);
        public Task<string> GetOrderRedisAsync(string orderNo);
        public int CoutedOrderByStoreAsync(string redis_server, string port, string partnerCode,string appCode, string storeNo);
    }
}
