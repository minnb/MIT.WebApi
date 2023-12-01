using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCM.Shared.Const;
using VCM.Shared.Entity.Partner;
using WCM.EntityFrameworkCore.EntityFrameworkCore.PriceEngine;
using WebApi.PriceEngine.Application.Interfaces;

namespace WebApi.PriceEngine.Application.Implementation
{
    public class RedisCacheService: IRedisCacheService
    {
        private readonly ILogger<RedisCacheService> _logger;
        private readonly PriceEngineDbContext _dbContext;
        private readonly IDistributedCache _distributeCache;
        public RedisCacheService(
            ILogger<RedisCacheService> logger,
            PriceEngineDbContext dbContext,
            IDistributedCache distributeCache
        )
        {

            _logger = logger;
            _dbContext = dbContext;
            _distributeCache = distributeCache;
        }
        private async Task SetCacheRedisAsync(string key_redis, byte[] data)
        {
            var options = new DistributedCacheEntryOptions()
                                   .SetSlidingExpiration(TimeSpan.FromHours(RedisConst.Redis_cache_time));
            await _distributeCache.SetAsync(key_redis, data, options);
        }
        public async Task<string> GetRedisValueAsync(string key)
        {
            var redis_data = await _distributeCache.GetAsync(key);
            if (redis_data != null)
            {
                return Encoding.UTF8.GetString(redis_data);
            }
            else
            {
                return null;
            }
        }
        public async Task SetRedisKeyAsync(string key, string value)
        {
            var checkKey = await _distributeCache.GetAsync(key);
            if (checkKey == null)
            {
                await SetCacheRedisAsync(key, Encoding.UTF8.GetBytes(value));
            }
        }
        public async Task<List<UserRoles>> GetUserRolesAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_webapi_sys_user_roles);
            }
            var key_redis = await _distributeCache.GetAsync(RedisConst.Redis_cache_webapi_sys_user_roles);
            if (key_redis != null)
            {
                return JsonConvert.DeserializeObject<List<UserRoles>>(Encoding.UTF8.GetString(key_redis));
            }
            else
            {
                var data = _dbContext.UserRoles.Where(x => x.Blocked == false).ToList();
                if (data.Count > 0)
                {
                    await SetCacheRedisAsync(RedisConst.Redis_cache_webapi_sys_user_roles, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                }
                return data;
            }
        }
        public async Task<List<SysConfig>> GetDataSysConfigAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_webapi_sys_config);
            }
            var key_redis = await _distributeCache.GetAsync(RedisConst.Redis_cache_webapi_sys_config);
            if (key_redis != null)
            {
                return JsonConvert.DeserializeObject<List<SysConfig>>(Encoding.UTF8.GetString(key_redis));
            }
            else
            {
                var data = _dbContext.SysConfig.Where(x => x.Blocked == false).ToList();
                if (data.Count > 0)
                {
                    await SetCacheRedisAsync(RedisConst.Redis_cache_webapi_sys_config, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                }
                return data;
            }
        }
        public async Task<byte[]> GetDataByteAsync(string key)
        {
            return await _distributeCache.GetAsync(key);
        }
        public async Task<List<SysStoreSet>> GetDataSysStoreSetAsync(bool isDelete = false)
        {
            if (isDelete)
            {
                await _distributeCache.RemoveAsync(RedisConst.Redis_cache_webapi_store_set);
            }
            var key_redis = await _distributeCache.GetAsync(RedisConst.Redis_cache_webapi_store_set);
            if (key_redis != null)
            {
                return JsonConvert.DeserializeObject<List<SysStoreSet>>(Encoding.UTF8.GetString(key_redis));
            }
            else
            {
                var data = _dbContext.SysStoreSet.Where(x => x.Blocked == false).ToList();
                if (data.Count > 0)
                {
                    await SetCacheRedisAsync(RedisConst.Redis_cache_webapi_store_set, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
                }
                return data;
            }
        }
    }
}
