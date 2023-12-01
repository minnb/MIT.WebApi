using Dapper;
using Hifresh.Interface.Dtos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ShopInShop.Interface.Models.Central;
using ShopInShop.Interface.Models.Odoo;
using ShopInShop.Interface.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Dapper;

namespace ShopInShop.Interface.Services
{
    public class TransactionService
    {
        private DapperContext _dapperContext;
        private readonly IConfiguration _configuration;
        public TransactionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<int> GetTransFromCentral(string app, string type, string pathLocal)
        {
            int affecaffectedRows = 0;
            _dapperContext = new DapperContext("");
            using (IDbConnection conn = _dapperContext.CreateConnDB)
            {
                conn.Open();
                try
                {
                    Console.WriteLine("SELECT HEADER");
                    string orderDate = _configuration["AppSetting:EntryDate"].ToString();
                    if (string.IsNullOrEmpty(orderDate))
                    {
                        orderDate = DateTime.Now.AddDays(-2).ToString("yyyyMMdd");
                    }

                    var transHeader = conn.Query<TransHeaderCentral>(GetDataSalesQuery.GetTransHeaderQuery(app, type, orderDate, 4800)).ToList();
                    if (transHeader.Count > 0)
                    {
                        var lstOrderNo = transHeader.Select(x => x.OrderNo).ToList();
                        var lstOrderNoOrig = transHeader.Select(x => x.OrderNoOrig).ToList();

                        var transLine = conn.Query<TransLineCentral>(GetDataSalesQuery.GetTransLineQuery(), new { OrderNo = lstOrderNo }).ToList();
                        var transPaymentEntry = conn.Query<TransPaymentEntryCentral>(GetDataSalesQuery.GetTransPaymentEntryQuery(), new { OrderNo = lstOrderNo }).ToList();
                        var lstVAT = await GetInvoiceCreatedAsync(lstOrderNoOrig);

                        List<TransRawOdoo> trans = new List<TransRawOdoo>();
                        foreach (var item in transHeader)
                        {
                            var infoVAT = lstVAT.Where(x => x.OrderNo == item.OrderNo && x.StoreNo == item.StoreNo).FirstOrDefault();
                            trans.Add(new TransRawOdoo()
                            {
                                OrderNo = item.OrderNo,
                                StoreNo = item.StoreNo,
                                UpdateFlg = "N",
                                CrtDate = DateTime.Now,
                                RawData = JsonConvert.SerializeObject(new RawDataDto()
                                {
                                    IssueInvoice = infoVAT != null ? true : false,
                                    InfoInvoice = infoVAT != null ? new InfoInvoiceDto() { 
                                        CustomerName = infoVAT.CustomerName,
                                        CompanyName = infoVAT.CustomerName,
                                        Address = infoVAT.Address,
                                        Email = infoVAT.Email,
                                        PhoneNumber = infoVAT.PhoneNumber,
                                        TaxCode = infoVAT.TaxCode
                                    } : null,
                                    TransHeader = item,
                                    TransLine = transLine.Where(x => x.DocumentNo == item.OrderNo).ToList(),
                                    TransPaymentEntry = transPaymentEntry.Where(x => x.OrderNo == item.OrderNo).ToList(),
                                })
                            });
                            Console.WriteLine("Add {0}", item.OrderNo);
                        }
                        bool flag = true;
                        if (trans.Count > 0)
                        {
                            Console.WriteLine("INSERT");
                            using var transaction = conn.BeginTransaction();
                            try
                            {
                                var queryIns = @"INSERT INTO [dbo].[TransRaw] ([OrderNo],[StoreNo],[UpdateFlg],[RawData],[CrtDate])
                                                VALUES (@OrderNo, @StoreNo, @UpdateFlg, @RawData, @CrtDate);";
                                affecaffectedRows = await conn.ExecuteAsync(queryIns, trans, transaction);
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                flag = false;
                                FileHelper.WriteLogs("Rollback Exception: " + ex.Message.ToString());
                            }
                            if (flag)
                            {
                                if (FileHelper.CreateFileMaster(@"TransRaw", app, pathLocal, JsonConvert.SerializeObject(trans)))
                                {
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    FileHelper.WriteLogs("GetTransFromCentral Exception: " + ex.Message.ToString());
                }
            }
            Console.WriteLine("THE END {0}", affecaffectedRows.ToString());
            return affecaffectedRows;
        }
        public async Task<List<InvoiceCreated>> GetInvoiceCreatedAsync(List<string> lstOrder)
        {
            List<InvoiceCreated> invoiceCreateds = new List<InvoiceCreated>();
            try
            {
                _dapperContext = new DapperContext("");
                using IDbConnection conn = _dapperContext.CreateConnDB;
                conn.Open();
                var result = await conn.QueryAsync(GetEInvoiceQuery.GetInvoiceCreatedQuery(), new { OrderNo = lstOrder }).ConfigureAwait(false);
                if(result.ToList().Count > 0)
                {
                    foreach (var item in result.ToList())
                    {
                        invoiceCreateds.Add(new InvoiceCreated() { 
                            StoreNo = item.StoreNo,
                            OrderNo = item.OrderNo,
                            CustomerName = item.CustomerName,
                            CompanyName = item.CompanyName,
                            Address = item.Address,
                            Email = item.Email,
                            PhoneNumber = item.PhoneNumber,
                            TaxCode = item.TaxCode
                        });
                    }
                }
            }
            catch
            {
                invoiceCreateds =  null;
            }
            return invoiceCreateds;
        }
    }
}
