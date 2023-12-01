using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhucLong.Interface.Central.Database;
using PhucLong.Interface.Central.Models.Master;
using PhucLong.Interface.Central.Models.OCC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using VCM.Common.Helpers;
using VCM.Shared.Entity.Central;
using VCM.Shared.Enums;

namespace PhucLong.Interface.Central.AppService
{
    public class TransOCCService
    {
        private IConfiguration _config;
        private DapperStaging _dapperContext;
        private string _connectString;
        private CentralDbContext _dbContext;
        private List<MasterItem> _lstItem;
        private LoggingService _loggingDb;
        private List<MappingChannel> _lstChannel;
        private List<MappingStore> _mappingStore;
        private List<MappingTender> _mappingTender;
        public TransOCCService
        (
              IConfiguration config,
              string connectString,
               CentralDbContext dbContext
        )
        {
            _config = config;
            _dbContext = dbContext;
            _connectString = connectString;
            _dapperContext = new DapperStaging(_connectString);
            _loggingDb = new LoggingService(_config);
        }

        private string InitOrderNoOCC(string order, string salesType)
        {
            if (!string.IsNullOrEmpty(order) && salesType != "KIOS")
            {
                return order + "99";
            }
            else if (!string.IsNullOrEmpty(order) && salesType == "KIOS")
            {
                return order;
            }
            else
            {
                return string.Empty;
            }
        }
        public void SaveOCCTransaction_V1(string pathLocal, string pathArchive, string pathError, int maxFile, bool isMoveFile)
        {
            int affectedRows = 0;
            var appCode = "OCC";
            var lstFile = FileHelper.GetFileFromDir(pathLocal, "*.txt");
            FileHelper.WriteLogs("Scan: " + pathLocal + "===> " + lstFile.Count.ToString());
            try
            {
                if (lstFile.Count > 0)
                {
                    _lstItem = _dbContext.MasterItem.Where(x => x.Blocked == false).ToList();
                    _lstChannel = _dbContext.MappingChannel.Where(x => x.Blocked == false && x.AppCode == PartnerEnum.PLH.ToString()).ToList();
                    _mappingStore = _dbContext.MappingStore.Where(x => x.Blocked == false && x.AppCode == PartnerEnum.PLH.ToString()).ToList();
                    _mappingTender = _dbContext.MappingTender.Where(x => x.Blocked == false && x.AppCode == PartnerEnum.PLH.ToString()).ToList();
                    foreach (string file in lstFile)
                    {
                        if (file.ToString().Substring(0, 3).ToUpper() == appCode)
                        {
                            var transRaw = JsonConvert.DeserializeObject<List<StagingTransLine>>(System.IO.File.ReadAllText(pathLocal + file));
                            //FileHelper.WriteLogs(JsonConvert.SerializeObject(transRaw));
                            if (transRaw.Count > 0)
                            {
                                bool check = false;
                                using var transaction = _dbContext.Database.BeginTransaction();
                                try
                                {
                                    var lstOrder = transRaw.Select(x => new { x.OrderNo }).GroupBy(x => new { x.OrderNo }).Select(x => { var temp = x.OrderByDescending(o => o.OrderNo).FirstOrDefault(); return new { x.Key.OrderNo }; }).ToList();
                                    if (lstOrder.Count > 0)
                                    {
                                        foreach (var item in lstOrder)
                                        {
                                            var orderData = transRaw.Where(x => x.OrderNo == item.OrderNo).ToList();
                                            var firstOrderData = orderData.FirstOrDefault();
                                            string orderNo = InitOrderNoOCC(firstOrderData.OrderNo.ToString(), firstOrderData.Size);
                                            if (orderData.Count > 0)
                                            {
                                                var saleType = _lstChannel.Where(x => x.OrderChannel == firstOrderData.OrderType.ToString()).FirstOrDefault();
                                                var saleModel = _mappingStore.Where(x => x.StoreNo2 == firstOrderData.StoreNo2).FirstOrDefault();
                                                
                                                FileHelper.WriteLogs("===> procssing: " + orderNo + " channel: " + saleModel.Type.ToString() + " discount: " + saleModel.Discount.ToString());

                                                if (saleType == null || saleModel == null || _mappingTender == null)
                                                {
                                                    _loggingDb.LoggingToDB("INB-OCC", orderNo, firstOrderData.OrderType.ToString() + " - SalesType or StoreNo or TenderType chưa khai báo");
                                                    if (isMoveFile) FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                                                    break;
                                                }

                                                var transLines = SaveOCCTransLine(orderData, saleModel, saleType, orderNo);
                                                if (transLines != null)
                                                {
                                                    var transHeader = SaveOCCTransHeader(orderData, appCode, saleModel.Type, orderNo);
                                                    SaveOCCTransPaymentEntry(orderData, transLines, saleModel, saleType, _mappingTender, orderNo);
                                                    check = true;
                                                }
                                                else
                                                {
                                                    check = false;
                                                    if (isMoveFile) FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if (check)
                                    {
                                        transaction.Commit();
                                        Console.WriteLine("Done: " + file);
                                        if (isMoveFile) FileHelper.MoveFileToFolder(pathArchive, pathLocal + file);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    ExceptionHelper.WriteExptionError("TransOCCService.SaveOCCTransaction_V1: ", ex);
                                }
                            }
                            else
                            {
                                FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                                continue;
                            }
                            affectedRows++;
                            if (affectedRows == maxFile)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("===> SaveOCCTransaction_V1.Exception " + ex.Message.ToString());
            }
        }
        private OCCTransHeader SaveOCCTransHeader(List<StagingTransLine> stagingTransLines, string appCode, string ver, string OrderNo)
        {
            var fistStagingTransLine = stagingTransLines.Where(x => x.LineType == 0).OrderBy(x => x.CrtDate).FirstOrDefault();
            var transHeader = new OCCTransHeader()
            {
                AppCode = appCode,
                Version = ver,
                OrderNo = OrderNo,
                OrderNo2 = fistStagingTransLine.OrderNo,
                OrderDate = fistStagingTransLine.OrderDate.Date,
                StoreNo = fistStagingTransLine.StoreNo2,
                StoreNo2 = fistStagingTransLine.StoreNo,
                PosNo = fistStagingTransLine.PosNo,
                OrderType = fistStagingTransLine.OrderType.ToString(),
                TransactionType = fistStagingTransLine.TransactionType == "1" ? "PLT1" : "PLR1",
                BeginDate = fistStagingTransLine.CrtDate.ToString("yyyyMMddHHmmss"),
                EndDate = fistStagingTransLine.CrtDate.ToString("yyyyMMddHHmmss"),
                TransactionCurrency = "VND",
                MemberCardNo = fistStagingTransLine.MemberCardNo??"",
                MemberPointsEarn = fistStagingTransLine.MemberPointsEarn,
                MemberPointsRedeem = fistStagingTransLine.MemberPointsRedeem,
                RefKey = InitOrderNoOCC(fistStagingTransLine.RefNo??"", fistStagingTransLine.Size),
                UpdateFlg = "N",
                IssuedVATInvoice = false,
                IsEOD = false,
                CrtDate = DateTime.Now
            };
            //FileHelper.WriteLogs(JsonConvert.SerializeObject(transHeader));
            _dbContext.OCCTransHeader.Add(transHeader);
            _dbContext.SaveChanges();
            return transHeader;
        }
        private List<OCCTransLine> SaveOCCTransLine(List<StagingTransLine> lstTransLines, MappingStore saleModel, MappingChannel channel, string orderNo)
        {
            try
            {
                List<OCCTransLine> transLines = new List<OCCTransLine>();
                var stagingTransLines = lstTransLines.Where(x => x.LineType < 9999).ToList();
                if (stagingTransLines.Count > 0)
                {
                    decimal discountPercentPartner = 0;
                    //decimal LineAmountInclVAT = item
                    if (saleModel.Discount > 0 && saleModel.Discount <= 100 && channel.OrderChannel == "10" && saleModel.Type == "CV_LIFE")
                    {
                        discountPercentPartner = saleModel.Discount;
                    }

                    decimal revenueRatio = 100 - discountPercentPartner;
                    int i = 0;
                    foreach (var item in stagingTransLines)
                    {
                        i++;
                        MasterItem itemMaster = new MasterItem();
                        if (item.LineType == 0)
                        {
                            itemMaster = _lstItem.Where(x => x.ItemNo == item.ItemNo2).FirstOrDefault();
                        }
                        else
                        {
                            var transLine = stagingTransLines.Where(x => x.LineNo == item.LineParent).FirstOrDefault();
                            itemMaster = _lstItem.Where(x => x.ItemNo == transLine.ItemNo2).FirstOrDefault();
                        }

                        if (itemMaster == null)
                        {
                            FileHelper.WriteLogs("====> " + item.ItemNo + " - " + item.ItemName + " @chưa mapping mã sản phẩm theo SAP-PhucLong");
                            _loggingDb.LoggingToDB("INB-OCC", orderNo, item.ItemNo + " - " + item.ItemName + " -  chưa mapping mã sản phẩm theo SAP-PhucLong");
                            return null;
                        }

                        decimal vatPercent = itemMaster.VatPercent;
                        string vatGroup = itemMaster.TaxGroupCode;
                        decimal unitPrice = item.UnitPrice;
                        decimal commissionsAmount = 0;

                        if (saleModel.Type == "CV_LIFE") //&& channel.OrderChannel == "10" - Deli CV_LIFE
                        {
                            unitPrice = Math.Round((item.LineAmountIncVAT * (100 - saleModel.Discount) / 100) / item.Quantity, 0);
                            item.DiscountAmount = 0;
                        }

                        decimal LineAmountIncVAT = unitPrice * item.Quantity - item.DiscountAmount;
                        decimal netPrice = Math.Round((LineAmountIncVAT / (1 + vatPercent / 100)) / item.Quantity, 0);
                        decimal VATAmount = LineAmountIncVAT - netPrice * item.Quantity;

                        if (channel.IsDiscount && channel.OrderChannel != "10")
                        {
                            if (saleModel.Type == "WIN_LIFE")
                            {
                                commissionsAmount = Math.Round((LineAmountIncVAT * (channel.DiscountPercent)) / 100, 0);
                            }
                        }

                        transLines.Add(new OCCTransLine()
                        {
                            OrderNo = orderNo,
                            LineType = item.LineType,
                            LineNo = item.LineNo + i,
                            LineNo2 = item.LineNo,
                            ParentLineNo = item.LineParent,
                            ItemNo = item.ItemNo2,
                            ItemNo2 = item.ItemNo,
                            ItemName = item.ItemName,
                            Uom = itemMaster.Uom,
                            Quantity = item.Quantity,
                            OriginPrice = item.UnitPrice,
                            UnitPrice = unitPrice,
                            NetPrice = netPrice,
                            DiscountAmount = item.DiscountAmount,
                            VATPercent = vatPercent,
                            VATGroup = vatGroup,
                            VATAmount = VATAmount,
                            LineAmountExcVAT = LineAmountIncVAT - VATAmount,
                            LineAmountIncVAT = LineAmountIncVAT,
                            OriginLineAmountIncVAT = item.LineAmountIncVAT,
                            RevenueShareRatio = revenueRatio,
                            CommissionsAmount = commissionsAmount,
                            MemberPointsEarn = item.MemberPointsEarn,
                            MemberPointsRedeem = item.MemberPointsRedeem,
                            CupType = item.CupType,
                            Size = itemMaster.SIZE_DIM ?? "",
                            IsCombo = item.IsCombo,
                            IsTopping = item.IsTopping,
                            ScanTime = item.CrtDate,
                            UpdateFlg = "N",
                            CrtDate = DateTime.Now
                        });
                    }
                    //FileHelper.WriteLogs(JsonConvert.SerializeObject(transLines));
                    transLines.ForEach(n => _dbContext.OCCTransLine.Add(n));
                    _dbContext.SaveChanges();
                    return transLines;
                }
                else
                {
                    return null;
                }
            }
            catch(Exception ex) 
            {
                FileHelper.WriteLogs("SaveOCCTransLine Exception: " + ex.Message.ToString());
                return null;
            }
        }
        private List<OCCTransPaymentEntry> SaveOCCTransPaymentEntry(List<StagingTransLine> lstTransLines, List<OCCTransLine> transLines,MappingStore saleModel, MappingChannel channel, List<MappingTender> tender,string orderNo)
        {
            List<OCCTransPaymentEntry> transPaymentEntries = new List<OCCTransPaymentEntry>();
            var listPayment = lstTransLines.Where(x => x.LineType == 9999).ToList();
            decimal paymentAmount = transLines.Where(x => x.LineAmountIncVAT > 0).Sum(x => x.LineAmountIncVAT);
            if (saleModel.Type.ToUpper() == SalesModelEnum.CV_LIFE.ToString())
            {
                if (channel.OrderChannel == "10" && listPayment.Count > 0)
                {
                    transPaymentEntries.Add(new OCCTransPaymentEntry()
                    {
                        OrderNo = orderNo,
                        PaymentDate = listPayment.FirstOrDefault().OrderDate.Date,
                        LineNo = 1000,
                        StoreNo = listPayment.FirstOrDefault().StoreNo,
                        PosNo = listPayment.FirstOrDefault().PosNo,
                        TenderType = saleModel.TenderType.ToString(),
                        ExchangeRate = 1,
                        CurrencyCode = "VND",
                        AmountInCurrency = paymentAmount,
                        AmountTendered = paymentAmount,
                        ReferenceNo = listPayment.FirstOrDefault().RefNo ?? ""
                    });
                }
                else
                {
                    decimal comimission = transLines.Where(x => x.CommissionsAmount > 0).Sum(x => x.CommissionsAmount);
                    if (comimission > 0)
                    {
                        paymentAmount -= (comimission > 0 ? comimission : 0);
                        var fistPayment = listPayment.FirstOrDefault();
                        transPaymentEntries.Add(new OCCTransPaymentEntry()
                        {
                            OrderNo = orderNo,
                            PaymentDate = fistPayment.OrderDate.Date,
                            LineNo = fistPayment.LineNo,
                            StoreNo = fistPayment.StoreNo,
                            PosNo = fistPayment.PosNo,
                            TenderType = fistPayment.ItemNo,
                            ExchangeRate = 1,
                            CurrencyCode = "VND",
                            AmountInCurrency = paymentAmount,
                            AmountTendered = paymentAmount,
                            ReferenceNo = fistPayment.RefNo ?? ""
                        });

                        transPaymentEntries.Add(new OCCTransPaymentEntry()
                        {
                            OrderNo = orderNo,
                            PaymentDate = fistPayment.OrderDate.Date,
                            LineNo = 1999,
                            StoreNo = fistPayment.StoreNo,
                            PosNo = fistPayment.PosNo,
                            TenderType = channel.TenderType.ToString(),
                            ExchangeRate = 1,
                            CurrencyCode = "VND",
                            AmountInCurrency = comimission,
                            AmountTendered = comimission,
                            ReferenceNo = orderNo ?? ""
                        });
                    }
                    else
                    {
                        if (saleModel.Type == "CV_LIFE")
                        {
                            var tenderDeli = tender.Where(x => x.Type == "CV_LIFE").FirstOrDefault().TenderType.ToString();
                            transPaymentEntries.Add(new OCCTransPaymentEntry()
                            {
                                OrderNo = orderNo,
                                PaymentDate = lstTransLines.FirstOrDefault().OrderDate.Date,
                                LineNo = 9999,
                                StoreNo = lstTransLines.FirstOrDefault().StoreNo,
                                PosNo = lstTransLines.FirstOrDefault().PosNo,
                                TenderType = tenderDeli,
                                ExchangeRate = 1,
                                CurrencyCode = "VND",
                                AmountInCurrency = transLines.Sum(x => x.LineAmountIncVAT),
                                AmountTendered = transLines.Sum(x => x.LineAmountIncVAT),
                                ReferenceNo = lstTransLines.FirstOrDefault().OrderNo
                            });
                        }
                        else
                        {
                            foreach (var item in listPayment)
                            {
                                transPaymentEntries.Add(new OCCTransPaymentEntry()
                                {
                                    OrderNo = orderNo,
                                    PaymentDate = item.OrderDate.Date,
                                    LineNo = item.LineNo,
                                    StoreNo = item.StoreNo,
                                    PosNo = item.PosNo,
                                    TenderType = item.ItemNo,
                                    ExchangeRate = 1,
                                    CurrencyCode = "VND",
                                    AmountInCurrency = item.UnitPrice,
                                    AmountTendered = item.UnitPrice,
                                    ReferenceNo = item.RefNo
                                });
                            }
                        }
                    }
                }
            }
            else if(saleModel.Type.ToUpper() == SalesModelEnum.WIN_LIFE.ToString())
            {

                if (channel.OrderChannel == "10" && listPayment.Count > 0)
                {
                    string[] tenderNotSum = _mappingTender.Select(x => x.WCM).ToArray();

                    var paymentEx = listPayment.Where(x => !_mappingTender.Select(x => x.WCM).ToArray().Contains(x.ItemNo)).ToList();
                    var paymentIncl = listPayment.Where(x => _mappingTender.Select(x => x.WCM).ToArray().Contains(x.ItemNo)).ToList();
                    if(paymentAmount - paymentIncl.Sum(x => x.UnitPrice) > 0)
                    {
                        transPaymentEntries.Add(new OCCTransPaymentEntry()
                        {
                            OrderNo = orderNo,
                            PaymentDate = listPayment.FirstOrDefault().OrderDate.Date,
                            LineNo = 1000,
                            StoreNo = listPayment.FirstOrDefault().StoreNo,
                            PosNo = listPayment.FirstOrDefault().PosNo,
                            TenderType = saleModel.TenderType.ToString(),
                            ExchangeRate = 1,
                            CurrencyCode = "VND",
                            AmountInCurrency = paymentAmount - paymentIncl.Sum(x => x.UnitPrice),
                            AmountTendered = paymentAmount - paymentIncl.Sum(x => x.UnitPrice),
                            ReferenceNo = listPayment.FirstOrDefault().RefNo ?? ""
                        });
                    }

                   if(paymentIncl.Count > 0)
                    {
                        int i = 1;
                        foreach(var p in paymentIncl)
                        {
                            transPaymentEntries.Add(new OCCTransPaymentEntry()
                            {
                                OrderNo = orderNo,
                                PaymentDate = p.OrderDate.Date,
                                LineNo = 1000 + i,
                                StoreNo = p.StoreNo,
                                PosNo = p.PosNo,
                                TenderType = _mappingTender.Where(x=>x.WCM == p.ItemNo).FirstOrDefault().TenderType,
                                ExchangeRate = 1,
                                CurrencyCode = "VND",
                                AmountInCurrency = p.UnitPrice,
                                AmountTendered = p.UnitPrice,
                                ReferenceNo = p.RefNo ?? ""
                            });
                            i++;
                        }
                    }
                }
                else
                {
                    decimal comimission = transLines.Where(x => x.CommissionsAmount > 0).Sum(x => x.CommissionsAmount);
                    if (comimission > 0 && saleModel.Type == "WIN_LIFE")
                    {
                        paymentAmount -= (comimission > 0 ? comimission : 0);
                        var fistPayment = listPayment.FirstOrDefault();
                        transPaymentEntries.Add(new OCCTransPaymentEntry()
                        {
                            OrderNo = orderNo,
                            PaymentDate = fistPayment.OrderDate.Date,
                            LineNo = fistPayment.LineNo,
                            StoreNo = fistPayment.StoreNo,
                            PosNo = fistPayment.PosNo,
                            TenderType = fistPayment.ItemNo,
                            ExchangeRate = 1,
                            CurrencyCode = "VND",
                            AmountInCurrency = paymentAmount,
                            AmountTendered = paymentAmount,
                            ReferenceNo = fistPayment.RefNo ?? ""
                        });

                        transPaymentEntries.Add(new OCCTransPaymentEntry()
                        {
                            OrderNo = orderNo,
                            PaymentDate = fistPayment.OrderDate.Date,
                            LineNo = 1999,
                            StoreNo = fistPayment.StoreNo,
                            PosNo = fistPayment.PosNo,
                            TenderType = channel.TenderType.ToString(),
                            ExchangeRate = 1,
                            CurrencyCode = "VND",
                            AmountInCurrency = comimission,
                            AmountTendered = comimission,
                            ReferenceNo = orderNo ?? ""
                        });
                    }
                    else
                    {

                            foreach (var item in listPayment)
                            {
                                transPaymentEntries.Add(new OCCTransPaymentEntry()
                                {
                                    OrderNo = orderNo,
                                    PaymentDate = item.OrderDate.Date,
                                    LineNo = item.LineNo,
                                    StoreNo = item.StoreNo,
                                    PosNo = item.PosNo,
                                    TenderType = item.ItemNo,
                                    ExchangeRate = 1,
                                    CurrencyCode = "VND",
                                    AmountInCurrency = item.UnitPrice,
                                    AmountTendered = item.UnitPrice,
                                    ReferenceNo = item.RefNo
                                });
                            }
                    
                    }
                }
            }
 
            //FileHelper.WriteLogs(JsonConvert.SerializeObject(paymentEntry));
            transPaymentEntries.ForEach(n => _dbContext.OCCTransPaymentEntry.Add(n));
            _dbContext.SaveChanges();
            return transPaymentEntries;
        }
        public void GetTransFromBLUEPOS(string query, string pathLocal)
        {
            using IDbConnection conn = _dapperContext.ConnDapperStaging;
            conn.Open();
            try
            {

                var data = conn.Query<StagingTransLine>(query).ToList();
                int resultRows = data.Count;
                FileHelper.WriteLogs("EXEC " + query + " ===> result: " + resultRows.ToString());
                if (resultRows > 0)
                {
                    var lstStore = data
                          .Select(x => new
                          {
                              x.OrderNo,
                          })
                          .GroupBy(x => new { x.OrderNo })
                          .Select(x =>
                          {
                              var temp = x.OrderByDescending(o => o.OrderNo).FirstOrDefault();
                              return new
                              {
                                  x.Key.OrderNo,
                              };
                          }).ToList();
                    int countFile = 0;
                    if (lstStore.Count > 0)
                    {
                        foreach (var item in lstStore)
                        {
                            var jsonData = data.Where(x => x.OrderNo == item.OrderNo).ToList();
                            if (FileHelper.CreateFileMaster("StagingTransLine_" + item.OrderNo.ToString(), "OCC", pathLocal, JsonConvert.SerializeObject(jsonData)))
                            {
                                countFile++;
                            }
                        }
                    }
                    FileHelper.WriteLogs("Created: " + countFile.ToString() + " file");
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("GetTransFromBLUEPOS Exception:" + ex.Message.ToString());
            }
        }

    }
}
