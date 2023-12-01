using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.PhucLong.API.Database
{
    public class DapperOdooContext
    {
        private readonly IConfiguration _configuration;
        public DapperOdooContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IDbConnection CreateConnection(int set)
        {
            string connectString = string.Empty;
            switch (set)
            {
                case 1:
                    connectString = _configuration.GetConnectionString("ERP01");
                    break;
                case 2:
                    connectString = _configuration.GetConnectionString("ERP02");
                    break;
                case 3:
                    connectString = _configuration.GetConnectionString("ERP03");
                    break;
                case 4:
                    connectString = _configuration.GetConnectionString("ERP04");
                    break;
            }
            return new NpgsqlConnection(connectString);
        }
        public IDbConnection CreateConnDB
        {
            get
            {
                return new SqlConnection(_configuration.GetConnectionString("CentralMD"));
            }
        }

    }
}

