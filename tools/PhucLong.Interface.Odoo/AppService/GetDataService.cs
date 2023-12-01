using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhucLong.Interface.Odoo.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using Tools.Common.Models.CRX_PLH;
using VCM.Common.Helpers;

namespace PhucLong.Interface.Odoo.AppService
{
    public class GetDataService
    {
        private IConfiguration _config;
        private readonly ConndbNpgsql _dbOdoo;
        private ConndbCentral _dbContext;
        private PosRawService posRawService;
        public GetDataService(IConfiguration config)
        {
            _config = config;
            _dbOdoo = new ConndbNpgsql(_config);
            posRawService = new PosRawService(_config);
            _dbContext = new ConndbCentral(_config);
        }
       
        public List<SyncSalesPrice> GetSalesPriceOdoo(string connectString, string type)
        {
            try
            {
                using IDbConnection conn = _dbOdoo.GetConndbNpgsql(connectString);
                conn.Open();
                string query = string.Empty;
                if (type == "A")
                {
                    query = @"select 'ALL' as storeId, sap_code as articleId, name articleName, list_price as soldPrice, sap_uom unitCode, '' productBarcode, 
                                    '20210101' as fromDate,'20290101' as toDate
                                    from product_template t
                                    where active = true and sale_ok = true and sap_code is not null;";
                }
                else if(type == "D")
                {
                    query = @"select 'ALL' as storeId, sap_code as articleId, name articleName, list_price as soldPrice, sap_uom unitCode, '' productBarcode, 
                                    '20210101' as fromDate,'20290101' as toDate
                                    from product_template t
                                    where active = true and sale_ok = true and sap_code is not null
                                          and cast(write_date::timestamp AT TIME ZONE 'UTC' as date) >= cast(now() AT TIME ZONE 'UTC' as date) ;";
                }
                var data = conn.Query<SyncSalesPrice>(query).ToList();
                return data;

            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("GetSalesPriceOdoo Exception: " + ex.Message.ToString());
                return null;
            }
        }
    }
}
