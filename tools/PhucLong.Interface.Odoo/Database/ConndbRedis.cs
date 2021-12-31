using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace PhucLong.Interface.Odoo.Database
{
    public class ConndbRedis
    {
        private IConfiguration _config;
        public ConndbRedis
            (
                IConfiguration config
            )
        {
            _config = config;
        }

        public IDatabase GetRedisServer()
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(
                        new ConfigurationOptions
                        {
                            EndPoints = { _config["RedisServer:Host"] + @":" + _config["RedisServer:Port"] }
                        });
            return redis.GetDatabase();
        }
    }
}
