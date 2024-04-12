using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using VCM.Common.Database;
using VCM.Common.Helpers;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.Common.Const;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Shared.API;
using VCM.Shared.Dtos.WinMoney;
using WebApi.Core.AppServices;
using WebApi.Core.Shared;

namespace VCM.Partner.API.Application.Implementation
{
    public class WinMoneyService : IWinMoneyService
    {
        private readonly ILogger<WinMoneyService> _logger;
        private readonly IKibanaService _kibanaService;
        private readonly IMemoryCacheService _memoryCacheService;
        public WinMoneyService
            (
                IKibanaService kibanaService,
                ILogger<WinMoneyService> logger,
                IMemoryCacheService memoryCacheService
            ) 
        {
            _kibanaService = kibanaService;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
        }

        public ResponseClient RspURL_WMC(WebApiViewModel webApi, POSRequestUrl_WMC request)
        {
            ResponseClient resultObj = new ResponseClient();
            string errMsg = "";
            try
            {
                var requestWMC = new GetTokenWMC()
                {
                    Cashier_id = request.CashierID,
                    Cashier_name = request.CashierName,
                    Pos_browser_uid = request.PosBrowserUID,// Md5Utils.CalculateMD5Hash(request.PosBrowserUID),
                    Store_no = request.StoreNo,
                    Pos_no = request.PosNo,
                    Merchant_no = request.MerchantNo
                };

                var router = webApi.WebRoute.Where(x => x.Name.ToUpper() == "GetToken".ToUpper()).FirstOrDefault();
                var url_login = webApi.WebRoute.FirstOrDefault(x => x.Name == "Login");

                if ( router == null || url_login == null )
                {
                    return ResponseHelper.RspNotWarning(404, "WinMoney chưa được khai báo URL, vui lòng liên hệ IT");
                }

                string url_request = webApi.Host + router.Route;
                string jsonString = ConvertHelper.ObjectToStringLowercase(requestWMC);
               
                ApiHelper api = new ApiHelper(
                   url_request,
                   "",
                   "",
                   "POST",
                   jsonString,
                   false,
                   webApi.HttpProxy,
                   new string[] { webApi.Bypasslist }
                   );

                _kibanaService.LogRequest(NetwordHelper.GetHostName(), jsonString);
                long milliseconds = 0;
                var st1 = new Stopwatch();
                st1.Start();

                var rs = api.InteractWithApiResponse(ref errMsg);

                if (string.IsNullOrEmpty(errMsg) || errMsg == HttpStatusCode.OK.ToString())
                {
                    using Stream stream = rs.GetResponseStream();
                    StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
                    var result = streamReader.ReadToEnd();
                    _kibanaService.LogResponse(url_request, NetwordHelper.GetHostName(), result, milliseconds);
                    var check = JsonConvert.DeserializeObject<Response_WMC>(result);
                    if(check != null && check.Error_code == "1")
                    {
                        var data_WMC_Rsp = JsonConvert.DeserializeObject<Url_Success_WMC>(result);
                        data_WMC_Rsp.Data.Token = url_login.Route + request.CashierID + @"&token=" + data_WMC_Rsp.Data.Token;
                        resultObj = ResponseHelper.RspOK(data_WMC_Rsp);
                    }
                    else if(check != null && check.Error_code != "1")
                    {
                        var msgConfig = _memoryCacheService.GetNotifyConfig().Result.Where(x=>x.AppCode == "WMC").ToList();
                        if(msgConfig != null && msgConfig.Count > 0)
                        {
                            resultObj = ResponseHelper.RspNotWarning(400, MessageHelper.GetMsgConfig(msgConfig, "", check.Error_code));
                        }
                        else
                        {
                            resultObj = ResponseHelper.RspNotWarning(400, result);
                        }
                    }
                }
                else
                {
                    resultObj = ResponseHelper.RspNotWarning(400, errMsg);
                }

                milliseconds = st1.ElapsedMilliseconds;
                st1.Stop();

                _kibanaService.LogResponse(url_request, NetwordHelper.GetHostName(), errMsg, milliseconds);
            }
            catch (Exception ex)
            {
                resultObj.Meta = ExceptionConst.ExceptionTryCatch(ex);
                _logger.LogWarning("CheckCampaigns Exception: " + errMsg + "@@@" + ex.Message.ToString());
            }
            return resultObj;
        }
        public List<CashierInfoWMC> GetCashierInfo(string _connectCentralMD, string storeNo)
        {
            try
            {
                DapperContext dapperDbContext = new DapperContext(_connectCentralMD);
                using var conn = dapperDbContext.CreateConnDB;
                conn.Open();
                string query = @"SELECT StoreNo, [ID] CashierID, [FirstName] CashierName, '' CashierPhone, IIF(ISNULL(Blocked, 1) = 0, 1, 0) CashierStatus,0 CashierGender, VoidTransaction CashierJob " +
                                "FROM [dbo].[Staff] (NOLOCK) "+
                                "WHERE StoreNo = '" + storeNo + "' AND SalesPerson IS NOT NULL;";
                var data = conn.Query<CashierInfoWMC>(query).ToList();

                return data;
            }
            catch
            {
                return null;
            }
        }
        public List<PosInfoWMC> GetPosInfo(string _connectCentralMD, string storeNo)
        {
            try
            {
                DapperContext dapperDbContext = new DapperContext(_connectCentralMD);
                using var conn = dapperDbContext.CreateConnDB;
                conn.Open();
                string query = @"SELECT StoreNo, [No] PosID, [Status] PosStatus FROM POSTerminal (NOLOCK) WHERE  StoreNo = '" + storeNo + "' ORDER BY [No];";
                var data = conn.Query<PosInfoWMC>(query).ToList();

                return data;
            }
            catch
            {
                return null;
            }
        }
        public List<StoreInfoWMC> GetStoreInfo(string _connectCentralMD)
        {
            try
            {
                DapperContext dapperDbContext = new DapperContext(_connectCentralMD);
                using var conn = dapperDbContext.CreateConnDB;
                conn.Open();
                string query = @"SELECT [No] StoreNo, [Name] StoreName,  PhoneNo StorePhone, IIF(ClosingMethod = 1, 0, 1) StoreStatus, IIF(StyleProfile = 'VM', 'WMT', 'WMP') StoreType " +
                               " FROM Store (NOLOCK) WHERE LEFT([No],2) NOT IN ('11','12','13','14','10') " +
                               " ORDER BY [No];";
                var data = conn.Query<StoreInfoWMC>(query).ToList();

                return data;
            }
            catch
            {
                return null;
            }
        }
    }
}
