using Newtonsoft.Json;
using StackExchange.Redis;
using System;

namespace WebApi.Core.AppServices.RedisService
{
    public interface IStackExchangeRedisService
    {
        T Get<T>(string key);
        T Set<T>(string key, T value, TimeSpan? timeOut = null);
    }

}
