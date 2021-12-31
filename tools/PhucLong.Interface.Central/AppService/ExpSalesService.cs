using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhucLong.Interface.Central.Database;
using PhucLong.Interface.Central.Models.Inbound;
using PhucLong.Interface.Central.Models.Staging;
using PhucLong.Interface.Central.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using VCM.Common.Helpers;
using VCM.Shared.Enums;

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

        public void Exp_Reconciliation(string pathLocal, string prefix)
        {
            using (IDbConnection conn = _dapperContext.ConnCentralPhucLong)
            {
                try
                {
                    var inb_RECONCILE = conn.Query<B02_Reconciliation_Dto>(InbQuery.INB_B02_RECONCILE_QUERY()).ToList();
                    FileHelper.WriteLogs("Selected: " + inb_RECONCILE.Count.ToString());

                    if(inb_RECONCILE.Count > 0)
                    {
                        string fileName = GetFileName(prefix);
                        inb_RECONCILE.ForEach(c => c.FileValue = fileName);

                        Console.WriteLine("create xml: {0}", fileName);

                        string bodyXml = B02_Reconciliation_xml(inb_RECONCILE);

                        string result_xml = B02_ReconciliationStringXml(bodyXml);
                        XElement.Parse(result_xml).Save(pathLocal + fileName);

                        UpdateStatus_INB_B02_RECONCILE(conn, inb_RECONCILE);

                    }
                    else
                    {
                        FileHelper.WriteLogs("No data");
                    }
                }
                catch (Exception ex)
                {
                    FileHelper.WriteLogs("GetInboundData Exception: " + ex.Message.ToString());
                }
            }
        }
        private void SP_INS_INBOUND(IDbConnection conn, string storeProcedure)
        {
            int execute = 201;
            if (!string.IsNullOrEmpty(storeProcedure))
            {
                execute = conn.Execute(storeProcedure, commandTimeout: 36000);
            }
            FileHelper.WriteLogs("EXEC: " + storeProcedure + " ===> result: " + execute.ToString());
        }
        public void Exp_Sales(string storeProcedure, string pathLocal, string prefix)
        {
            using IDbConnection conn = _dapperContext.ConnCentralPhucLong;
            conn.Open();
            try
            {
                SP_INS_INBOUND(conn, storeProcedure);

                var inb_Header = conn.Query<INB_SALE_MASTER>(InbQuery.INB_HEADER_QUERY()).ToList();
                FileHelper.WriteLogs("Selected: " + inb_Header.Count.ToString());
                if (inb_Header.Count > 0)
                {
                    string fileName = string.Empty;
                    List<string> TRANSACTIONSEQUENCENUMBER = new List<string>();
                    foreach (var item in inb_Header)
                    {
                        TRANSACTIONSEQUENCENUMBER.Add(item.TRANSACTIONSEQUENCENUMBER);

                        if (TRANSACTIONSEQUENCENUMBER.Count == 1000)
                        {
                            //Create xml
                            fileName = GetFileName(prefix);
                            inb_Header.ForEach(c => c.FIELDVALUE = fileName);
                            Console.WriteLine("create xml: {0}", fileName);

                            if (SalesCreateXml(conn, inb_Header, TRANSACTIONSEQUENCENUMBER, pathLocal, inb_Header.FirstOrDefault().FIELDVALUE.ToString()))
                            {
                                UpdateStatus_INB_MASTER(conn, inb_Header);
                            }

                            TRANSACTIONSEQUENCENUMBER.Clear();
                            Thread.Sleep((int)ThreadEnum.Two);
                        }

                        //1-1
                        fileName = GetFileName(prefix);
                        inb_Header.ForEach(c => c.FIELDVALUE = fileName);
                        Console.WriteLine("create xml: {0}", fileName);

                        if (SalesCreateXml(conn, inb_Header, TRANSACTIONSEQUENCENUMBER, pathLocal, inb_Header.FirstOrDefault().FIELDVALUE.ToString()))
                        {
                            UpdateStatus_INB_MASTER(conn, inb_Header);
                        }

                        TRANSACTIONSEQUENCENUMBER.Clear();

                        Thread.Sleep((int)ThreadEnum.Two);
                    }

                    //Create xml
                    //Thread.Sleep((int)ThreadEnum.Two);
                    //fileName = GetFileName(prefix);
                    //inb_Header.ForEach(c => c.FIELDVALUE = fileName);
                    //Console.WriteLine("create xml: {0}", fileName);
                    //if (CreateXml(conn, inb_Header, TRANSACTIONSEQUENCENUMBER, pathLocal, inb_Header.FirstOrDefault().FIELDVALUE.ToString()))
                    //{
                    //    //conn.Execute(@"UPDATE INB_SALE_MASTER SET CHGE_DATE = getdate(), FIELDVALUE = '" + inb_Header.FirstOrDefault().FIELDVALUE.ToString() + "' WHERE TRANSACTIONSEQUENCENUMBER = @TRANSACTIONSEQUENCENUMBER", inb_Header);
                    //    UpdateStatus_INB_MASTER(conn, inb_Header);
                    //}
                }

            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("ExpSales Exception: " + ex.Message.ToString());
            }
        }
        private void UpdateStatus_INB_MASTER(IDbConnection conn, List<INB_SALE_MASTER> inb_Header)
        {
            conn.Execute(@"UPDATE INB_SALE_MASTER SET UPDATE_FLG = 'Y', CHGE_DATE = getdate(), FIELDVALUE = '" + inb_Header.FirstOrDefault().FIELDVALUE.ToString() + "' WHERE TRANSACTIONSEQUENCENUMBER = @TRANSACTIONSEQUENCENUMBER", inb_Header);
        }
        private void UpdateStatus_INB_B02_RECONCILE(IDbConnection conn, List<B02_Reconciliation_Dto> inb_Recon)
        {
            conn.Execute(@"UPDATE INB_B02_RECONCILE SET UpdateFlg = 'Y', [ChgDate] = getdate(), FileValue = '" + inb_Recon.FirstOrDefault().FileValue.ToString() + "' WHERE StoreCode = @StoreCode AND BusinessDate = @BusinessDate", inb_Recon);
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

        private string GetFileName(string prefix)
        {
            int checkSum = _random.Next(1, 999);
            return prefix + "_" + DateTime.Now.ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("HHmmssfff")  + "_" +  checkSum.ToString().PadRight(3, '0') + ".xml";
        }
    }
}
