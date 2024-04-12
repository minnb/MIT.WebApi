using System.Data;
using System.Data.SqlClient;

namespace WCM.EntityFrameworkCore.EntityFrameworkCore.Dapper
{
    public class DapperContext
    {
        private readonly string _connectionStrings;
        public DapperContext(string connectionStrings)
        {
            _connectionStrings = connectionStrings;
        }
        public IDbConnection CreateConnDB
        {
            get
            {
                return new SqlConnection(_connectionStrings);
            }
        }
    }
}
