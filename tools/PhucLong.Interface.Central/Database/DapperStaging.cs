using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace PhucLong.Interface.Central.Database
{
    public class DapperStaging
    {
        private readonly string _connectString;
        public DapperStaging(string connectString)
        {
            _connectString = connectString;
        }

        public IDbConnection ConnDapperStaging
        {
            get
            {
                return new SqlConnection(_connectString);
            }
        }
    }
}
