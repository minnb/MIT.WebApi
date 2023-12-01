using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tools.Common.Database
{
    public class ConndbRedis
    {
        private IConfiguration _config;
        private string _stringConnectRedis = string.Empty;
        private string _hostRedis = string.Empty;
        private int _portRedis = 6379;
        private string _databaseRedis = "";
        public ConndbRedis
            (
                IConfiguration config
            )
        {
            _config = config;
            _hostRedis = _config["RedisServer:Host"];
            _portRedis = int.Parse(_config["RedisServer:Port"]);
            _databaseRedis = _config["RedisServer:Database"];
            _stringConnectRedis = _hostRedis + @":" + _portRedis.ToString() + "," + _databaseRedis;
        }

        public List<string> GetAllkeys()
        {
            List<string> listKeys = new List<string>();

            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_stringConnectRedis + ",allowAdmin=true"))
            {
                var keys = redis.GetServer(_hostRedis, _portRedis).Keys();
                listKeys.AddRange(keys.Select(key => (string)key).ToList());

            }

            return listKeys;
        }

        public string StringSetKey(string key, string value)
        {
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_stringConnectRedis + ",allowAdmin=true"))
            {
                IDatabase db = redis.GetDatabase();
                //db.StringSet(key, value, TimeSpan.FromDays(1));

                HashEntry[] countryMasterObj = {
                      new HashEntry("Code", "IND"),
                     new HashEntry("Language", "Hindi"),
                     new HashEntry("Name", "INDIA")
                     };
                db.HashSet("CountryMasterKey", countryMasterObj);

            }
            
            return key;
        }

        public string StringGetValue(string key)
        {
            string value = "";
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_stringConnectRedis + ",allowAdmin=true"))
            {
                IDatabase db = redis.GetDatabase();
                value = db.StringGet(key);
            }

            return value;
        }
    }
}
