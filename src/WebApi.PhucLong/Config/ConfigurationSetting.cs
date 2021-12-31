using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.PhucLong.API.Config
{
    public class ConfigurationSetting
    {
        public bool UseRedisCache { get; set; }
        public string RedisConnectionString { get; set; }
    }
}
