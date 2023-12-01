using Hifresh.Interface.Dtos;
using Hifresh.Interface.Models.Hifresh;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ShopInShop.Interface.Models.Odoo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using VCM.Common.Helpers;
using VCM.Shared.Const;

namespace ShopInShop.Interface.Services
{
    public class HifreshTransService
    {
        private readonly IConfiguration _configuration;
        public HifreshTransService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task TransfferDataToHF(string app, string type, string pathLocal, string pathArchived)
        {
            try
            {
                TransactionService transactionService = new TransactionService(_configuration);
                int numberRow = await transactionService.GetTransFromCentral(app, type, pathLocal);
                FileHelper.WriteLogs("Selected: " + numberRow.ToString());

                var lstFile = FileHelper.GetFileFromDir(pathLocal, "*.txt");
                FileHelper.WriteLogs("Scan: " + pathLocal);
                FileHelper.WriteLogs("Count: " + lstFile.Count.ToString() + " file");
                if (lstFile.Count > 0)
                {
                    List<SalesExpModel> salesExps = new List<SalesExpModel>();
                    foreach (string file in lstFile)
                    {
                        Console.WriteLine(file);
                        if (file.ToString().Substring(0, 7).ToUpper() == "HIFRESH")
                        {
                            var transRaw = JsonConvert.DeserializeObject<List<TransRawOdoo>>(System.IO.File.ReadAllText(pathLocal + file));
                            if(transRaw.Count > 0)
                            {
                                foreach(var item in transRaw)
                                {
                                    salesExps.AddRange(GetSalesExp(item));
                                }
                                FileHelper.WriteLogs("Processed: " + file);
                            }
                            FileHelper.MoveFileToDestination(pathLocal + file, pathArchived);
                        }
                    }

                    if(salesExps.Count > 0)
                    {
                        string xml = salesExps.SerializeXml();
                        //FileHelper.WriteLogs(JsonConvert.SerializeObject(salesExps));
                        //FileHelper.WriteLogs(xml);
                        if (!string.IsNullOrEmpty(xml))
                        {
                            XmlDocument xmlDocument = new XmlDocument();
                            xmlDocument.LoadXml(xml);
                            //Save the document to a file.
                            xmlDocument.Save(pathLocal + @"Winmart_Hifresh_Sales_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + @".xml");

                        }
                    }
                }
                //upload
                if (!string.IsNullOrEmpty(SftpConst.SftpHost))
                {
                    var lstFileUpload = FileHelper.GetFileFromDir(pathLocal, SftpConst.Sftp_file_extension);
                    if(lstFileUpload.Count > 0)
                    {
                        SftpHelper sftpHelper = new SftpHelper(SftpConst.SftpHost, SftpConst.SftpPort, SftpConst.SftpUserName, SftpConst.SftpPassword);
                        if(SftpConst.SftpHostOS.ToUpper() == "WINDOWS")
                        {
                            sftpHelper.UploadSftpWindow(SftpConst.Sftp_local_process, SftpConst.Sftp_sftp_process, SftpConst.Sftp_local_archive, SftpConst.Sftp_file_extension);
                        }
                        else
                        {
                            sftpHelper.UploadSftpLinux2(SftpConst.Sftp_local_process, SftpConst.Sftp_sftp_process, SftpConst.Sftp_local_archive, SftpConst.Sftp_file_extension);
                        }
                    }
                }

            }
            catch(Exception ex)
            {
                FileHelper.WriteLogs("TransfferDataToHF Exception" + ex.Message.ToString());
            }
        }

        private List<SalesExpModel> GetSalesExp(TransRawOdoo transRaw)
        {
            List<SalesExpModel> lstData = new List<SalesExpModel>();
            try
            {
                var rawData = JsonConvert.DeserializeObject<RawDataDto>(transRaw.RawData);
                if(rawData.TransLine.Count > 0)
                {
                    foreach(var item in rawData.TransLine)
                    {
                        var returned = 1;

                        if (!string.IsNullOrEmpty(item.OrigTransPos))
                        {
                            returned = -1;
                        }

                        lstData.Add(new SalesExpModel()
                        {
                            VENDOR = item.IsSendSAP == true ? "WCM" : "HIF",
                            STOREID = rawData.TransHeader.StoreNo,
                            KEY = item.LineNo.ToString(),
                            SALESORDERNUMBER = rawData.TransHeader.OrderNoOrig,
                            ISSUEINVOICE = rawData.IssueInvoice,
                            CUSTOMERNAME = rawData.IssueInvoice == true ? rawData.InfoInvoice.CompanyName:"",
                            CUSTOMERADDRESS = rawData.IssueInvoice == true ? rawData.InfoInvoice.Address : "",
                            CUSTOMEREMAIL = rawData.IssueInvoice == true ? rawData.InfoInvoice.Email : "",
                            CUSTOMERTAXNUMBER = rawData.IssueInvoice == true ? rawData.InfoInvoice.TaxCode : "",
                            ITEMCODE = item.ItemNo,
                            ITEMNAME = item.Description,
                            UNIT = item.UnitOfMeasure,
                            QUANTITY = item.Quantity * returned,
                            CURRENCY = "VND",
                            PRICEINCLUDESTAX = true,
                            TAXOUT = item.VATPercent,
                            UNITPRICE = item.UnitPrice,
                            DISCOUNTAMOUNT = item.DiscountAmount * returned,
                            AMOUNT = item.LineAmountIncVATOrig * returned,
                            SALESDATE = rawData.TransHeader.OrderDate.ToString("yyyy-MM-dd"),
                            COSTAMOUNT = item.LineAmountIncVAT,
                            TAXIN = item.VATPercent,

                        });
                    }
                }
            }
            catch
            {

            }
            return lstData;
        }
    }
}
