using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhucLong.Interface.Central.Database;
using PhucLong.Interface.Central.Models.GCP;
using PhucLong.Interface.Central.Models.OCC;
using PhucLong.Interface.Central.Queries;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using VCM.Common.Helpers;
using VCM.Shared.API.PhucLongV2;
using VCM.Shared.Dtos.PhucLong;

namespace PhucLong.Interface.Central.AppService
{
    public class SelesGCPService
    {
        private IConfiguration _configuration;
        private DapperCentral _dapperContext;
        public SelesGCPService
           (
               IConfiguration config
           )
        {
            _configuration = config;
            _dapperContext = new DapperCentral(_configuration);
        }
        public void GetSurveyResult(string date, string prifix)
        {
            if (!string.IsNullOrEmpty(prifix))
            {
                var lstSrv = prifix.Split(";");
                if(lstSrv.Length > 0)
                {
                    foreach(var srv in lstSrv)
                    {
                        string connectString = @"Server = "+ srv + @"; Database = CentralSales; User ID = SIS; Password = QAZwsx!@#";
                        var _dapperContext = new DapperStaging(connectString);
                        using IDbConnection db = _dapperContext.ConnDapperStaging;
                        try
                        {
                            string queryResult = @"SELECT [Id],[AnswerCode],[QuestionCode],[PhoneNumber],[OrderNo],[Type],[CreatedDate],[RefID] "
                                                   + "FROM [dbo].[SurveyResult] NOLOCK WHERE CAST([CreatedDate] AS DATE) = '"+ date  + @"'";

                            var dataResult = db.Query<SurveyResult>(queryResult).ToList();
                            if(dataResult.Count > 0)
                            {
                                string router = @"pos-wcm/pos-surveys";
                                List<SurveyResult> bodyJson = new List<SurveyResult>();
                                int count = 0;
                                foreach (var item in dataResult)
                                {
                                    bodyJson.Add(item);
                                    count++;
                                    if(count == 999)
                                    {
                                        if(CallApiGCP(router, bodyJson, null))
                                        {
                                            FileHelper.WriteLogs("===> OK: " );
                                            bodyJson.Clear();
                                        }
                                    }
                                    FileHelper.WriteLogs(JsonConvert.SerializeObject(item));
                                }
                                if (CallApiGCP(router, bodyJson, null))
                                {
                                    FileHelper.WriteLogs("===> OK: ");
                                    bodyJson.Clear();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            FileHelper.WriteLogs("===> GetSurveyResult Exception:" + ex.Message.ToString());
                        }
                    }

                }
                else
                {
                    FileHelper.WriteLogs("===> ERROR: " + prifix);
                }
            }
        }
        private bool CallApiGCP(string router, object bodyJson, string param)
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
                    FileHelper.WriteLogs("===> CallApiGCP.OK: " + response.Content.ToString());
                    return true;
                }
                else
                {
                    FileHelper.WriteLogs("===> CallApiGCP.ERROR: " + response.Content.ToString());
                    return false;
                }

            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> CallApiGCP.Exception: " + response.Content.ToString());
                response.ResponseStatus = ResponseStatus.Error;
                response.ErrorMessage = ex.Message;
                response.ErrorException = ex;
                return false;
            }
        }
        public void GetDataSurveyResult(string storeProcedure)
        {
            using IDbConnection conn = _dapperContext.ConnCentralPhucLong;
            conn.Open();
            try
            {
                var dataResult = conn.Query<SurveyResult>("EXEC " +storeProcedure).ToList();
                FileHelper.WriteLogs("===> EXEC " + storeProcedure + " ===> count: " + dataResult.Count.ToString());
                if (dataResult.Count > 0)
                {
                    string router = @"pos-wcm/pos-surveys";
                    List<SurveyResult> bodyJson = new List<SurveyResult>();
                    int count = 0;
                    foreach (var item in dataResult)
                    {
                        bodyJson.Add(item);
                        count++;
                        if (count == 999)
                        {
                            if (CallApiGCP(router, bodyJson, null))
                            {
                                FileHelper.WriteLogs("===> OK: ");
                                bodyJson.Clear();
                                count = 0;
                            }
                        }
                    }
                    if (CallApiGCP(router, bodyJson, null))
                    {
                        FileHelper.WriteLogs("===> OK: ");
                        bodyJson.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> GetDataSurveyResult Exception:" + ex.Message.ToString());
            }
        }
        public void GetSales_Exp_GCP(string query, string pathLocal, int totalRows,string prifix)
        {
            int fileTotal = 0;
            using IDbConnection conn = _dapperContext.ConnCentralPhucLong;
            conn.Open();
            try
            {
                var dataTransHeader = conn.Query<OrderExpToGCP>(query).ToList();
                int resultRows = dataTransHeader.Count;
                FileHelper.WriteLogs("===> " + query + " ===> result: " + resultRows.ToString());
                if (resultRows > 0)
                {
                    var lstOrderId = new List<string>();
                    foreach (var o in dataTransHeader)
                    {
                        lstOrderId.Add(o.OrderNo);
                    }

                    string transLinesQuery = SalesGCPQuery.TransLineQuery();
                    string transPaymentsQuery = SalesGCPQuery.TransPaymentEntryQuery();
                    string transPointLinesQuery = SalesGCPQuery.TransPointLineQuery();
                    string transDiscountEntriesQuery = SalesGCPQuery.TransDiscountEntryQuery2();

                    if (query.ToUpper().Contains("ARCHIVED"))
                    {
                         transLinesQuery = SalesGCPQuery.TransLineQueryArchive();
                         transPaymentsQuery = SalesGCPQuery.TransPaymentEntryQueryArchive();
                         transPointLinesQuery = SalesGCPQuery.TransPointLineQueryArchive();
                         transDiscountEntriesQuery = SalesGCPQuery.TransDiscountEntryQuery2Archive();
                    }

                    var transLines = conn.Query<TransLine_PLH_BLUEPOS>(transLinesQuery, new { DocumentNo = lstOrderId }).ToList();
                    var transPayments = conn.Query<TransPaymentEntry_PLH_BLUEPOS>(transPaymentsQuery, new { OrderNo = lstOrderId }).ToList();
                    var transPointLines = conn.Query<TransPointLine_PLH_BLUEPOS>(transPointLinesQuery, new { OrderNo = lstOrderId }).ToList();
                    var transDiscountEntries = conn.Query<TransDiscountEntry_PLH_BLUEPOS>(transDiscountEntriesQuery, new { OrderNo = lstOrderId }).ToList();
                    using (var transaction = conn.BeginTransaction())
                    {
                        List<OrderExpToGCP> gCPTransactions = new List<OrderExpToGCP>();
                        int maxRow = 0;
                        List<TempSalesGCP> transTemplate = new List<TempSalesGCP>();
                        string batchFile = string.Empty;
                        foreach (var item in dataTransHeader)
                        {
                            var transLine = transLines.Where(x => x.OrderNo == item.OrderNo).ToList();
                            var transPayment = transPayments.Where(x => x.OrderNo == item.OrderNo).ToList();
                            var transDiscountEntry = transDiscountEntries.Where(x => x.OrderNo == item.OrderNo).ToList();
                            var transPointLine = transPointLines.Where(x => x.OrderNo == item.OrderNo).ToList();
                            gCPTransactions.Add(new OrderExpToGCP()
                            {
                                OrderNo = item.OrderNo,
                                OrderDate = item.OrderDate,
                                StoreNo = item.StoreNo,
                                CustName = item.CustName,
                                OrderTime = item.OrderTime,
                                PosNo = item.PosNo,
                                Note = item.Note,
                                ReturnedOrderNo = item.ReturnedOrderNo,
                                TransactionType = item.TransactionType,
                                IsRetry = item.IsRetry,
                                SalesType = item.SalesType,
                                Items = transLine.ToList(),
                                Loyalty = transPointLine.ToList(),
                                DiscountEntry = transDiscountEntry.ToList(),
                                Payments = transPayment.ToList()
                            });

                            transTemplate.Add(new TempSalesGCP()
                            {
                                SalesType = "PLH",
                                OrderDate = item.OrderDate.Date,
                                OrderNo = item.OrderNo,
                                CrtDate = DateTime.Now
                            });

                            maxRow++;
                            if (maxRow == totalRows && gCPTransactions.Count > 0)
                            {
                                batchFile = DateTime.Now.ToString("yyyyMMddHHmmssf");
                                if (FileHelper.CreateFileMaster(prifix + batchFile.ToString(), "GCP", pathLocal, JsonConvert.SerializeObject(gCPTransactions)))
                                {
                                    fileTotal++;
                                    maxRow = 0;
                                    InsLogs(conn, transaction, transTemplate, batchFile);

                                    gCPTransactions.Clear();
                                    transTemplate.Clear();
                                }
                            }
                        }
                        
                        if (gCPTransactions.Count > 0)
                        {
                            batchFile = DateTime.Now.ToString("yyyyMMddHHmmssf");
                            if (FileHelper.CreateFileMaster(prifix + batchFile.ToString(), "GCP", pathLocal, JsonConvert.SerializeObject(gCPTransactions)))
                            {
                                fileTotal++;
                                InsLogs(conn, transaction, transTemplate, batchFile);
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> GetSales_Exp_GCP Exception:" + ex.Message.ToString());
            }
        }
        public void GetSalesGCP_OCC(string query, string pathLocal, int totalRows)
        {
            int fileTotal = 0;
            using IDbConnection conn = _dapperContext.ConnCentralPhucLong; 
            conn.Open();
            try
            {
                var dataTransHeader = conn.Query<OCCTransHeader>(query).ToList();
                int resultRows = dataTransHeader.Count;
                FileHelper.WriteLogs("===> " + query + " ===> result: " + resultRows.ToString());
                if (resultRows > 0)
                {
                    var lstOrderId = new List<string>();
                    foreach (var o in dataTransHeader)
                    {
                        lstOrderId.Add(o.OrderNo);
                    }

                    var transLines = conn.Query<OCCTransLine>(SalesGCPQuery.SalesDetailQuery(), new { OrderNo = lstOrderId }).ToList();
                    var transPayments = conn.Query<OCCTransPaymentEntry>(SalesGCPQuery.SalesPaymentQuery(), new { OrderNo = lstOrderId }).ToList();

                    using (var transaction = conn.BeginTransaction())
                    {
                        List<SalesGCP> gCPTransactions = new List<SalesGCP>();
                        int maxRow = 0;
                        List<TempSalesGCP> transTemplate = new List<TempSalesGCP>();
                        string batchFile = string.Empty;
                        foreach (var item in dataTransHeader)
                        {
                            var transline = transLines.Where(x => x.OrderNo == item.OrderNo && x.LineType == 0).ToList();
                            var transpayment = transPayments.Where(x => x.OrderNo == item.OrderNo).ToList();
                            var paymentAmount = transline.Where(x => x.LineAmountIncVAT > 0).Sum(x => x.OriginLineAmountIncVAT);
                            gCPTransactions.Add(new SalesGCP()
                            {
                                StoreNo = item.StoreNo,
                                SalesStoreNo = item.StoreNo2,
                                SalesPosNo = item.PosNo,
                                OrderNo = item.OrderNo,
                                OrderDate = item.OrderDate.ToString("yyyy-MM-dd"),
                                SaleType = item.OrderType,
                                TransactionType = item.TransactionType,
                                MemberCardNo = item.MemberCardNo,
                                RefNo = item.TransactionType == "PLR1" ? item.RefKey : "",
                                TransLine = MappingTransLine(transline),
                                TransPaymentEntry = MappingPayment(transpayment, paymentAmount)
                            });

                            transTemplate.Add(new TempSalesGCP()
                            {
                                SalesType = "OCC",
                                OrderDate = item.OrderDate.Date,
                                OrderNo = item.OrderNo,
                                CrtDate = DateTime.Now
                            });

                            maxRow++;
                            if (maxRow == totalRows && gCPTransactions.Count > 0)
                            {
                                batchFile = DateTime.Now.ToString("yyyyMMddHHmmssf");
                                if (FileHelper.CreateFileMaster("SALES_" + batchFile.ToString(), "GCP", pathLocal, JsonConvert.SerializeObject(gCPTransactions)))
                                {
                                    fileTotal++;
                                    maxRow = 0;
                                    gCPTransactions.Clear();
                                    InsLogs(conn, transaction, transTemplate, batchFile);
                                }
                            }
                        }
                        if (gCPTransactions.Count > 0)
                        {
                            batchFile = DateTime.Now.ToString("yyyyMMddHHmmssf");
                            if (FileHelper.CreateFileMaster("SALES_" + batchFile.ToString(), "GCP", pathLocal, JsonConvert.SerializeObject(gCPTransactions)))
                            {
                                fileTotal++;
                                InsLogs(conn, transaction, transTemplate, batchFile);
                            }
                        }
                        transaction.Commit();
                    }
                    FileHelper.WriteLogs("===> Finished total: " + resultRows.ToString() + " transaction ==> created: " + fileTotal.ToString() + " file");
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> GetSalesGCP_OCC Exception:" + ex.Message.ToString());
            }
        }
        private List<SalesGCPDetail> MappingTransLine(List<OCCTransLine> transLines)
        {
            List<SalesGCPDetail> result = new List<SalesGCPDetail>();
            foreach (var item in transLines)
            {
                result.Add(new SalesGCPDetail()
                {
                    LineNo = item.LineNo,
                    ParentLineNo = item.ParentLineNo,
                    ItemNo = item.ItemNo,
                    ItemName = item.ItemName,
                    Uom = item.Uom,
                    Quantity = item.Quantity,
                    UnitPrice = item.OriginPrice,
                    DiscountAmount = item.DiscountAmount,
                    VATPercent = item.VATPercent,
                    LineAmount = item.OriginLineAmountIncVAT,
                    MemberPointsEarn = item.MemberPointsEarn,
                    MemberPointsRedeem = item.MemberPointsRedeem,
                    CupType = item.CupType,
                    Size = item.Size,
                    IsCombo = item.IsCombo,
                    IsTopping = item.IsTopping,
                    ScanTime = item.ScanTime,
                });
            }
            return result;
        }
        private List<SalesGCPPayment> MappingPayment(List<OCCTransPaymentEntry> paymentEntries, decimal paymentAmount)
        {
            List<SalesGCPPayment> result = new List<SalesGCPPayment>();
            foreach (var item in paymentEntries)
            {
                result.Add(new SalesGCPPayment()
                {
                    LineNo = item.LineNo,
                    TenderType = item.TenderType,
                    CurrencyCode = item.CurrencyCode,
                    ExchangeRate = item.ExchangeRate,
                    PaymentAmount = paymentAmount,
                    ReferenceNo = item.ReferenceNo
                });
            }
            return result;
        }
        private void InsLogs(IDbConnection conn, IDbTransaction transaction, List<TempSalesGCP> transTemplate, string batchFile)
        {
            transTemplate.ForEach(x => x.Batch = batchFile);
            var queryIns = @"INSERT INTO dbo.Temp_SalesGCP (SalesType, OrderNo, OrderDate, CrtDate, Batch)  VALUES(@SalesType, @OrderNo, @OrderDate, @CrtDate, @Batch);";
            FileHelper.WriteLogs("==> Inserted: " + conn.Execute(queryIns, transTemplate, transaction).ToString());
        }
    }
}
