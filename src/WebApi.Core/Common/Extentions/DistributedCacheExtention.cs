using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text;
using System.Threading.Tasks;
using VCM.Shared.Const;

namespace WebApi.Core.Common.Extentions
{
    public static class DistributedCacheExtention
    {
        public static async Task SetDistributedCacheAsync(IDistributedCache _distributeCache, double fromHours, string key, byte[] data)
        {
            var options = new DistributedCacheEntryOptions()
                                   .SetSlidingExpiration(TimeSpan.FromMinutes(fromHours));
            await _distributeCache.SetAsync(key, data, options);
        }

        public static async Task<string> GetDistributedCacheAsync(IDistributedCache _distributeCache, string key)
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
    }
}
