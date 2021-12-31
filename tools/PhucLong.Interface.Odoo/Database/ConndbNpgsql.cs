using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace PhucLong.Interface.Odoo.Database
{
    public class ConndbNpgsql
    {
        private IConfiguration _config;
        public ConndbNpgsql
            (
             IConfiguration config
            )
        {
            _config = config;
        }

        public IDbConnection GetConndbNpgsql(string connectString)
        {
            return new NpgsqlConnection(connectString);
        }
    }
}
