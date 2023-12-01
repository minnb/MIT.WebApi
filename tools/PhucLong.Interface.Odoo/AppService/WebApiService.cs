using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhucLong.Interface.Odoo.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Tools.Common.Models.CRX_PLH;
using VCM.Common.Helpers;
using VCM.Shared.Entity.Central;
using VCM.Shared.Entity.Partner;

namespace PhucLong.Interface.Odoo.AppService
{
    public class WebApiService
    {
        private IConfiguration _config;
        private readonly ConndbNpgsql _dbOdoo;
        private ConndbCentral _dbContext;
        private PosRawService posRawService;
        public WebApiService
           (
               IConfiguration config
           )
        {
            _config = config;
            _dbOdoo = new ConndbNpgsql(_config);
            posRawService = new PosRawService(_config);
            _dbContext = new ConndbCentral(_config);
        }
        public void CallApi(IDbConnection conn, InterfaceEntry interfaceEntry)
        {
            string[] scheduler = null;
            string connectString = interfaceEntry.Prefix.ToString();
            string pathLog = interfaceEntry.LocalPath.ToString();

            if (!string.IsNullOrEmpty(interfaceEntry.Scheduler.ToString()))
            {
                scheduler = StringHelper.SliptString(interfaceEntry.Scheduler.ToString(), ";");
            }

            if (scheduler.Contains(DateTime.Now.ToString("HH")))
            {
                if (DateTime.Now.ToString("HH") == "08")
                {
                    //dữ liệu all salesPrice
                    SyncSalesPriceToCRX(conn, connectString, pathLog, "A");
                }
                else
                {
                    //urgent
                    SyncSalesPriceToCRX(conn, connectString, pathLog, "D");
                }
            }
        }
        private void SyncSalesPriceToCRX(IDbConnection conn, string connecString, string pathLog, string typeSales)
        {
            try
            {
                string time = DateTime.Now.ToString("yyyyMMdd_HH");
                string logfile = pathLog + typeSales + "_SyncSalesPriceToCRX_" + time + ".txt";
                if (File.Exists(logfile))
                {
                    FileHelper.WriteLogs("Exists: " + logfile);
                    return;
                }
                FileHelper.WriteLogs("===>>> SyncSalesPriceToCRX: " + typeSales + " Date: " + time);
                WebApiService webApiService = new WebApiService(_config);
                var lstWebApi = webApiService.GetWebApiInfo(conn, "CRX");
                var apiVoucher = lstWebApi.WebRoute.Where(x => x.Name == "SyncItemPrices").FirstOrDefault();

                if (lstWebApi != null && apiVoucher != null)
                {
                    GetDataService getDataService = new GetDataService(_config);

                    var lstSalesPrice = getDataService.GetSalesPriceOdoo(connecString, typeSales);

                    FileHelper.WriteLogs("lstSalesPrice: " + lstSalesPrice.Count.ToString());
                    FileHelper.WriteLogs("Url: " + lstWebApi.Host + apiVoucher.Route);
                    if (lstSalesPrice.Count > 0)
                    {
                        List<SyncSalesPrice> lstItems = new List<SyncSalesPrice>();
                        foreach (var item in lstSalesPrice)
                        {
                            lstItems.Add(item);
                        }

                        SyncSalesPrice_CRX bodyData = new SyncSalesPrice_CRX()
                        {
                            Items = lstItems
                        };

                        RestShapHelper apiHelper = new RestShapHelper(
                                   lstWebApi.Host,
                                   apiVoucher.Route,
                                   "POST",
                                   null,
                                   null,
                                   bodyData
                               );
                        string mess_error = "";
                        int statusRsp = 0;
                        var result = apiHelper.InteractWithApi(ref statusRsp, ref mess_error);

                        //FileHelper.WriteLogs("Response: " + result);
                        RspHttpStatusCode_CRX rsp = JsonConvert.DeserializeObject<RspHttpStatusCode_CRX>(result);
                        if (rsp != null)
                        {
                            if (statusRsp == 200)
                            {
                                FileHelper.WriteLogs("DONE: " + JsonConvert.SerializeObject(rsp));
                                File.Create(logfile).Dispose();
                            }
                            else
                            {
                                FileHelper.WriteLogs(JsonConvert.SerializeObject(rsp));
                                FileHelper.WriteLogs(mess_error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("UpdateStatusVoucherToCX Exception" + ex.Message.ToString());
            }
        }
        private WebApiModel GetWebApiInfo(IDbConnection conn, string appCode)
        {
            try
            {
                var api = conn.Query<SysWebApi>(@"SELECT [Id],[AppCode],[Host],[Description],[UserName],[Password],[PublicKey],[PrivateKey],[Blocked],[Version]  
                                                        FROM [dbo].[WebApi] (NOLOCK)
                                                        WHERE [Blocked] = 0 AND AppCode = '" + appCode.ToUpper() + @"'  ORDER BY Id;").ToList().FirstOrDefault();

                var routeApi = conn.Query<SysWebRoute>(@"SELECT [Id],[AppCode],[Name],[Route],[Description],[Blocked],[Version],[Notes]  FROM [dbo].[WebRoute] (NOLOCK) WHERE AppCode = '" + appCode.ToUpper() + @"';").ToList();

                if (api != null)
                {
                    return new WebApiModel()
                    {
                        Id = api.Id,
                        Host = api.Host,
                        AppCode = api.AppCode,
                        UserName = api.UserName,
                        Password = api.Password,
                        PublicKey = api.PublicKey,
                        PrivateKey = api.PrivateKey,
                        Blocked = api.Blocked,
                        Description = api.Description,
                        WebRoute = routeApi?.Where(x => x.AppCode == api.AppCode).ToList()
                    };
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
    public class WebApiModel : SysWebApi
    {
        public List<SysWebRoute> WebRoute { get; set; }
    }
}
