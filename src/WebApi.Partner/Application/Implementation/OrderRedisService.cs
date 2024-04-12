using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCM.Partner.API.Application.Interfaces;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Partner;

namespace VCM.Partner.API.Application.Implementation
{
    public class OrderRedisService: IOrderRedisService
    {
        private readonly ILogger<OrderRedisService> _logger;
        private readonly PartnerDbContext _dbContext;
        private readonly IDistributedCache _distributeCache;
        public OrderRedisService
            (
                ILogger<OrderRedisService> logger,
                PartnerDbContext dbContext,
                IDistributedCache distributeCache
            )
        {
            _logger = logger;
            _dbContext = dbContext;
            _distributeCache = distributeCache;
        }

        public async Task<string> GetOrderRedisAsync(string orderNo)
        {
            var redis_data = await _distributeCache.GetAsync(orderNo);
            if (redis_data != null)
            {
                return Encoding.UTF8.GetString(redis_data);
            }
            else
            {
                return null;
            }
        }
        public async Task SetOrderRedisAsync(string orderNo, string value)
        {
            if (await _distributeCache.GetAsync(orderNo) == null)
            {
                var options = new DistributedCacheEntryOptions()
                                  .SetSlidingExpiration(TimeSpan.FromHours(12));
                await _distributeCache.SetAsync(orderNo, Encoding.UTF8.GetBytes(value), options);
            }
        }
        public async Task DelOrderRedisAsync(string orderNo)
        {
            await _distributeCache.RemoveAsync(orderNo);
        }
        public int CoutedOrderByStoreAsync(string redis_server, string port, string partnerCode, string appCode, string storeNo)
        {
            int result = 0;
            if (string.IsNullOrEmpty(redis_server))
            {
                return result;
            }

            string countIf = partnerCode + "_" + storeNo;
            var redisConn = redis_server.Split(":").ToArray();
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redis_server + @", allowAdmin=true"))
            {
                IDatabase db = redis.GetDatabase();

                var keys = redis.GetServer(redisConn[0].ToString(), int.Parse(port)).Keys();

                string[] keysArr = keys.Select(key => (string)key).ToArray();

                foreach (string key in keysArr)
                {
                    if (key.Contains(countIf))
                    {
                        result++;
                    }
                }
            }

            return result;
        }
    }
}
