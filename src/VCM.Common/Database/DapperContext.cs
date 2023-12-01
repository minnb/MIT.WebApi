using System.Data;
using System.Data.SqlClient;

namespace VCM.Common.Database
{
    public class DapperContext
    {
        private string _connectionStrings;
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
