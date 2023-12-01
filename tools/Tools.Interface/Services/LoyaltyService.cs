using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tools.Common.Models.Loyalty;
using VCM.Common.Helpers;
using VCM.Shared.Dtos.POS;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Dapper;

namespace ShopInShop.Interface.Services
{
    public class LoyaltyService
    {
        private readonly IConfiguration _configuration;
        private DapperContext _dapperContext;
        public LoyaltyService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

       public async Task GetDataRawLoyaltyAsync(string localPath, string localPathArchived)
        {
            string url = _configuration["Loyalty:Host"].ToString();
            string route = _configuration["Loyalty:Route"].ToString();
            string connectionString = "";
            _dapperContext = new DapperContext(connectionString);
            using (IDbConnection conn = _dapperContext.CreateConnDB)
            {
                conn.Open();
                try
                {
                    Console.WriteLine("SELECT SALES");
                    string query = @"EXEC SP_GET_LOYALTY_PLG; ";

                    var transRaw = conn.Query<TransLoyalty>(query).ToList();
                    Console.WriteLine("SELECTED: " + transRaw.Count.ToString());
                    if (transRaw.Count > 0)
                    {                       
                        foreach(var item in transRaw)
                        {
                            if(!string.IsNullOrEmpty(item.Phone) && RegularHelper.ValidatePhoneNumber(item.Phone, false))
                            {
                                var storeId = string.Empty;
                                if (!string.IsNullOrEmpty(item.PosNo))
                                {
                                    storeId = item.PosNo.Substring(0, item.PosNo.Length - 2);
                                }
                                
                                var loyDto =  new LoyaltyDto()
                                {
                                    QRCode = "",
                                    CardNumber = RegularHelper.RemoveNonNumeric(item.Phone),
                                    MerchantId = storeId,
                                    TerminalId = item.PosNo,
                                    InvoiceNo = item.OrderNo,
                                    OrderNo = item.OrderNo,
                                    SpendPoints = 0,
                                    BillAmount = item.AmountInclVAT,
                                    OrderAmount = item.AmountInclVAT,
                                    IsOffline = true,
                                    VirtualCard = ""
                                };

                                FileHelper.CreateFileMaster(loyDto.OrderNo, "LOY", localPath, JsonConvert.SerializeObject(loyDto));
                            }
                        }
                        FileHelper.WriteLogs("Processed: " + transRaw.Count.ToString());
                    }

                    var lstFile = FileHelper.GetFileFromDir(localPath, "*.txt");
                    FileHelper.WriteLogs("Scan: " + localPath + "===> " + lstFile.Count.ToString());
                    if (lstFile.Count > 0)
                    {
                        foreach (string file in lstFile)
                        {
                            if (file.ToString().Substring(0, 3).ToUpper() == "LOY" && !string.IsNullOrEmpty(url))
                            {
                                var loy = JsonConvert.DeserializeObject<LoyaltyDto>(System.IO.File.ReadAllText(localPath + file));
                                if (loy != null)
                                {
                                    var param = route + @"?QRCode=";
                                    param += @"&CardNumber=" + loy.CardNumber;
                                    param += @"&MerchantId=" + loy.MerchantId;
                                    param += @"&TerminalId=" + loy.TerminalId;
                                    param += @"&InvoiceNo=" + loy.OrderNo;
                                    param += @"&SpendPoints=" + loy.SpendPoints;
                                    param += @"&BillAmount=" + loy.BillAmount;
                                    param += @"&OrderNo=" + loy.OrderNo;
                                    param += @"&IsOffline=False";
                                    param += @"&OrderAmount=" + loy.OrderAmount;
                                    param += @"&VirtualCard=";

                                    RestShapHelper apiHelper = new RestShapHelper(
                                        url,
                                        param,
                                        "GET",
                                        null,
                                        null,
                                        null,
                                        30000,
                                        null,
                                        0
                                    );

                                    int test = 0;
                                    string mess_errors = string.Empty;
                                    var result = apiHelper.InteractWithApi(ref test,ref mess_errors);

                                    RspLoyalty rsp = JsonConvert.DeserializeObject<RspLoyalty>(result);
                                    FileHelper.WriteLogs(result);

                                    if (rsp != null)
                                    {
                                        if (rsp.Status == 200 || rsp.Status == 409)
                                        {
                                            if(rsp.Status == 200)
                                            {
                                                await SaveLogging(conn, rsp, loy);
                                            }
                                            FileHelper.MoveFileToDestination(localPath + file, localPathArchived);
                                        }
                                        FileHelper.WriteLogs("OrderNo: " + loy.OrderNo + " ==> " + JsonConvert.SerializeObject(rsp));
                                    }
                                    else 
                                    {
                                        FileHelper.WriteLogs(JsonConvert.SerializeObject(rsp));
                                        var errors = localPathArchived + @"Errors\";
                                        FileHelper.WriteLogs("Move to: " + errors);
                                        FileHelper.MoveFileToDestination(localPath + file, errors);
                                    }
                                    Thread.Sleep(500);
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    FileHelper.WriteLogs("GetDataRawLoyaltyAsync Exception" + ex.Message.ToString());
                }
            }
        }
        private async Task SaveLogging(IDbConnection conn, RspLoyalty rsp, LoyaltyDto loyalty)
        {
            using var transaction = conn.BeginTransaction();
            try
            {
                LoyaltyLogging logging = new LoyaltyLogging()
                {
                    CardNumber = loyalty.CardNumber,
                    OrderNo = loyalty.OrderNo,
                    BillAmount = loyalty.BillAmount,
                    UpdateFlg = "N",
                    Status = rsp.Status,
                    Message = rsp.Message,
                };
                string insLog = @"INSERT INTO dbo.PL_Loyalty_Logs
		                                    (CardNumber,OrderNo,BillAmount,UpdateFlg,Status,Message,CrtDate,Id)
                                    VALUES (@CardNumber,@OrderNo,@BillAmount,@UpdateFlg,@Status,@Message,getdate(),newid())";
                await conn.ExecuteAsync(insLog, logging, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
            }
        }
    }
    public class TransLoyalty
    {
        public string OrderNo { get; set; }
        public string PosNo { get; set; }
        public string Phone { get; set; }
        public decimal AmountInclVAT { get; set; }
    }
}