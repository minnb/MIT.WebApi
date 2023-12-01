using Newtonsoft.Json;
using StackExchange.Redis;
using System;

namespace WebApi.Core.AppServices.RedisService
{
    public class StackExchangeRedisService : IStackExchangeRedisService
    {
        private readonly IDatabase _redis;
        public StackExchangeRedisService(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public T Get<T>(string key)
        {
            var value = _redis.StringGet(key);
            return value.IsNull ? default : JsonConvert.DeserializeObject<T>(value);
        }

        public T Set<T>(string key, T value, TimeSpan? timeOut = null)
        {
            return _redis.StringSet(key, JsonConvert.SerializeObject(value), timeOut) ? value : default;
        }
    }
}
