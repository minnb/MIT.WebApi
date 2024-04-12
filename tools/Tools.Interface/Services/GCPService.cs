using Dapper;
using Newtonsoft.Json;
using PhucLong.Interface.Central.Models.GCP;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Tools.Interface.Models.GCP;
using Tools.Interface.Services.GCP;
using VCM.Common.Database;
using VCM.Common.Helpers;
using VCM.Shared.API.PhucLongV2;

namespace Tools.Interface.Services
{
    public class GCPService
    {
        private DapperContext _dapperContext;
        public GCPService() { }
        public void ExpTransVoid(string appCode, string connectionString, string procedure, string jobType, string pathLocal)
        {
            _dapperContext = new DapperContext(connectionString);
            using IDbConnection conn = _dapperContext.CreateConnDB;
            conn.Open();
            try
            {
                var transVoidHeader = conn.Query<TransVoidHeader>(@"EXEC " + procedure + " 'TransVoidHeader';", commandTimeout: 36000).ToList();
                var transVoidLine = conn.Query<TransVoidLine>(@"EXEC " + procedure + " 'TransVoidLine';", commandTimeout: 36000).ToList();

                if (transVoidLine.Count > 0 || transVoidHeader.Count > 0)
                {
                    var transVoidGCP = new TransVoidGCP()
                    {
                        TransVoidHeader = transVoidHeader.ToList(),
                        TransVoidLine = transVoidLine.ToList()
                    };
                    FileHelper.CreateFileMaster("", "GCP_TransVoid", pathLocal, JsonConvert.SerializeObject(transVoidGCP));
                }

            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(jobType, appCode + "===> ExpTransVoid Exception" + ex.Message.ToString());
            }
        }
        public void ExpHR_To_GCP(string appCode, string connectionString, string jobType, string pathLocal, string pathArchived, bool isMoveFile = true)
        {
            //10.235.19.71:50001 / sap-hcm/hr
            //10.235.19.71:50001 / sap-hcm/turnover
            //10.235.19.71:50001 / sap-hcm/training
            //10.235.19.71:50001 / sap-hcm/recruitment
            //10.235.19.71:50001 / sap-hcm/revenue-profit
            GCP_HR_Service serviceHR = new GCP_HR_Service();
            try
            {
                serviceHR.Exp_FromTable_MSN(appCode, connectionString, jobType, pathLocal);
                var lstFile = FileHelper.GetFileFromDir(pathLocal, "*.txt");
                if(lstFile.Count > 0)
                {
                    foreach (string file in lstFile)
                    {
                        if (file.ToUpper().Contains("MSN_HR".ToUpper()))
                        {
                            var bodyData = JsonConvert.DeserializeObject<List<MSN_HR>>(System.IO.File.ReadAllText(pathLocal + file));
                            if (bodyData.Count > 0)
                            {
                                if (CallApiGCP("sap-hcm/hr", bodyData, null, file))
                                {
                                    if (isMoveFile) FileHelper.MoveFileToFolder(pathArchived, pathLocal + file);
                                }
                            }
                        }
                        else if (file.ToUpper().Contains("MSN_Recruitment".ToUpper()))
                        {
                            var bodyData = JsonConvert.DeserializeObject<List<MSN_Recruitment>>(System.IO.File.ReadAllText(pathLocal + file));
                            if (bodyData.Count > 0)
                            {
                                if (CallApiGCP("sap-hcm/recruitment", bodyData, null, file))
                                {
                                    if (isMoveFile) FileHelper.MoveFileToFolder(pathArchived, pathLocal + file);
                                }
                            }
                        }
                        else if (file.ToUpper().Contains("MSN_RevenueProfit".ToUpper()))
                        {
                            var bodyData = JsonConvert.DeserializeObject<List<MSN_RevenueProfit>>(System.IO.File.ReadAllText(pathLocal + file));
                            if (bodyData.Count > 0)
                            {
                                if (CallApiGCP("sap-hcm/revenue-profit", bodyData, null, file))
                                {
                                    if (isMoveFile) FileHelper.MoveFileToFolder(pathArchived, pathLocal + file);
                                }
                            }
                        }
                        else if (file.ToUpper().Contains("MSN_Training".ToUpper()))
                        {
                            var bodyData = JsonConvert.DeserializeObject<List<MSN_Training>>(System.IO.File.ReadAllText(pathLocal + file));
                            if (bodyData.Count > 0)
                            {
                                if(bodyData.Count > 5000)
                                {
                                    foreach(MSN_Training training in bodyData)
                                    {
                                        CallApiGCP("sap-hcm/training", training, null, file);
                                    }
                                    if (isMoveFile) FileHelper.MoveFileToFolder(pathArchived, pathLocal + file);
                                }
                                else
                                {
                                    if (CallApiGCP("sap-hcm/training", bodyData, null, file))
                                    {
                                        if (isMoveFile) FileHelper.MoveFileToFolder(pathArchived, pathLocal + file);
                                    }
                                }
                            }
                        }
                        else if (file.ToUpper().Contains("MSN_Turnover".ToUpper()))
                        {
                            var bodyData = JsonConvert.DeserializeObject<List<MSN_Turnover>>(System.IO.File.ReadAllText(pathLocal + file));
                            if (bodyData.Count > 0)
                            {
                                if (CallApiGCP("sap-hcm/turnover", bodyData, null, file))
                                {
                                    if (isMoveFile) FileHelper.MoveFileToFolder(pathArchived, pathLocal + file);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                FileHelper.Write2Logs(jobType, "===> ExpHR_To_GCP Exception" + ex.Message.ToString());
            }
        }
        public void ExpCountTotalBill(string appCode, string connectionString, string procedure, string jobType, string pathLocal)
        {
            _dapperContext = new DapperContext(connectionString);
            using IDbConnection conn = _dapperContext.CreateConnDB;
            conn.Open();
            try
            {
                var dataBill = conn.Query<ExpCountTotalBillByDate>(@"EXEC " + procedure + ";", commandTimeout: 36000).ToList();
                if (dataBill.Count > 0)
                {
                    var listDate = dataBill
                                  .Select(x => new
                                  {
                                      x.OrderDate
                                  })
                                  .GroupBy(x => new { x.OrderDate})
                                  .Select(x =>
                                  {
                                      var temp = x.OrderByDescending(o => o.OrderDate).FirstOrDefault();
                                      return new
                                      {
                                          x.Key.OrderDate
                                      };
                                  }).ToList();

                    if (listDate.Count > 0)
                    {
                        int stt = 0;
                        foreach(var byDate in listDate)
                        {
                            stt++;
                            var dataExpExcel = dataBill.Where(x => x.OrderDate == byDate.OrderDate).ToList();
                            if(dataExpExcel.Count > 0)
                            {
                                var fileName = appCode + "_reconciliation_".ToUpper() + dataExpExcel.FirstOrDefault().OrderDate + "_GCP_" + stt.ToString() + ".csv";
                                var csv = new StringBuilder();
                                var allLines = (from trade in dataExpExcel
                                                select new object[]
                                                {
                                                    trade.StoreNo.ToString(),
                                                    trade.OrderDate,
                                                    trade.OrderNo.ToString()
                                                }).ToList();
                                allLines.ForEach(line =>
                                {
                                    csv.AppendLine(string.Join(";", line));
                                });

                                File.WriteAllText(pathLocal + fileName, csv.ToString());
                                FileHelper.WriteLogs("===> Created file csv: " + fileName);
                            }

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> ExpCountTotalBill Exception" + ex.Message.ToString());
            }
        }
        public bool Run_StoreProcedure(string connectionString, string procedure, string jobType)
        {
            _dapperContext = new DapperContext(connectionString);
            using IDbConnection conn = _dapperContext.CreateConnDB;
            conn.Open();
            try
            {
                FileHelper.WriteLogs("===> " + jobType + " start: " + procedure);
                conn.Query(@"EXEC " + procedure, commandTimeout: 36000);
                FileHelper.WriteLogs("===> " + jobType + " run procedure done");
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> Run_StoreProcedure.Exception" + ex.Message.ToString());
                return false;
            }
        }
        public void Push_Summary_SalesOut_WCM(string connectionString, string storeProcedure, string pathLog)
        {
            _dapperContext = new DapperContext(connectionString);
            using IDbConnection conn = _dapperContext.CreateConnDB;
            conn.Open();
            try
            {
                var dataSalesOutSum = conn.Query<SaleOutChecksumGCP>(storeProcedure).ToList();
                if(dataSalesOutSum != null)
                {
                    if (CallApiGCP("pos-wcm/sale-out-checksum", dataSalesOutSum, null))
                    {
                        dataSalesOutSum.ForEach(x => x.UpdateFlg = "Y");
                        FileHelper.CreateFileMaster("Summary_SalesOut", "Logs", pathLog, JsonConvert.SerializeObject(dataSalesOutSum));
                        FileHelper.WriteLogs("===> Push_SaleOutCheck_GCP.OK");
                    }
                    else
                    {
                        dataSalesOutSum.ForEach(x => x.UpdateFlg = "E");
                    }

                    using var transaction = conn.BeginTransaction();
                    try
                    {
                        conn.Execute("UPDATE SummarySalesOut SET UpdateFlg = @UpdateFlg WHERE AppCode = @AppCode AND CalendarDay = @CalendarDay;", dataSalesOutSum, transaction);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        FileHelper.WriteLogs("Push_SaleOutCheck_GCP.Exception.transaction.Rollback: " + ex.Message.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> Push_SaleOutCheck_GCP.Exception" + ex.Message.ToString());
            }
        }
        public int Exp_Trans_WCM_To_GCP(string connectionString, string procedure, string pathLocal, string batchJob, int max_rows)
        {
            int total_file = 0;
            _dapperContext = new DapperContext(connectionString);
            using IDbConnection conn = _dapperContext.CreateConnDB;
            conn.Open();
            try
            {
                var check = conn.Query<CheckSL>(@"SELECT COUNT(1) AS SL FROM (SELECT ReceiptNo FROM SUMD11_BLUE GROUP BY ReceiptNo) A;", commandTimeout: 36000).FirstOrDefault();
                if(check.SL > 0)
                {
                    int count = (int)Math.Ceiling(check.SL / 999);
                    FileHelper.WriteLogs("===> Count:" + count);
                    for (var k = 1; k <= count; k++)
                    {
                        string queryData = @"EXEC " + procedure + @" '" + batchJob + "'";
                        var lstSales = conn.Query<TransTempGCP>(queryData, commandTimeout: 36000).ToList();
                        if (lstSales.Count > 0)
                        {
                            var lstOrder = lstSales
                                    .Select(x => new
                                    {
                                        x.ReceiptNo
                                    })
                                    .GroupBy(x => new  { x.ReceiptNo })
                                    .Select(x =>
                                    {
                                        var temp = x.OrderByDescending(o => o.ReceiptNo).FirstOrDefault();
                                        return new
                                        {
                                            x.Key.ReceiptNo
                                        };
                                    }).ToList();

                            var lstDiscountEntry = conn.Query<TransDiscountGCP>(@"SELECT * FROM [SUMD11_DISCOUNT_BLUE] NOLOCK WHERE UpdateFlg ='N';", commandTimeout: 36000).ToList();
                            var lstPaymentEntry = conn.Query<TransPaymentEntryGCP>(@"SELECT [ReceiptNo],[LineNo],[ExchangeRate],[TenderType],[AmountTendered],[CurrencyCode],[AmountInCurrency],[ReferenceNo],[ApprovalCode],
                                                                                    [BankPOSCode],[BankCardType],[IsOnline] FROM [SUMD11_PAYMENT] NOLOCK;", 
                                                                                    commandTimeout: 36000).ToList();

                            var lstReceiptNo = lstOrder.Select(s => new LstReceiptNo { ReceiptNo = s.ReceiptNo }).ToList();
                            
                            FileHelper.WriteLogs("===> record of file: " + CreateFile_WCM_GCP(lstSales, lstReceiptNo, lstDiscountEntry, lstPaymentEntry, batchJob, pathLocal, max_rows).ToString());
                        }
                        total_file++;
                        FileHelper.WriteLogs("===> finished:" + k.ToString());
                    }
                    
                }
                
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> Exp_TransToGCP Exception" + ex.Message.ToString());
            }

            FileHelper.WriteLogs("===> Exp_TransToGCP.Created " + total_file.ToString() + " file");
            return total_file;
        }
        private int CreateFile_WCM_GCP(List<TransTempGCP> lstSales, List<LstReceiptNo> lstOrder, List<TransDiscountGCP> lstDiscountEntry, List<TransPaymentEntryGCP> lstPaymentEntry, string batchJob, string pathLocal, int max_rows)
        {
            int total_file = 0;
            int count = 0;
            var batchDetail = new List<TransGCPModel>();
            foreach (var order in lstOrder)
            {
                var lstReceiptNo = lstSales.Where(x => x.ReceiptNo == order.ReceiptNo).ToList();
                var transaction = new TransGCPModel()
                {
                    CalendarDay = lstReceiptNo.FirstOrDefault().CalendarDay,
                    StoreCode = lstReceiptNo.FirstOrDefault().StoreCode,
                    PosNo = lstReceiptNo.FirstOrDefault().PosNo,
                    TranTime = lstReceiptNo.FirstOrDefault().TranTime,
                    ReceiptNo = lstReceiptNo.FirstOrDefault().ReceiptNo,
                    MemberCardNo = lstReceiptNo.FirstOrDefault().MemberCardNo ?? "",
                    VinidCsn = lstReceiptNo.FirstOrDefault().VinidCsn ?? "",
                    Header_ref_01 = lstReceiptNo.FirstOrDefault().Header_ref_01 ?? "",
                    Header_ref_02 = lstReceiptNo.FirstOrDefault().Header_ref_02 ?? "",
                    Header_ref_03 = lstReceiptNo.FirstOrDefault().Header_ref_03 ?? "",
                    Header_ref_04 = lstReceiptNo.FirstOrDefault().Header_ref_04 ?? "",
                    Header_ref_05 = lstReceiptNo.FirstOrDefault().Header_ref_05 ?? "",
                    TransPaymentEntry = lstPaymentEntry.Where(x=>x.ReceiptNo == lstReceiptNo.FirstOrDefault().ReceiptNo).ToList()
                };
                var transLine = new List<TransLineGCP>();
                foreach (var item in lstReceiptNo)
                {
                    transLine.Add(new TransLineGCP()
                    {
                        Barcode = item.Barcode,
                        Brand = item.Brand,
                        Article = item.Article,
                        Name = item.Name,
                        TranNo = int.Parse(item.TranNo),
                        Uom = item.Uom,
                        POSQuantity = item.POSQuantity,
                        Price = item.Price,
                        Amount = item.Amount,
                        DiscountEntry = lstDiscountEntry.Where(x => x.ReceiptNo == item.ReceiptNo && int.Parse(item.TranNo) == x.TranNo && x.ItemNo == item.Article).ToList()
                    });
                }

                transaction.TransLine = transLine;
                batchDetail.Add(transaction);
                if (count == max_rows)
                {
                    if (FileHelper.CreateFileMaster(batchJob, "GCP_Sales", pathLocal, JsonConvert.SerializeObject(batchDetail)))
                    {
                        total_file++;
                    }
                    count = 0;
                    batchDetail.Clear();
                }
                else
                {
                    count++;
                }
            }

            if (batchDetail.Count > 0)
            {
                if (FileHelper.CreateFileMaster(batchJob, "GCP_Sales", pathLocal, JsonConvert.SerializeObject(batchDetail)))
                {
                    total_file++;
                }
                batchDetail.Clear();
            }
            return lstOrder.Count;
        }
        public int Push_TransWCM_To_GCP(string pathLocal, string pathArchived, string pathError, bool isMoveFile)
        {
            var lstFile = FileHelper.GetFileFromDir(pathLocal, "*.txt");
            int count = 0;
            try
            {
                FileHelper.Write2Logs("WCM", " Scan " + lstFile.Count + " files");
                foreach (string file in lstFile)
                {
                    if (file.ToUpper().Contains("GCP_Sales".ToUpper()))
                    {
                        var transRaw = JsonConvert.DeserializeObject<List<TransGCPModel>>(System.IO.File.ReadAllText(pathLocal + file));
                        if (transRaw.Count > 0)
                        {
                            if (CallApiGCP("pos-wcm/sale-out", transRaw, null, file))
                            {
                                if (isMoveFile) FileHelper.MoveFileToFolder(pathArchived, pathLocal + file);
                                count++;
                            }
                            else
                            {
                                //FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                            }
                        }
                    }
                    else if (file.ToUpper().Contains("GCP_TransVoid".ToUpper()))
                    {
                        var bodyData = JsonConvert.DeserializeObject<TransVoidGCP>(System.IO.File.ReadAllText(pathLocal + file));
                        if (bodyData!= null)
                        {
                            if (CallApiGCP("pos-wcm/sale-out-cancel", bodyData, null, file))
                            {
                                if (isMoveFile) FileHelper.MoveFileToFolder(pathArchived, pathLocal + file);
                                count++;
                            }
                            else
                            {
                                //FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                            }
                        }
                    }
                    
                    Thread.Sleep(500);
                }
                FileHelper.Write2Logs("WCM", " Push_TransToGCP Finished: " + count.ToString() + " files");
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> Push_TransToGCP Exception" + ex.Message.ToString());
            }
            FileHelper.WriteLogs("===> Push_TransToGCP count: " + count.ToString());
            return count;
        }
        public int Push_Trans_PLH_GCP(string pathLocal, string pathArchived, bool isMoveFile, int max_file)
        {
            var lstFile = FileHelper.GetFileFromDir(pathLocal, "*.txt");
            int count = 0;
            try
            {
                FileHelper.Write2Logs("PLH"," Scan " + lstFile.Count + " files");
                foreach (string file in lstFile)
                {
                    //OCC
                    if(file.Substring(0,9) == "GCP_SALES")
                    {
                        var transRawWinLife = JsonConvert.DeserializeObject<List<SalesGCP>>(System.IO.File.ReadAllText(pathLocal + file));
                        if (transRawWinLife.Count > 0)
                        {
                            if (CallApiGCP("pos-wcm/sale-out-plg", transRawWinLife, null, file))
                            {
                                if (isMoveFile) FileHelper.MoveFileToFolder(pathArchived, pathLocal + file);
                                count++;
                            }
                        }
                    }
                    //POS PL
                    else if(file.Substring(0, 11) == "GCP_BLUEPOS")
                    {
                        var transRawPOS = JsonConvert.DeserializeObject<List<OrderExpToGCP>>(System.IO.File.ReadAllText(pathLocal + file));
                        if (transRawPOS.Count > 0)
                        {
                            if (CallApiGCP("pos-plg/sale-out", transRawPOS, null, file))
                            {
                                if (isMoveFile) FileHelper.MoveFileToFolder(pathArchived, pathLocal + file);
                                count++;
                            }
                        }
                    }
                    if(max_file == count)
                    {
                        FileHelper.Write2Logs("PLH","===> Call-Api-PLH finished: " + count.ToString() + " file");
                        break;
                    }
                    FileHelper.Write2Logs("PLH", "===> Call-Api-PLH finished: " + count.ToString() + " file");
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> Push_TransToGCP Exception" + ex.Message.ToString());
            }
            FileHelper.WriteLogs("===> Push_TransToGCP count: " + count.ToString());
            return count;
        }
        private bool CallApiGCP(string router, object bodyJson, string param, string fileName = "")
        {
            IRestResponse response = new RestResponse();
            try
            {
                RestClient client = new RestClient("http://10.235.19.71:50001/")
                {
                    Timeout = 30000
                };

                if (!string.IsNullOrEmpty(param))
                {
                    router += param;
                }

                RestRequest restRequest = new RestRequest(router, Method.POST);
                restRequest.AddHeader("Accept", "application/json");

                if (bodyJson != null)
                {
                    restRequest.AddJsonBody(bodyJson);
                }

                response = client.Execute(restRequest);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //FileHelper.WriteLogs("===> CallApiGCP.OK: " + response.Content.ToString() + " ==> " + fileName);
                    return true;
                }
                else
                {
                    FileHelper.WriteLogs("===> CallApiGCP.ERROR: " + response.Content.ToString() + " ==> " + fileName);
                    return false;
                }

            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> CallApiGCP.Exception: " + response.Content.ToString() + " ==> " + fileName);
                FileHelper.WriteLogs("===> CallApiGCP.Exception: " + ex.Message.ToString());
                response.ResponseStatus = ResponseStatus.Error;
                response.ErrorMessage = ex.Message;
                response.ErrorException = ex;
                return false;
            }
        }
        public void Import_ReceiptNo_GCP_To_SET1(string connectionString, string pathLocal, string pathArchived, bool isMoveFile)
        {
            var lstFile = FileHelper.GetFileFromDir(pathLocal, "*.csv");
            FileHelper.WriteLogs("Scan: " + pathLocal + "===> " + lstFile.Count.ToString());
            if (lstFile.Count > 0)
            {
                foreach (var fileName in lstFile)
                {
                    var batchNo = DateTime.Now.ToString("MMddHHss");
                    if (fileName[..3] == "PLH")
                    {
                        batchNo = "PLH_" + batchNo;
                    }
                    else
                    {
                        batchNo = "WCM_" + batchNo;
                    }

                    _dapperContext = new DapperContext(connectionString);
                    FileHelper.WriteLogs(connectionString);
                    using IDbConnection conn = _dapperContext.CreateConnDB;
                    conn.Open();
                    List<RECEIPT_NO_CSV> lstReceiptNo = new List<RECEIPT_NO_CSV>();
                    var checkFile = conn.Query<CheckFileCSV>(@"SELECT FileCSV FROM ReceiptNo_GCP (NOLOCK) GROUP BY FileCSV;").ToList();
                    var temp = File.ReadAllLines(pathLocal + fileName);

                    if (checkFile.Where(x => x.FileCSV == fileName).ToList().Count > 0)
                    {
                        FileHelper.MoveFileToFolder(pathArchived, pathLocal + fileName);
                        break;
                    }
                    if(temp.Count() <= 1)
                    {
                        FileHelper.MoveFileToFolder(pathArchived, pathLocal + fileName);
                    }
                    
                    FileHelper.WriteLogs(pathLocal + fileName + " ===> " + temp.Length.ToString());
                    
                    foreach (var line in temp)
                    {
                        if (line != "RECEIPT_NO")
                        {
                            lstReceiptNo.Add(new RECEIPT_NO_CSV()
                            {
                                Batch = batchNo,
                                RECEIPT_NO = line.ToString(),
                                UpdateFlg = "N",
                                CrtDate = DateTime.Now,
                                FileCSV = fileName
                            });
                        }
                    }
                    FileHelper.WriteLogs(lstReceiptNo.Count.ToString());
                    if (lstReceiptNo.Count > 0)
                    {
                        bool flg = true;
                        string query = @"INSERT INTO ReceiptNo_GCP (Batch, RECEIPT_NO, UpdateFlg, CrtDate, FileCSV) VALUES (@Batch, @RECEIPT_NO, @UpdateFlg, @CrtDate, @FileCSV)";
                        try
                        {
                            FileHelper.WriteLogs("Insert...");
                            List<RECEIPT_NO_CSV> lstReceiptNoInsert = new List<RECEIPT_NO_CSV>();
                            int count = 0;
                            foreach (var item in lstReceiptNo)
                            {
                                lstReceiptNoInsert.Add(new RECEIPT_NO_CSV()
                                {
                                    Batch = item.Batch,
                                    RECEIPT_NO = item.RECEIPT_NO,
                                    UpdateFlg = item.UpdateFlg,
                                    CrtDate = item.CrtDate,
                                    FileCSV = item.FileCSV
                                });
                                count++;
                                if (count == 999)
                                {
                                    conn.Execute(query, lstReceiptNoInsert);
                                    FileHelper.WriteLogs("Successfully! " + lstReceiptNoInsert.Count.ToString());
                                    lstReceiptNoInsert.Clear();
                                    Console.Write("{0}", count.ToString());
                                    count = 0;
                                }
                            }
                            if (lstReceiptNoInsert.Count > 0)
                            {
                                conn.Execute(query, lstReceiptNoInsert);
                                FileHelper.WriteLogs("Successfully! " + lstReceiptNoInsert.Count.ToString());
                            }
                            Console.Write("DONE");
                        }
                        catch (Exception ex)
                        {
                            flg = false;
                            FileHelper.WriteLogs("===> GetSalesOutFromGCP.Exception" + ex.Message.ToString());
                        }
                        if (flg)
                        {
                            FileHelper.MoveFileToFolder(pathArchived, pathLocal + fileName);
                            FileHelper.WriteLogs("===> GetSalesOutFromGCP move file");
                        }
                    }
                    lstReceiptNo.Clear();
                }              
            }
            FileHelper.WriteLogs("===> Finished GetSalesOutFromGCP");
        }
        public void ProcessPriceRateDaily_GCP(string connectionString)
        {
            _dapperContext = new DapperContext(connectionString);
            FileHelper.WriteLogs(connectionString);
            using IDbConnection conn = _dapperContext.CreateConnDB;
            conn.Open();
            var user = new UserOauthToken_GCP()
            {
                username = "bluepos_user",
                grant_type = "password",
                password = "blue@12345"
            };
            var getToken = CreateOauthToken_GCP(user);
            if (getToken != null)
            {
                var lstStore = GetPriceRateDailyStoreID_GCP(getToken.access_token);
                FileHelper.WriteLogs("===> lstStore: " + JsonConvert.SerializeObject(lstStore));
                if (lstStore!=null && lstStore.Count > 0)
                {
                    foreach (var item in lstStore)
                    {
                        FileHelper.WriteLogs("====> processing: " + JsonConvert.SerializeObject(item));
                        string queryIns = @"INSERT INTO PriceRateDaily_GCP (STORE_ID, CALDAY, PRODUCT_ID, UOM, VALID_DATE, PRICE_RATE, TOTAL_SKU, UPDATE_FLG, CRT_DATE, REQUEST, ID) " +
                                          " VALUES (@STORE_ID, @CALDAY, @PRODUCT_ID, @UOM, @VALID_DATE, @PRICE_RATE, @TOTAL_SKU, @UPDATE_FLG, @CRT_DATE, @REQUEST, @ID) ";
                        
                        var dataPriceRateDaily = GetPriceRateDaily_GCP(getToken.access_token, item.STORE_ID, DateTime.Now.ToString("yyyyMMdd"));
                        
                        if (dataPriceRateDaily != null && dataPriceRateDaily.Count > 0)
                        {
                            dataPriceRateDaily.ForEach(x => x.TOTAL_SKU = item.TOTAL_SKU);
                            conn.Execute(queryIns, dataPriceRateDaily);
                            FileHelper.WriteLogs("===> Processed store: " + item.STORE_ID + " total: " + dataPriceRateDaily.Count.ToString());
                        }
                    }
                }
            }
        }
        private List<RequestPriceRateDailyStoreID_GCP> GetPriceRateDailyStoreID_GCP(string token)
        {
            List<RequestPriceRateDailyStoreID_GCP> priceRateDailyStoreID_ = new List<RequestPriceRateDailyStoreID_GCP>();
            IRestResponse response = new RestResponse();
            try
            {
                RestClient client = new RestClient("http://10.235.19.71:50002/gcp-ods/price-rate-daily")
                {
                    Timeout = 30000
                };

                RestRequest restRequest = new RestRequest("", Method.POST);
                restRequest.AddHeader("Accept", "application/json");
                restRequest.AddHeader("Authorization", "Bearer " + token);

                response = client.Execute(restRequest);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<List<RequestPriceRateDailyStoreID_GCP>>(response.Content);
                    if (result != null && result.Count > 0)
                    {
                        foreach (var item in result)
                        {
                            priceRateDailyStoreID_.Add(new RequestPriceRateDailyStoreID_GCP()
                            {
                                STORE_ID = item.STORE_ID,
                                TOTAL_SKU = item.TOTAL_SKU
                            }); 
                        }
                    }
                }
                else
                {
                    FileHelper.WriteLogs("===> GetPriceRateDaily_GCP.ERROR: " + response.Content.ToString());
                    priceRateDailyStoreID_ = null;
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> GetPriceRateDaily_GCP.Exception: " + response.Content.ToString());
                FileHelper.WriteLogs("===> GetPriceRateDaily_GCP.Exception: " + ex.Message.ToString());
                response.ResponseStatus = ResponseStatus.Error;
                response.ErrorMessage = ex.Message;
                response.ErrorException = ex;
                priceRateDailyStoreID_ = null;
            }

            return priceRateDailyStoreID_;
        }
        private List<TablePriceRateDaily_GCP> GetPriceRateDaily_GCP(string token, string storeNo, string calday)
        {
            List<TablePriceRateDaily_GCP> priceRateDaily_ = new List<TablePriceRateDaily_GCP>();
            IRestResponse response = new RestResponse();
            try
            {
                RestClient client = new RestClient("http://10.235.19.71:50002/gcp-ods/price-rate-daily")
                {
                    Timeout = 30000
                };

                RestRequest restRequest = new RestRequest("", Method.POST);
                restRequest.AddHeader("Accept", "application/json");
                restRequest.AddHeader("Authorization", "Bearer " + token);

                RequestPriceRateDaily_GCP requestPriceRateDaily_GCP = new RequestPriceRateDaily_GCP()
                {
                    CALDAY = calday,
                    STORE_ID = storeNo
                };
                
                restRequest.AddJsonBody(requestPriceRateDaily_GCP);

                response = client.Execute(restRequest);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result =  JsonConvert.DeserializeObject<List<PriceRateDaily_GCP>>(response.Content);
                    if(result != null && result.Count > 0)
                    {
                        foreach(var item in result)
                        {
                            priceRateDaily_.Add(new TablePriceRateDaily_GCP()
                            {
                                STORE_ID = item.STORE_ID,
                                CALDAY = item.CALDAY,
                                PRODUCT_ID = item.PRODUCT_ID,
                                UOM = item.UOM,
                                PRICE_RATE = item.PRICE_RATE,
                                VALID_DATE = item.VALID_DATE,
                                UPDATE_FLG = "N",
                                TOTAL_SKU = 0,
                                CRT_DATE = DateTime.Now,
                                ID = Guid.NewGuid().ToString(),
                                REQUEST = JsonConvert.SerializeObject(requestPriceRateDaily_GCP)
                            });
                        }
                    }
                }
                else
                {
                    FileHelper.WriteLogs("===> GetPriceRateDaily_GCP.ERROR: " + response.Content.ToString());
                    priceRateDaily_ = null;
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> GetPriceRateDaily_GCP.Exception: " + response.Content.ToString());
                FileHelper.WriteLogs("===> GetPriceRateDaily_GCP.Exception: " + ex.Message.ToString());
                response.ResponseStatus = ResponseStatus.Error;
                response.ErrorMessage = ex.Message;
                response.ErrorException = ex;
                priceRateDaily_ = null;
            }

            return priceRateDaily_;
        }
        private OauthToken_GCP CreateOauthToken_GCP(UserOauthToken_GCP user)
        {
            IRestResponse response = new RestResponse();
            try
            {
                RestClient client = new RestClient("http://10.235.19.71:50000/oauth/token")
                {
                    Timeout = 30000
                };

                RestRequest restRequest = new RestRequest("", Method.POST);
                restRequest.AddHeader("Accept", "application/json");

                if (user != null)
                {
                    restRequest.AddJsonBody(user);
                }

                response = client.Execute(restRequest);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    FileHelper.WriteLogs("===> response: " + response.Content.ToString());
                    return JsonConvert.DeserializeObject<OauthToken_GCP>(response.Content.ToString());
                }
                else
                {
                    FileHelper.WriteLogs("===> CreateOauthToken_GCP.ERROR: " + response.Content.ToString());
                    return null;
                }

            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> CreateOauthToken_GCP.Exception: " + response.Content.ToString());
                FileHelper.WriteLogs("===> CreateOauthToken_GCP.Exception: " + ex.Message.ToString());
                response.ResponseStatus = ResponseStatus.Error;
                response.ErrorMessage = ex.Message;
                response.ErrorException = ex;
                return null;
            }
        }
    }
    public class LstReceiptNo
    {
        public string ReceiptNo { get; set; }
    }
    public class CheckSL
    {
        public decimal SL { get; set; }
    }
    public class RECEIPT_NO_CSV
    {
        public string Batch { get; set; }
        public string RECEIPT_NO { get; set; }
        public string UpdateFlg { get; set; }
        public DateTime CrtDate { get; set; }
        public string FileCSV { get; set; }
    }
    public class CheckFileCSV
    {
        public string FileCSV { get; set; }
    }
    public class ExpCountTotalBillByDate
    {
        public string StoreNo { get; set; }
        public string OrderDate { get; set; } //yyyyMMdd
        public string OrderNo { get; set; }
    }
}