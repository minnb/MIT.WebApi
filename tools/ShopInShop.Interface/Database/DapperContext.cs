using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ShopInShop.Interface.Database
{
    public class DapperContextHFH
    {
        private readonly IConfiguration _configuration;
        public DapperContextHFH(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection ConnCentralSalesStaging
        {
            get
            {
                return new SqlConnection(_configuration["ConnectionStrings:CentralSalesStaging"]);
            }
        }
        public IDbConnection ConnEInvoice
        {
            get
            {
                return new SqlConnection(_configuration["ConnectionStrings:EInvoice"]);
            }
        }
        public IDbConnection ConnPartnerDB
        {
            get
            {
                return new SqlConnection(_configuration["ConnectionStrings:PartnerDB"]);
            }
        }
    }
}
