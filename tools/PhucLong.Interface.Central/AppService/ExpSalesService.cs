using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhucLong.Interface.Central.Database;
using PhucLong.Interface.Central.Models.Inbound;
using PhucLong.Interface.Central.Models.Inbound.FRANCHISE;
using PhucLong.Interface.Central.Models.Staging;
using PhucLong.Interface.Central.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using VCM.Common.Extensions;
using VCM.Common.Helpers;

namespace PhucLong.Interface.Central.AppService
{
    public class ExpSalesService
    {
        private IConfiguration _configuration;
        private DapperCentral _dapperContext;
        private Random _random;
        public ExpSalesService
           (
               IConfiguration config
           )
        {
            _configuration = config;
            _dapperContext = new DapperCentral(_configuration);
            _random = new Random();
        }

        public void Exp_Reconciliation(string bussinessDate, string storeProcedure, string pathLocal, string prefix, bool isMultiThread, int pageSize)
        {
            using (IDbConnection conn = _dapperContext.ConnCentralPhucLong)
            {
                try
                {
                    FileHelper.WriteLogs("===> " + storeProcedure + " " + bussinessDate);
                    conn.Query(@"EXEC " + storeProcedure + " '" + bussinessDate + "';");
                    var inb_RECONCILE = conn.Query<B02_Reconciliation_Dto>(InbQuery.INB_B02_RECONCILE_QUERY()).ToList();
                    if(inb_RECONCILE.Count > 0)
                    {
                         var lstStore = inb_RECONCILE.Select(x => new
                        {
                            x.StoreCode, x.BusinessDate
                        }).GroupBy(x => new { x.StoreCode, x.BusinessDate })
                            .Select(x =>
                            {
                                var temp = x.OrderBy(o => o.StoreCode).FirstOrDefault();
                                return new
                                {
                                    x.Key.StoreCode, x.Key.BusinessDate
                                };
                            }).OrderBy(x => x.StoreCode).ToList();

                        if(isMultiThread && pageSize > 0)
                        {
                            decimal totalRows = lstStore.Count;
                            int pageNumber = (int) Math.Ceiling(totalRows / pageSize);
                            pageNumber = pageNumber < 1 ? 1 : pageNumber;
                            FileHelper.WriteLogs("===> TotalRow: "+ totalRows .ToString() + " - PageSize: " + pageSize.ToString() + " - PageNumber: " + pageNumber .ToString());
                            for(var i = 0; i < pageNumber; i++)
                            {
                                string threadNumber = (i + 1).ToString();
                                var queryable = lstStore.AsQueryable();
                                var dataPage = queryable.Paging(i, pageSize);
                                var stores = dataPage.Select(s => new { s.StoreCode, s.BusinessDate }).ToList();
                                Console.WriteLine("===> Start thread {0}", threadNumber);
                                Thread t = new Thread(() =>
                                    {
                                        foreach (var s in stores)
                                        {
                                            var reconData = inb_RECONCILE.Where(x => x.StoreCode == s.StoreCode && x.BusinessDate == s.BusinessDate).ToList();
                                            CreateXml_Reconciliation(conn, reconData, pathLocal, prefix, threadNumber);
                                        }
                                    });
                                t.Start();
                            }
                        }
                        else
                        {
                            foreach (var item in lstStore)
                            {
                                var reconData = inb_RECONCILE.Where(x => x.StoreCode == item.StoreCode && x.BusinessDate == item.BusinessDate).ToList();
                                CreateXml_Reconciliation(conn, reconData, pathLocal, prefix, "0");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    FileHelper.WriteLogs("Exp_Reconciliation Exception: " + ex.Message.ToString());
                }
            }
        }
        private void CreateXml_Reconciliation(IDbConnection conn , List<B02_Reconciliation_Dto> reconData, string pathLocal, string prefix, string threadNumber)
        {
            if (reconData.Count > 0)
            {
                string fileName = GetFileName(reconData.FirstOrDefault().BusinessDate.ToString(), prefix + "_" + reconData.FirstOrDefault().StoreCode + "_" + threadNumber);
                reconData.ForEach(c => c.FileValue = fileName);
                Console.WriteLine("create xml: {0}", fileName);
                string bodyXml = B02_Reconciliation_xml(reconData);
                string result_xml = B02_ReconciliationStringXml(bodyXml);
                XElement.Parse(result_xml).Save(pathLocal + fileName);
            }
        }
        public void Exp_Sales(string storeProcedure, string pathLocal, string prefix, int top)
        {
            using IDbConnection conn = _dapperContext.ConnCentralPhucLong;
            conn.Open();
            try
            {
                FileHelper.WriteLogs("===> SP_INS_INBOUND");
                //SP_INS_INBOUND(conn, storeProcedure);
                FileHelper.WriteLogs("===> END SP_INS_INBOUND");
                var inb_Header = conn.Query<INB_SALE_MASTER>(@"EXEC SP_GET_SALES_OUTBOUND").ToList();
                FileHelper.WriteLogs("===> Selected: "  + inb_Header.Count.ToString() + " rows");
                if (inb_Header.Count > 0)
                {
                    string fileName = string.Empty;
                    List<string> TRANSACTIONSEQUENCENUMBER = new List<string>();
                    FileHelper.WriteLogs("===> COUNT: " + inb_Header.Count.ToString());

                    foreach (var item in inb_Header)
                    {
                        TRANSACTIONSEQUENCENUMBER.Add(item.TRANSACTIONSEQUENCENUMBER);

                        fileName = GetFileName(item.TRANSACTIONSEQUENCENUMBER.ToString(), prefix);
                        inb_Header.ForEach(c => c.FIELDVALUE = fileName);
                        Console.WriteLine("create xml: {0}", fileName);

                        if (SalesCreateXml(conn, inb_Header, TRANSACTIONSEQUENCENUMBER, pathLocal, inb_Header.FirstOrDefault().FIELDVALUE.ToString()))
                        {
                            UpdateStatus_INB_MASTER(conn, inb_Header);
                        }
                        TRANSACTIONSEQUENCENUMBER.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("ExpSales Exception: " + ex.Message.ToString());
            }
        }
        public void Exp_Sales_V2(string storeProcedure, string pathLocal, string prefix, bool isMultiThread, int pageSize, string jobType = null)
        {
            using IDbConnection conn = _dapperContext.ConnCentralPhucLong;
            conn.Open();
            try
            {
                if (!string.IsNullOrEmpty(storeProcedure))
                {
                    var inb_Header = conn.Query<INB_SALE_MASTER>(@"EXEC " + storeProcedure).ToList();
                    FileHelper.Write2Logs(jobType, "===> " + storeProcedure + " ===> result: " + inb_Header.Count.ToString() + " rows");
                    if (inb_Header.Count > 0)
                    {
                        string fileName = string.Empty;
                        var TRANSACTIONSEQUENCENUMBER = inb_Header.Select(x => x.TRANSACTIONSEQUENCENUMBER).ToArray();

                        if (isMultiThread)
                        {
                            var totalTransNumberAsQueryable = TRANSACTIONSEQUENCENUMBER.AsQueryable();
                            decimal totalRows = TRANSACTIONSEQUENCENUMBER.Count();
                            int pageNumber = (int)Math.Ceiling(totalRows / pageSize);
                            pageNumber = pageNumber < 1 ? 1 : pageNumber;

                            FileHelper.Write2Logs(jobType, "===> TotalRow: " + totalRows.ToString() + " - PageSize: " + pageSize.ToString() + " - PageNumber: " + pageNumber.ToString());
                            for (var i = 0; i < pageNumber; i++)
                            {
                                string threadNumber = (i + 1).ToString();
                                Console.WriteLine(jobType, "===> Start Task {0}", threadNumber);
                                var dataPage = totalTransNumberAsQueryable.Paging(i, pageSize);
                                int count = 0;
                                Task t = Task.Run(async () =>
                                {
                                    var lst_inbDetail = conn.Query<INB_SALE_DETAIL>(InbQuery.INB_DETAIL_QUERY(), new { TRANSACTIONSEQUENCENUMBER = dataPage.ToArray() }).ToList();
                                    var lst_inbTender = conn.Query<INB_SALE_TENDER>(InbQuery.INB_TENDER_QUERY(), new { TRANSACTIONSEQUENCENUMBER = dataPage.ToArray() }).ToList();
                                    var lst_inbDiscount = conn.Query<INB_SALE_DISCOUNT>(InbQuery.INB_DISCOUNT_QUERY(), new { TRANSACTIONSEQUENCENUMBER = dataPage.ToArray() }).ToList();

                                    foreach (var item in dataPage)
                                    {
                                        var inbHeader = inb_Header.Where(x => x.TRANSACTIONSEQUENCENUMBER == item).FirstOrDefault();
                                        fileName = GetFileName_V2(inbHeader.RETAILSTOREID, inbHeader.TRANSACTIONSEQUENCENUMBER, prefix);
                                        if (SalesCreateXml_V2(inbHeader, lst_inbDetail, lst_inbTender, lst_inbDiscount, pathLocal, fileName))
                                        {
                                            await UpdateStatus_INB_MASTER_V2Async(conn, inbHeader, fileName);
                                            count++;
                                        }
                                        Console.WriteLine("create xml: {0}", fileName);
                                    }
                                });
                                t.Wait();
                                FileHelper.Write2Logs(jobType, "===> Task " + threadNumber.ToString() + ": " + count.ToString() + " file");
                            }
                        }
                        else
                        {
                            //foreach (var item in TRANSACTIONSEQUENCENUMBER)
                            //{
                            //    var inbHeader = inb_Header.Where(x => x.TRANSACTIONSEQUENCENUMBER == item).FirstOrDefault();
                            //    fileName = GetFileName_V2(inbHeader.RETAILSTOREID, inbHeader.TRANSACTIONSEQUENCENUMBER, prefix);
                            //    Console.WriteLine("create xml: {0}", fileName);
                            //    if (SalesCreateXml_V2(inbHeader, lst_inbDetail, lst_inbTender, lst_inbDiscount, pathLocal, fileName))
                            //    {
                            //        UpdateStatus_INB_MASTER_V2(conn, inbHeader, fileName);
                            //    }
                            //}
                        }
                        FileHelper.Write2Logs(jobType, "===> SalesCreateXml_V2: " + TRANSACTIONSEQUENCENUMBER.Count().ToString() + " file");
                    }
                }
                else
                {
                    FileHelper.Write2Logs(jobType, "===> Not found store procedure"); ;
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(jobType, "===> Exp_Sales_V2 Exception: " + ex.Message.ToString());
            }
        }
        private void UpdateStatus_INB_MASTER(IDbConnection conn, List<INB_SALE_MASTER> inb_Header)
        {
            conn.Execute(@"UPDATE INB_SALE_MASTER SET UPDATE_FLG = 'Y', CHGE_DATE = getdate(), FIELDVALUE = '" + inb_Header.FirstOrDefault().FIELDVALUE.ToString() + "' WHERE ID = @ID", inb_Header);
        }
        private async Task UpdateStatus_INB_MASTER_V2Async(IDbConnection conn, INB_SALE_MASTER inb_Header, string fileName)
        {
           await conn.ExecuteAsync(@"UPDATE INB_SALE_MASTER SET UPDATE_FLG = 'Y', CHGE_DATE = getdate(), FIELDVALUE = '" + fileName + "' WHERE ID = @ID", inb_Header);
        }
        private async Task UpdateStatus_INB_MASTER_V3(IDbConnection conn, List<INB_SALE_MASTER> inb_Header)
        {
            await conn.ExecuteAsync(@"UPDATE INB_SALE_MASTER SET UPDATE_FLG = 'Y', CHGE_DATE = getdate(), FIELDVALUE = @FIELDVALUE WHERE TRANSACTIONSEQUENCENUMBER = @TRANSACTIONSEQUENCENUMBER", inb_Header);
        }
        private bool SalesCreateXml(IDbConnection conn,List<INB_SALE_MASTER> inb_Header, List<string> TRANSACTIONSEQUENCENUMBER, string pathLocal, string fileName)
        {
            try
            {
                var inb_detail = conn.Query<INB_SALE_DETAIL>(InbQuery.INB_DETAIL_QUERY(), new { TRANSACTIONSEQUENCENUMBER = TRANSACTIONSEQUENCENUMBER }).ToList();
                var inb_tender = conn.Query<INB_SALE_TENDER>(InbQuery.INB_TENDER_QUERY(), new { TRANSACTIONSEQUENCENUMBER = TRANSACTIONSEQUENCENUMBER }).ToList();
                var inb_discount = conn.Query<INB_SALE_DISCOUNT>(InbQuery.INB_DISCOUNT_QUERY(), new { TRANSACTIONSEQUENCENUMBER = TRANSACTIONSEQUENCENUMBER }).ToList();

                string bodyXml = string.Empty;
                foreach (var tran in TRANSACTIONSEQUENCENUMBER)
                {
                    Console.WriteLine("processing {0}", tran.ToString());

                    bodyXml += @"<_-POSDW_-E1POSTR_CREATEMULTIP SEGMENT=""1"">";

                    //Header
                    bodyXml += E1BPTRANSACTION_xml(inb_Header.Where(x => x.TRANSACTIONSEQUENCENUMBER == tran.ToString()).FirstOrDefault());

                    bodyXml += E1BPTRANSACTEXTENSIO_xml(inb_Header.Where(x => x.TRANSACTIONSEQUENCENUMBER == tran.ToString()).FirstOrDefault());

                    //Item
                    bodyXml += E1E1BPRETAILLINEITEM_xml(inb_detail.Where(x => x.TRANSACTIONSEQUENCENUMBER == tran.ToString()).ToList());

                    bodyXml += E1BPLINEITEMTAX_xml(inb_detail.Where(x => x.TRANSACTIONSEQUENCENUMBER == tran.ToString()).ToList());

                    bodyXml += E1BPLINEITEMEXTENSIO_xml(inb_detail.Where(x => x.TRANSACTIONSEQUENCENUMBER == tran.ToString()).ToList());

                    bodyXml += E1BPLINEITEMDISCOUNT_xml(inb_discount.Where(x => x.TRANSACTIONSEQUENCENUMBER == tran.ToString()).ToList());

                    //Tender
                    bodyXml += E1BPTENDER_xml(inb_tender.Where(x => x.TRANSACTIONSEQUENCENUMBER == tran.ToString()).ToList());

                    bodyXml += E1BPCREDITCARD_xml(inb_tender.Where(x => x.TRANSACTIONSEQUENCENUMBER == tran.ToString()).ToList());
                    
                    //LOY
                    bodyXml += E1BPTRANSACTIONLOYAL_xml(inb_Header.Where(x => x.TRANSACTIONSEQUENCENUMBER == tran.ToString()).FirstOrDefault());

                    bodyXml += @"</_-POSDW_-E1POSTR_CREATEMULTIP>";
                }

                string result_xml = SalesStringXml(bodyXml);

                XElement.Parse(result_xml).Save(pathLocal + fileName);
                return true;
            }
            catch(Exception ex)
            {
                FileHelper.WriteLogs("CreateXml Exception: " + ex.Message.ToString());
                return false;
            }
        }
        private bool SalesCreateXml_V2(INB_SALE_MASTER inb_Header,List<INB_SALE_DETAIL> lst_inb_detail, List<INB_SALE_TENDER> lst_inb_tender, List<INB_SALE_DISCOUNT> lst_inb_discount, string pathLocal, string fileName)
        {
            try
            {
                var inb_detail = lst_inb_detail.Where(x=>x.TRANSACTIONSEQUENCENUMBER == inb_Header.TRANSACTIONSEQUENCENUMBER).ToList();
                var inb_tender = lst_inb_tender.Where(x => x.TRANSACTIONSEQUENCENUMBER == inb_Header.TRANSACTIONSEQUENCENUMBER).ToList(); ;
                var inb_discount = lst_inb_discount.Where(x => x.TRANSACTIONSEQUENCENUMBER == inb_Header.TRANSACTIONSEQUENCENUMBER).ToList(); ;

                string bodyXml = string.Empty;
                Console.WriteLine("processing {0}", inb_Header.TRANSACTIONSEQUENCENUMBER.ToString());

                bodyXml += @"<_-POSDW_-E1POSTR_CREATEMULTIP SEGMENT=""1"">";

                //Header
                bodyXml += E1BPTRANSACTION_xml(inb_Header);

                bodyXml += E1BPTRANSACTEXTENSIO_xml(inb_Header);

                //Item
                bodyXml += E1E1BPRETAILLINEITEM_xml(inb_detail);

                bodyXml += E1BPLINEITEMTAX_xml(inb_detail);

                bodyXml += E1BPLINEITEMEXTENSIO_xml(inb_detail);

                bodyXml += E1BPLINEITEMDISCOUNT_xml(inb_discount);

                //Tender
                bodyXml += E1BPTENDER_xml(inb_tender);

                bodyXml += E1BPCREDITCARD_xml(inb_tender);

                //LOY
                bodyXml += E1BPTRANSACTIONLOYAL_xml(inb_Header);

                bodyXml += @"</_-POSDW_-E1POSTR_CREATEMULTIP>";


                string result_xml = SalesStringXml(bodyXml);

                XElement.Parse(result_xml).Save(pathLocal + fileName);
                return true;
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("SalesCreateXml_V2 Exception: " + ex.Message.ToString());
                return false;
            }
        }
        //*******************************************
        //Private function
        private string E1BPTRANSACTION_xml(INB_SALE_MASTER header)
        {
            var map = new MapperConfiguration(cfg => cfg.CreateMap<INB_SALE_MASTER, E1BPTRANSACTION>()).CreateMapper();

            E1BPTRANSACTION result = map.Map<E1BPTRANSACTION>(header);
            string xml_open = @"<_-POSDW_-E1BPTRANSACTION SEGMENT=""1"">";
            string xml_close = @"</_-POSDW_-E1BPTRANSACTION>";

            return ConvertHelper.SerializeXmlNoHeader(result).Replace("<ArrayOfE1BPTRANSACTION>", "").Replace("</ArrayOfE1BPTRANSACTION>", "").Replace("<E1BPTRANSACTION>", xml_open).Replace("</E1BPTRANSACTION>", xml_close);
        }
        private string E1BPTRANSACTEXTENSIO_xml(INB_SALE_MASTER header)
        {
            var map = new MapperConfiguration(cfg => cfg.CreateMap<INB_SALE_MASTER, E1BPTRANSACTEXTENSIO>()).CreateMapper();
            E1BPTRANSACTEXTENSIO result = map.Map<E1BPTRANSACTEXTENSIO>(header);

            string xml_open = @"<_-POSDW_-E1BPTRANSACTEXTENSIO SEGMENT=""1"">";
            string xml_close = @"</_-POSDW_-E1BPTRANSACTEXTENSIO>";
            return ConvertHelper.SerializeXmlNoHeader(result).Replace("<ArrayOfE1BPTRANSACTEXTENSIO>", "").Replace("</ArrayOfE1BPTRANSACTEXTENSIO>", "").Replace("<E1BPTRANSACTEXTENSIO>", xml_open).Replace("</E1BPTRANSACTEXTENSIO>", xml_close);
        }
        private string E1BPTRANSACTIONLOYAL_xml(INB_SALE_MASTER header)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(header.CUSTOMERCARDNUMBER))
            {
                var map = new MapperConfiguration(cfg => cfg.CreateMap<INB_SALE_MASTER, E1BPTRANSACTIONLOYAL>()).CreateMapper();
                E1BPTRANSACTIONLOYAL LOYAL = map.Map<E1BPTRANSACTIONLOYAL>(header);

                string xml_open = @"<_-POSDW_-E1BPTRANSACTIONLOYAL SEGMENT=""1"">";
                string xml_close = @"</_-POSDW_-E1BPTRANSACTIONLOYAL>";
                result =  ConvertHelper.SerializeXmlNoHeader(LOYAL).Replace("<ArrayOfE1BPTRANSACTIONLOYAL>", "").Replace("</ArrayOfE1BPTRANSACTIONLOYAL>", "").Replace("<E1BPTRANSACTIONLOYAL>", xml_open).Replace("</E1BPTRANSACTIONLOYAL>", xml_close);
            }
            return result;
        }
        private string E1E1BPRETAILLINEITEM_xml(List<INB_SALE_DETAIL> lstDetail)
        {
            var map = new MapperConfiguration(cfg => cfg.CreateMap<INB_SALE_DETAIL, E1BPRETAILLINEITEM>()).CreateMapper();
            string result = string.Empty;
            string xml_open = @"<_-POSDW_-E1BPRETAILLINEITEM SEGMENT=""1"">";
            string xml_close = @"</_-POSDW_-E1BPRETAILLINEITEM>";

            foreach (var detail in lstDetail)
            {
                E1BPRETAILLINEITEM obj = map.Map<E1BPRETAILLINEITEM>(detail);
                result += ConvertHelper.SerializeXmlNoHeader(obj).Replace("<ArrayOfE1BPRETAILLINEITEM>", "").Replace("</ArrayOfE1BPRETAILLINEITEM>", "").Replace("<E1BPRETAILLINEITEM>", xml_open).Replace("</E1BPRETAILLINEITEM>", xml_close);
            }

            return result;
        }
        private string E1BPLINEITEMEXTENSIO_xml(List<INB_SALE_DETAIL> lstDetail)
        {
            var map = new MapperConfiguration(cfg => cfg.CreateMap<INB_SALE_DETAIL, E1BPLINEITEMEXTENSIO>()).CreateMapper();
            string result = string.Empty;
            string xml_open = @"<_-POSDW_-E1BPLINEITEMEXTENSIO SEGMENT=""1"">";
            string xml_close = @"</_-POSDW_-E1BPLINEITEMEXTENSIO>";

            foreach (var detail in lstDetail)
            {
                if (!string.IsNullOrEmpty(detail.FIELDGROUP))
                {
                    E1BPLINEITEMEXTENSIO obj = map.Map<E1BPLINEITEMEXTENSIO>(detail);
                    result += ConvertHelper.SerializeXmlNoHeader(obj).Replace("<ArrayOfE1BPLINEITEMEXTENSIO>", "").Replace("</ArrayOfE1BPLINEITEMEXTENSIO>", "").Replace("<E1BPLINEITEMEXTENSIO>", xml_open).Replace("</E1BPLINEITEMEXTENSIO>", xml_close);
                }
            }

            return result;
        }
        private string E1BPLINEITEMTAX_xml(List<INB_SALE_DETAIL> lstDetail)
        {
            var map = new MapperConfiguration(cfg => cfg.CreateMap<INB_SALE_DETAIL, E1BPLINEITEMTAX>()).CreateMapper();

            string result = string.Empty;
            string xml_open = @"<_-POSDW_-E1BPLINEITEMTAX SEGMENT=""1"">";
            string xml_close = @"</_-POSDW_-E1BPLINEITEMTAX>";

            foreach (var detail in lstDetail)
            {
                E1BPLINEITEMTAX obj = map.Map<E1BPLINEITEMTAX>(detail);
                result += ConvertHelper.SerializeXmlNoHeader(obj).Replace("<ArrayOfE1BPLINEITEMTAX>", "").Replace("</ArrayOfE1BPLINEITEMTAX>", "").Replace("<E1BPLINEITEMTAX>", xml_open).Replace("</E1BPLINEITEMTAX>", xml_close);
            }

            return result;
        }
        private string E1BPTENDER_xml(List<INB_SALE_TENDER> lstTender)
        {
            var map = new MapperConfiguration(cfg => cfg.CreateMap<INB_SALE_TENDER, E1BPTENDER>()).CreateMapper();
            string result = string.Empty;
            string xml_open = @"<_-POSDW_-E1BPTENDER SEGMENT=""1"">";
            string xml_close = @"</_-POSDW_-E1BPTENDER>";

            foreach (var tender in lstTender)
            {
                E1BPTENDER obj = map.Map<E1BPTENDER>(tender);
                result += ConvertHelper.SerializeXmlNoHeader(obj).Replace("<ArrayOfE1BPTENDER>", "").Replace("</ArrayOfE1BPTENDER>", "").Replace("<E1BPTENDER>", xml_open).Replace("</E1BPTENDER>", xml_close); 
            }

            return result;
        }
        private string E1BPCREDITCARD_xml(List<INB_SALE_TENDER> lstTender)
        {
            var map = new MapperConfiguration(cfg => cfg.CreateMap<INB_SALE_TENDER, E1BPCREDITCARD>()).CreateMapper();
            string result = string.Empty;
            string xml_open = @"<_-POSDW_-E1BPCREDITCARD SEGMENT=""1"">";
            string xml_close = @"</_-POSDW_-E1BPCREDITCARD>";

            foreach (var tender in lstTender)
            {
                if (!string.IsNullOrEmpty(tender.CARDNUMBER))
                {
                    E1BPCREDITCARD obj = map.Map<E1BPCREDITCARD>(tender);
                    result += ConvertHelper.SerializeXmlNoHeader(obj).Replace("<ArrayOfE1BPCREDITCARD>", "").Replace("</ArrayOfE1BPCREDITCARD>", "").Replace("<E1BPCREDITCARD>", xml_open).Replace("</E1BPCREDITCARD>", xml_close);
                }
            }
            return result;
        }
        private string E1BPLINEITEMDISCOUNT_xml(List<INB_SALE_DISCOUNT> lstDiscount)
        {
            var map = new MapperConfiguration(cfg => cfg.CreateMap<INB_SALE_DISCOUNT, E1BPLINEITEMDISCOUNT>()).CreateMapper();
            string result = string.Empty;
            string xml_open = @"<_-POSDW_-E1BPLINEITEMDISCOUNT SEGMENT=""1"">";
            string xml_close = @"</_-POSDW_-E1BPLINEITEMDISCOUNT>";

            foreach (var discount in lstDiscount)
            {
                E1BPLINEITEMDISCOUNT obj = map.Map<E1BPLINEITEMDISCOUNT>(discount);
                result += ConvertHelper.SerializeXmlNoHeader(obj).Replace("<ArrayOfE1BPLINEITEMDISCOUNT>", "").Replace("</ArrayOfE1BPLINEITEMDISCOUNT>", "").Replace("<E1BPLINEITEMDISCOUNT>", xml_open).Replace("</E1BPLINEITEMDISCOUNT>", xml_close);
            }

            return result;
        }
        private string B02_Reconciliation_xml(List<B02_Reconciliation_Dto> lstRecon)
        {
            var map = new MapperConfiguration(cfg => cfg.CreateMap<B02_Reconciliation_Dto, B02_Reconciliation>()).CreateMapper();
            string result = string.Empty;
            string xml_open = @"<Data>";
            string xml_close = @"</Data>";
            foreach (var recon in lstRecon)
            {
                B02_Reconciliation obj = map.Map<B02_Reconciliation>(recon);
                result += ConvertHelper.SerializeXmlNoHeader(obj).Replace("<ArrayOfE1BPLINEITEMDISCOUNT>", "").Replace("</ArrayOfB02_Reconciliation>", "").Replace("<B02_Reconciliation>", xml_open).Replace("</B02_Reconciliation>", xml_close);
            }

            return result;
        }
        private string SalesStringXml(string bodyData)
        {
            string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>";
            xml += @"<_-POSDW_-POSTR_CREATEMULTIPLE06 xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">";
            xml += @"<IDOC BEGIN=""1"">";
            //xml += @"<_-POSDW_-E1POSTR_CREATEMULTIP SEGMENT=""1"">";
            xml += bodyData;
            //xml += @"</_-POSDW_-E1POSTR_CREATEMULTIP>";
            xml += @"</IDOC>";
            xml += @"</_-POSDW_-POSTR_CREATEMULTIPLE06>";
            return xml;
        }
        private string B02_ReconciliationStringXml(string bodyData)
        {
            string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>";
            xml += @"<ns0:mt_odoo_reconciliation_in xmlns:ns0=""urn:phuclong:odoo:reconcile:to:car"">";
            xml += bodyData;
            xml += @"</ns0:mt_odoo_reconciliation_in>";
            return xml;
        }
        private string GetFileName(string orderNo, string prefix)
        {
            int checkSum = _random.Next(1, 999);
            return prefix + "_" + orderNo + "_" + DateTime.Now.ToString("MMdd_HHmmssfff")  + "_" +  checkSum.ToString().PadRight(3, '0') + ".xml";
        }
        private string GetFileName_V2(string storeNo, string orderNo, string prefix)
        {
            int checkSum = _random.Next(1, 999);
            return prefix + "_" + storeNo  + "_" + orderNo + "_" + DateTime.Now.ToString("yMMdd_HHmmssff") + "_" + checkSum.ToString().PadRight(3, '0') + ".xml";
        }

        //Franchise
        private string INB_FRANCHISE_xml(List<FRANCHISE_Dto> lstData)
        {
            var map = new MapperConfiguration(cfg => cfg.CreateMap<FRANCHISE_Dto, SalesFRE>()).CreateMapper();
            string result = string.Empty;
            string xml_open = @"<LINE>";
            string xml_close = @"</LINE>";
            foreach (var recon in lstData)
            {
                SalesFRE obj = map.Map<SalesFRE>(recon);
                result += ConvertHelper.SerializeXmlNoHeader(obj).Replace("<SalesFRE>", xml_open).Replace("</SalesFRE>", xml_close);
            }

            return result;
        }
        private string FRANCHISE_Xml(string bodyData)
        {
            string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>";
            xml += @"<MAPPING xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">";
            xml += bodyData;
            xml += @"</MAPPING>";
            return xml;
        }
        public void Exp_FRANCHISE(string bussinessDate, string storeProcedure, string pathLocal, string prefix)
        {
            using (IDbConnection conn = _dapperContext.ConnCentralPhucLong)
            {
                try
                {
                    FileHelper.WriteLogs(@"===> " + storeProcedure);
                    conn.Query(@"EXEC " + storeProcedure);
                    var dataINB = conn.Query<FRANCHISE_Dto>(InbQuery.INB_FRANCHISE_QUERY()).ToList();
                    if (dataINB.Count > 0)
                    {
                        var lstStore = dataINB.Select(x => new
                        {
                            x.PL_STORE
                        }).GroupBy(x => new { x.PL_STORE })
                            .Select(x =>
                            {
                                var temp = x.OrderBy(o => o.PL_STORE).FirstOrDefault();
                                return new
                                {
                                    x.Key.PL_STORE
                                };
                            }).OrderBy(x => x.PL_STORE).ToList();

                        foreach (var item in lstStore)
                        {
                            var reconData = dataINB.Where(x => x.PL_STORE == item.PL_STORE).ToList();
                            if (reconData.Count > 0)
                            {
                                string fileName = GetFileName(reconData.FirstOrDefault().POSTING_DATE.ToString(), prefix + "_" + item.PL_STORE);
                                reconData.ForEach(c => c.FileValue = fileName);
                                Console.WriteLine("create xml: {0}", fileName);
                                string bodyXml = INB_FRANCHISE_xml(reconData);
                                string result_xml = FRANCHISE_Xml(bodyXml);
                                XElement.Parse(result_xml).Save(pathLocal + fileName);
                                //update status
                                conn.Execute(@"UPDATE INB_FRANCHISE SET UpdateFlg = 'Y', [ChgDate] = getdate(), FileValue = '" + reconData.FirstOrDefault().FileValue.ToString() + "' WHERE TRANS_NO = @TRANS_NO AND PL_STORE = @PL_STORE AND POSTING_DATE = @POSTING_DATE", reconData);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    FileHelper.WriteLogs("Exp_FRANCHISE Exception: " + ex.Message.ToString());
                }
            }
        }
    }
}
