using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Tools.Common.Database;
using Tools.PriceEngine.Database;
using VCM.Shared.Const;

namespace Tools.PriceEngine.AppServices
{
    public class MasterDataService
    {
        private readonly DatabaseContext _dbContext;
        private readonly ConndbRedis _conndbRedis;
        private IConfiguration _config;
        public MasterDataService
           (
               DatabaseContext dbContext,
               IConfiguration config
           )
        {
            _dbContext = dbContext;
            _config = config;
            _conndbRedis = new ConndbRedis(_config);
        }

        public void GetKeyRedis()
        {
            var key = _conndbRedis.GetAllkeys();

            _conndbRedis.StringSetKey("test", "test-value");

            var value = _conndbRedis.StringGetValue("test");
        }

    }
}
