using Microsoft.Extensions.Configuration;
using MIT.Utils;
using PhucLong.Interface.Odoo.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PhucLong.Interface.Odoo.AppService
{
    public class ConfigService
    {

        private readonly IConfiguration _config;
        private readonly ConndbRedis _conndbRedis;
        public ConfigService
            (
             IConfiguration config
            )
        {
            _config = config;
            _conndbRedis = new ConndbRedis(_config);
        }

        
    }
}
