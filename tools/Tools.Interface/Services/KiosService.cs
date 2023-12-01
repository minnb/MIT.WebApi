using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhucLong.Interface.Central.Models.Master;
using PhucLong.Interface.Central.Models.OCC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.Interface.Const;
using Tools.Interface.Database;
using VCM.Common.Helpers;
using VCM.Shared.API.PhucLongV2;
using VCM.Shared.Const;
using VCM.Shared.Dtos.PhucLong;
using VCM.Shared.Entity.Central;
using VCM.Shared.Entity.Partner;
using VCM.Shared.Entity.PriceEngine;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Dapper;
using WCM.EntityFrameworkCore.EntityFrameworkCore.PriceEngine;

namespace Tools.Interface.Services
{
    public class KiosService
    {
        private DapperContext _dapperContext;
        private readonly string[] APPCODE_KIOS = new[] { "PLH", "WCM", "PLF" };
        public KiosService()
        {
        }
        public int ExpTransFromKios(string app, string connectString, string pathLocal)
        {
            int count = 0;
            _dapperContext = new DapperContext(connectString);
            using (IDbConnection conn = _dapperContext.CreateConnDB)
            {
                conn.Open();
                try
                {
                    string query = @"SELECT [PartnerCode],[AppCode],[StoreNo],[OrderNo],[JsonData],[UpdateFlg],[HostName],[CrtUser],[CrtDate],[RequestId] "
                                    + " FROM [dbo].[TmpTransRaw] (NOLOCK) WHERE UpdateFlg = 'N';";
                    var lstTransRaw = conn.Query<TmpTransRaw>(query).ToList();
                    if (lstTransRaw.Count > 0)
                    {
                        foreach (var item in lstTransRaw)
                        {
                            Console.WriteLine("processing {0}", item.OrderNo);
                            if (FileHelper.CreateFileMaster(item.OrderNo, item.AppCode + "_" + app, pathLocal, JsonConvert.SerializeObject(JsonConvert.DeserializeObject<OrderRequestBody>(item.JsonData))))
                            {
                                count++;
                                var queryIns = @"UPDATE TmpTransRaw SET UpdateFlg = 'Y' WHERE OrderNo = '" + item.OrderNo + "' AND [StoreNo] = '" + item.StoreNo + "';";
                                conn.Query(queryIns);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    FileHelper.WriteLogs("===> GetTransFromKios Exception: " + ex.Message.ToString());
                }
            }

            return count;
        }
        private string InitOrderNo(string OrderNo)
        {
            if (!string.IsNullOrEmpty(OrderNo))
            {
                return OrderNo;
            }
            else
            {
                return "";
            }
        }
        public void ImportTransWinmartToDB(string appCode, string connectString, string pathLocal, string pathArchive, string pathError, bool isMoveFile, int maxFile)
        {
            var lstFile = FileHelper.GetFileFromDir(pathLocal, "*.txt");
            FileHelper.WriteLogs("Scan: " + pathLocal + "===> " + lstFile.Count.ToString());
            if (lstFile.Count > 0)
            {
                int affectedRows = 0;
                var dbContext = new KiosDbContext(connectString);
                foreach (string file in lstFile)
                {
                    if (file.ToString()[..3].ToUpper() == appCode)
                    {
                        using var transaction = dbContext.Database.BeginTransaction();
                        {
                            try
                            {
                                var transRaw = JsonConvert.DeserializeObject<OrderRequestBody>(System.IO.File.ReadAllText(pathLocal + file));
                                //FileHelper.WriteLogs(JsonConvert.SerializeObject(transRaw));
                                if (transRaw != null)
                                {
                                    Console.WriteLine("OrderNo: {0}", transRaw.OrderNo);
                                    string orderNo = InitOrderNo(transRaw.OrderNo);
                                    var saveTransHeader = SaveTransHeader(dbContext, transRaw, appCode, orderNo, "");
                                    var saveTransLine = SaveTransLine(dbContext, transRaw, orderNo);
                                    var saveTransPaymentEntry = SaveTransPayment(dbContext, transRaw, orderNo);
                                    if(saveTransHeader != null && saveTransLine != null && saveTransPaymentEntry != null)
                                    {
                                        transaction.Commit();
                                        Console.WriteLine("Done: " + orderNo);
                                        if (isMoveFile) FileHelper.MoveFileToFolder(pathArchive, pathLocal + file);
                                    }
                                }
                                else
                                {
                                    if (isMoveFile) FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                FileHelper.WriteLogs("===> ImportTransToDB Exception: " + ex.Message.ToString());
                            }
                        }
                        affectedRows++;
                    }
                    if (affectedRows == maxFile)
                    {
                        break;
                    }
                }
            }
        }

        //NEW VERSION
        public void ProcessingSalesKIOS(string connectStringPriceEngine, string pathLocal, string pathArchive, string pathError, bool isMoveFile, int maxFile)
        {
            int count = 0;
            _dapperContext = new DapperContext(connectStringPriceEngine);
            List<SysStoreSet> sysStoreSets = new List<SysStoreSet>();
            List<SysConfig> sysConfigList = new List<SysConfig>();
            using (IDbConnection conn = _dapperContext.CreateConnDB)
            {
                conn.Open();
                try
                {
                    string query = @"SELECT [PartnerCode],[AppCode],[StoreNo],[OrderNo],[JsonData],[UpdateFlg],[HostName],[CrtUser],[CrtDate],[RequestId] "
                                    + " FROM [dbo].[TmpTransRaw] (NOLOCK) WHERE UpdateFlg = 'N';";
                    var lstTransRaw = conn.Query<TmpTransRaw>(query).ToList();
                    sysStoreSets = conn.Query<SysStoreSet>(@"SELECT [StoreNo],[SubSet],[ServerIP],[ServerRead],[Blocked],[PosType],[StoreNoWPH],[StoreNoPLH] FROM [dbo].[Sys_StoreSet] WHERE [Blocked] = 0;").ToList();
                    sysConfigList = conn.Query<SysConfig>(@"SELECT [AppCode],[Name],[Description],[Prefix],[Blocked],[CrtDate] FROM [dbo].[Sys_Config] WHERE Blocked = 0;").ToList();

                    if (lstTransRaw.Count > 0)
                    {
                        foreach (var item in lstTransRaw)
                        {
                            Console.WriteLine("processing {0}", item.OrderNo);
                            if (FileHelper.CreateFileMaster(item.OrderNo, item.AppCode + "_" + item.StoreNo, pathLocal, JsonConvert.SerializeObject(JsonConvert.DeserializeObject<OrderRequestBody>(item.JsonData))))
                            {
                                count++;
                                var queryIns = @"UPDATE TmpTransRaw SET UpdateFlg = 'Y' WHERE OrderNo = '" + item.OrderNo + "' AND [StoreNo] = '" + item.StoreNo + "';";
                                conn.Query(queryIns);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    FileHelper.WriteLogs("===> ProcessingSalesKIOS Exception: " + ex.Message.ToString());
                }
            }

            var connect_CentralMD_PL = sysConfigList.FirstOrDefault(x => x.AppCode == "PLF" && x.Name == "CentralMD").Prefix ?? "";
            List<ItemDto> dataItemMasterPL = new List<ItemDto>();
            if (!string.IsNullOrEmpty(connect_CentralMD_PL))
            {
                DapperContext dapperDbContext = new DapperContext(connect_CentralMD_PL);
                using var conn = dapperDbContext.CreateConnDB;
                conn.Open();
                string query = @"EXEC SP_API_GET_ITEMS;";
                dataItemMasterPL = conn.Query<ItemDto>(query).ToList();
                FileHelper.WriteLogs("ItemMaster PLH " + dataItemMasterPL.Count.ToString());
            }

            //Processing file
            var lstFile = FileHelper.GetFileFromDir(pathLocal, "*.txt");
            FileHelper.WriteLogs("Scan: " + pathLocal + "===> " + lstFile.Count.ToString());
            if (lstFile.Count > 0 && sysStoreSets.Count > 0 && sysConfigList.Count >0)
            {
                foreach (string file in lstFile)
                {
                    var storeNoSales = file.ToString().Substring(0, 8).Substring(4, 4);
                    string appCode = file.ToString().Substring(0, 3).ToUpper();
                    SysStoreSet checkSubSet = null;
                    SysConfig sysConfig = null;

                    if (appCode == "WCM")
                    {
                        checkSubSet = sysStoreSets.Where(x => x.StoreNo == storeNoSales).FirstOrDefault();
                    }
                    else if(appCode == "PLH")
                    {
                        checkSubSet = sysStoreSets.Where(x => x.StoreNoPLH == storeNoSales).FirstOrDefault();
                    }
                    else if(appCode == "PLF")
                    {
                        sysConfig = sysConfigList.Where(x => x.Name.ToUpper() == "PLH_FLAGSHIP" && x.AppCode == "KIOS").FirstOrDefault();
                        if (sysConfig != null && !string.IsNullOrEmpty(sysConfig.Prefix))
                        {
                            if (ImportTransWinmartToDB_V2(sysConfig.Prefix, "", pathLocal, pathArchive, pathError, file, isMoveFile, dataItemMasterPL))
                            {
                                FileHelper.WriteLogs("===> processed PLH Flagship: " + file.ToString());
                            }
                        }
                    }
                    //process insert db
                    if(checkSubSet != null)
                    {
                        sysConfig = sysConfigList.Where(x => x.Name == checkSubSet.SubSet && x.AppCode == "KIOS").FirstOrDefault();
                        if(sysConfig != null && !string.IsNullOrEmpty(sysConfig.Prefix))
                        {
                            if (ImportTransWinmartToDB_V2(sysConfig.Prefix, checkSubSet.StoreNo, pathLocal, pathArchive, pathError, file, isMoveFile, dataItemMasterPL))
                            {
                                FileHelper.WriteLogs("===> processed " + appCode +  ": "+ checkSubSet.StoreNo + "_" + file.ToString());
                            }
                        }
                    }
                }
            }

        }
        private bool ImportTransWinmartToDB_V2(string connectStringKIOS, string StoreNoBlue, string pathLocal, string pathArchive, string pathError, string fileName, bool isMoveFile, List<ItemDto> itemDtos = null)
        {
            bool result = false;
            string appCode = fileName.ToString().Substring(0, 3).ToUpper();
            if (APPCODE_KIOS.Contains(appCode))
            {
                var transRaw = JsonConvert.DeserializeObject<OrderRequestBody>(System.IO.File.ReadAllText(pathLocal + fileName));
                //FileHelper.WriteLogs(JsonConvert.SerializeObject(transRaw));
                if (transRaw != null)
                {
                    Console.WriteLine("OrderNo: {0}", transRaw.OrderNo);
                    var dbContext = new KiosDbContext(connectStringKIOS);
                    using var transaction = dbContext.Database.BeginTransaction();
                    {
                        try
                        {
                            string orderNo = InitOrderNo(transRaw.OrderNo);
                            var saveTransHeader = SaveTransHeader(dbContext, transRaw, appCode, orderNo, StoreNoBlue);
                            var saveTransLine = SaveTransLine(dbContext, transRaw, orderNo, itemDtos);
                            var saveTransPaymentEntry = SaveTransPayment(dbContext, transRaw, orderNo);
                            if (saveTransHeader != null && saveTransLine != null && saveTransPaymentEntry != null)
                            {
                                transaction.Commit();
                                Console.WriteLine("DONE: " + orderNo);
                                if (isMoveFile) FileHelper.MoveFileToFolder(pathArchive, pathLocal + fileName);
                                result = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            FileHelper.WriteLogs("===> ImportTransToDB Exception: " + ex.Message.ToString());
                            result = false;
                        }
                    }
                }
                else
                {
                    if (isMoveFile) FileHelper.MoveFileToFolder(pathError, pathLocal + fileName);
                    result = false;
                }
            }
            return result;
        }

        private string GetDivisionCode(string appCode)
        {
            string division = "WCM";
            switch (appCode)
            {
                case "PLH":
                    division = "PLG";
                    break;
                case "WCM":
                    division = "WCM";
                    break;
                default:
                    break;
            }
            return division;
        }
        //Save to Database
        private KIOS_TransHeader SaveTransHeader(KiosDbContext dbContext, OrderRequestBody transHeader, string appCode, string orderNo, string StoreNoBlue)
        {
            int cashierId = int.Parse(transHeader.OrderNo.ToString().Substring(0, 6));
            string zoneNo = transHeader.StoreNo;
            if (!string.IsNullOrEmpty(StoreNoBlue))
            {
                cashierId = int.Parse(StoreNoBlue + transHeader.OrderNo.ToString().Substring(0, 6).Substring(4, 2));
                zoneNo = StoreNoBlue;
            }

            KIOS_TransHeader tran = new KIOS_TransHeader()
            {
                OrderNo = orderNo,
                Version = "V2",
                OrderDate = transHeader.OrderDate,
                AppCode = appCode,
                StoreNo = transHeader.StoreNo,
                LocationId = 0,
                PosNo = transHeader.OrderNo.ToString().Substring(0, 6),
                ShiftNo = 0,
                CashierId = cashierId,
                CashierName = "KIOS",
                DiscountAmount = 0,
                VATAmount = 0,
                AmountExclVAT = 0,
                AmountInclVAT = transHeader.TotalAmount,
                AmountPaid = 0,
                AmountReturn = 0,
                State = "paid",
                CustNo = transHeader.WinCode??"",
                CustName = transHeader.CustName ?? "",
                CustPhone = transHeader.CustPhone??"",
                CustAddress = transHeader.CustAddress ?? "",
                CustEmail = "",
                OrderType = transHeader.SaleTypeId.ToString() ?? "",
                DeliveringMethod = 0,
                DeliveryComment = transHeader.Note ?? "",
                DeliveryDate = DateTime.Now,
                GeneralComment = transHeader.Note ?? "",
                ZoneNo = zoneNo,
                IssuedVATInvoice = false,
                TransactionType = 1,
                TransactionTypeName = "",
                OriginOrderNo = "",
                MemberCardNo = transHeader.CustPhone ?? "",
                MemberPointsEarn = 0,
                MemberPointsRedeem = 0,
                MemberPoint = 0,
                StartingTime = DateTime.Now,
                EndingTime = DateTime.Now,
                BillCreationTime = DateTime.Now.ToString("yyyyMMdd HH:MM:ss"),
                RefKey1 = transHeader.OrderNo,
                RefKey2 = "",
                StepProcess = 0,
                UpdateFlg = "N",
                IsEOD = false,
                CrtDate = DateTime.Now,
                ChgeDate = DateTime.Now
            };

            dbContext.KIOS_TransHeader.Add(tran);
            dbContext.SaveChanges();
            return tran;
        }
        private List<KIOS_TransLine> SaveTransLine(KiosDbContext dbContext, OrderRequestBody transRaw, string orderNo, List<ItemDto> itemDtos = null)
        {
            List<KIOS_TransLine> transLine = new List<KIOS_TransLine>();
            List<KIOS_TransDiscountEntry> transDiscountEntry = new List<KIOS_TransDiscountEntry>();
            int lineNo = 0;
            int lineNoDiscount = 0;
            var tmpTransLine = transRaw.Items;
            if (tmpTransLine.Count > 0)
            {
                foreach (var item in tmpTransLine.OrderBy(x => x.LineId))
                {
                    lineNo++;
                    int parentLineNo = lineNo;
                    decimal VatPercent = VATConst.MappingTaxWCM()[int.Parse(item.TaxGroupCode)];

                    if (itemDtos != null && transRaw.AppCode == "PLF")
                    {
                        var itemMasterLine = itemDtos.FirstOrDefault(x => x.ItemNo == item.ItemNo);
                        if(itemMasterLine == null) 
                        {
                            FileHelper.WriteLogs(string.Format(@"Đơn hàng {0} có Sản phẩm {1} không tồn tại", orderNo, item.ItemNo));
                            return null; 
                        }
                        item.TaxGroupCode = itemMasterLine.VatGroup;
                        VatPercent = VATConst.MappingTaxPLH()[int.Parse(item.TaxGroupCode)];
                    }

                    decimal discountZB21 = 0;
                    var checkZB21 = item.DiscountEntry.Where(x => x.OfferType == "ZB21").FirstOrDefault();
                    if(checkZB21 != null)
                    {
                        discountZB21 = checkZB21.DiscountAmount;
                    }

                    decimal LineAmountInclVAT = ((item.UnitPrice * item.Quantity) - item.DiscountAmount) + discountZB21;
                    decimal DiscountAmount = item.DiscountAmount - discountZB21;

                    decimal NetPrice = Math.Round((LineAmountInclVAT / (1 + VatPercent / 100)) / item.Quantity, 0);

                    decimal VatAmount = LineAmountInclVAT - NetPrice * item.Quantity;
                    var offerNo = "";
                    if(item.DiscountEntry.Count > 0)
                    {
                        offerNo = item.DiscountEntry.OrderByDescending(x => x.DiscountAmount).FirstOrDefault().OfferNo;
                    }

                    //Add transLine
                    transLine.Add(new KIOS_TransLine()
                    {
                        OrderId = 0,
                        LineId = 0,
                        LineNo = item.LineId, //item.id,
                        OrderNo = orderNo,
                        LineParent = item.ParentLineId,
                        LineName = "",
                        LineType = item.UnitPrice > 0 ? 0 : 9,
                        Barcode = item.Barcode,
                        ItemNo = item.ItemNo,
                        ItemName = item.ItemName,
                        Uom = item.Uom,
                        UomVN = item.Uom,
                        Quantity = item.Quantity,
                        OriginPrice = item.OldPrice,
                        UnitPrice = item.UnitPrice,
                        NetPrice = NetPrice,
                        DiscountPercent = 0,
                        DiscountAmount = DiscountAmount,
                        PercentPartner = 0,
                        DiscountPartner = 0,
                        CommissionsAmount = 0,
                        VATGroup = item.TaxGroupCode,
                        VATPercent = VatPercent,
                        VATAmount = VatAmount, //Math.Round(item.price_subtotal_incl, 0) - Math.Round(item.price_subtotal, 0),
                        LineAmountExcVAT = LineAmountInclVAT - VatAmount,   //Math.Round(item.price_subtotal, 0),
                        LineAmountIncVAT = LineAmountInclVAT, // Math.Round(item.price_subtotal_incl, 0),
                        OdooDiscountAmount = 0, //tt giam gia tren odoo
                        OdooAmountExcVat = 0,
                        OdooAmountIncVAT = 0,
                        LocationId = 0,
                        WarehouseId = 0,
                        DivisionCode = GetDivisionCode(transRaw.AppCode),
                        CategoryCode = "",
                        ProductGroupCode = "",
                        SerialNo = item.ItemNo ?? "",
                        VariantNo = item.Size ?? "",
                        ItemType = "",
                        OrderType = transRaw.SaleTypeId,
                        LotNo = item.Size ?? "",
                        ComboId = item.ComboId,
                        IsDoneCombo = item.IsCombo,
                        ComboQty = "",
                        ComboSeq = "",
                        CupType = item.CupType ?? "",
                        ExpireDate = DateTime.Now.Date,
                        BlockedMemberPoint = true,
                        BlockedPromotion = true,
                        MemberPointsEarn = 0,
                        MemberPointsRedeem = 0,
                        DeliveringMethod = 0,
                        ReturnedQuantity = 0,
                        DeliveryQuantity = 0,
                        DeliveryStatus = 0,
                        OfferNo = offerNo,
                        UpdateFlg = "N",
                        ChgeDate = DateTime.Now
                    }) ;

                    if (item.DiscountEntry.Count > 0)
                    {
                        foreach(var discount in item.DiscountEntry)
                        {
                            lineNoDiscount++;
                            transDiscountEntry.Add(new KIOS_TransDiscountEntry()
                            {
                                OrderNo = orderNo,
                                OrderId = parentLineNo,
                                LineId = discount.LineId,
                                LineNo = lineNoDiscount,
                                OrderLineNo = parentLineNo,
                                OfferNo = discount.OfferNo,
                                OfferType = discount.OfferType,
                                Quantity = discount.Quantity,
                                DiscountType = 0,
                                DiscountAmount = discount.DiscountAmount,
                                ParentLineNo = item.LineId,
                                ItemNo = item.ItemNo,
                                LineGroup = ""
                            }); ;
                        }
                        
                    }
                }//end foreach

                //check valueDiscountAmount
                //FileHelper.WriteLogs(JsonConvert.SerializeObject(transDiscountEntry));
                transLine.ForEach(n => dbContext.KIOS_TransLine.Add(n));
                if (transDiscountEntry.Count > 0)
                {
                    transDiscountEntry.ForEach(n => dbContext.KIOS_TransDiscountEntry.Add(n));
                }
                dbContext.SaveChanges();
                return transLine;
            }
            else
            {
                return null;
            }
        }
        private List<KIOS_TransPaymentEntry> SaveTransPayment(KiosDbContext dbContext, OrderRequestBody transRaw, string orderNo)
        {
            var transPayments = transRaw.Payments;
            decimal paymentZB21 = 0;
            foreach(var line in transRaw.Items)
            {
                if(line.DiscountEntry.Count > 0)
                {
                    paymentZB21 += line.DiscountEntry.Where(x => x.OfferType == "ZB21").Sum(x => x.DiscountAmount);
                }
            }
            if (transPayments.Count > 0)
            {
                List<KIOS_TransPaymentEntry> payments = new List<KIOS_TransPaymentEntry>();
                int i = 0;
                foreach (var item in transPayments)
                {
                    i++;
                    decimal paymentAmount = item.AmountTendered;

                    payments.Add(new KIOS_TransPaymentEntry()
                    {
                        PaymentTime = DateTime.Now,
                        PaymentDate = transRaw.OrderDate,
                        LineNo = i,
                        LineId = i,
                        OrderId = item.LineId,
                        WarehouseId = 0,
                        OrderNo = orderNo,
                        StoreNo = transRaw.StoreNo,
                        PosNo = transRaw.OrderNo.Substring(0,6),
                        ShiftNo = "0",
                        StaffID = "",
                        ExchangeRate = item.ExchangeRate,
                        PaymentMethod = 0,
                        TenderType = item.PaymentMethod,
                        TenderTypeName = "",
                        AmountTendered = paymentAmount,
                        CurrencyCode = "VND",
                        AmountInCurrency = paymentAmount,
                        ReferenceNo = item.TransactionNo??"",
                        CardOrAccount = "",
                        PayForOrderNo = "",
                        ApprovalCode = item.ApprovalCode??"",
                        BankCardType = "",
                        BankPOSCode = item.ApprovalCode??"",
                        IsOnline = false,
                        PayInfo = item.TraceCode??""
                    });
                }

                if(paymentZB21 > 0)
                {
                    payments.Add(new KIOS_TransPaymentEntry()
                    {
                        PaymentTime = DateTime.Now,
                        PaymentDate = transRaw.OrderDate,
                        LineNo = i + 10,
                        LineId = i,
                        OrderId = 0,
                        WarehouseId = 0,
                        OrderNo = orderNo,
                        StoreNo = transRaw.StoreNo,
                        PosNo = transRaw.OrderNo.Substring(0, 6),
                        ShiftNo = "0",
                        StaffID = "",
                        ExchangeRate = 1,
                        PaymentMethod = 0,
                        TenderType = "ZTSP",
                        TenderTypeName = "Trusting Social Pay ",
                        AmountTendered = paymentZB21,
                        CurrencyCode = "VND",
                        AmountInCurrency = paymentZB21,
                        ReferenceNo = "",
                        CardOrAccount = "",
                        PayForOrderNo = "",
                        ApprovalCode = "",
                        BankCardType = "",
                        BankPOSCode = "",
                        IsOnline = false,
                        PayInfo = ""
                    });
                }
                // FileHelper.WriteLogs(JsonConvert.SerializeObject(payments));
                payments.ForEach(n => dbContext.KIOS_TransPaymentEntry.Add(n));
                dbContext.SaveChanges();
                return payments;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// PHUCLONG
        /// </summary>
        /// <param name="appCode"></param>
        /// <param name="connectString"></param>
        /// <param name="pathLocal"></param>
        /// <param name="pathArchive"></param>
        /// <param name="pathError"></param>
        /// <param name="isMoveFile"></param>
        /// <param name="maxFile"></param>
        public void ImportTransPhucLongToDB(string appCode, string connectString, string pathLocal, string pathArchive, string pathError, bool isMoveFile, int maxFile)
        {
            var lstFile = FileHelper.GetFileFromDir(pathLocal, "*.txt");
            FileHelper.WriteLogs("Scan: " + pathLocal + "===> " + lstFile.Count.ToString());
            if (lstFile.Count > 0)
            {
                int affectedRows = 0;
                var dbContext = new InboundDbContext(connectString);
                foreach (string file in lstFile)
                {
                    if (file.ToString().Substring(0, 3).ToUpper() == appCode)
                    {
                        using var transaction = dbContext.Database.BeginTransaction();
                        {
                            try
                            {
                                var transRaw = JsonConvert.DeserializeObject<OrderRequestBody>(System.IO.File.ReadAllText(pathLocal + file));
                                //FileHelper.WriteLogs(JsonConvert.SerializeObject(transRaw));
                                if (transRaw != null)
                                {
                                    Console.WriteLine("OrderNo: {0}", transRaw.OrderNo);
                                    string ver = "KIOS";
                                    string orderNo = InitOrderNo(transRaw.OrderNo);
                                    var saveTransHeader = SaveTransHeader_PLH_KIOS(dbContext, transRaw, ver, appCode, orderNo);
                                    var saveTransLine = SaveTransLine_PLH_KIOS(dbContext, transRaw, orderNo);
                                    var saveTransPaymentEntry = SaveTransPayment_PLH_KIOS(dbContext, transRaw, orderNo);
                                    if (saveTransHeader != null && saveTransLine != null && saveTransPaymentEntry != null)
                                    {
                                        transaction.Commit();
                                        Console.WriteLine("Done: " + orderNo);
                                        if (isMoveFile) FileHelper.MoveFileToFolder(pathArchive, pathLocal + file);
                                    }
                                }
                                else
                                {
                                    if (isMoveFile) FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                FileHelper.WriteLogs("===> ImportTransToDB Exception: " + ex.Message.ToString());
                            }
                        }
                        affectedRows++;
                    }
                    if (affectedRows == maxFile)
                    {
                        break;
                    }
                }
            }

        }
        private OCCTransHeader SaveTransHeader_PLH_KIOS(InboundDbContext dbContext, OrderRequestBody fistStagingTransLine,string version, string appCode, string orderNo)
        {
            var transHeader = new OCCTransHeader()
            {
                AppCode = appCode,
                Version = version,
                OrderNo = orderNo,
                OrderNo2 = fistStagingTransLine.OrderNo,
                OrderDate = fistStagingTransLine.OrderDate.Date,
                StoreNo = fistStagingTransLine.StoreNo,
                StoreNo2 = fistStagingTransLine.StoreNo,
                PosNo = fistStagingTransLine.OrderNo.Substring(0,6),
                OrderType = fistStagingTransLine.SaleTypeId.ToString(),
                TransactionType = "PLT1",
                BeginDate = fistStagingTransLine.OrderDate.Date.ToString("yyyyMMdd") + DateTime.Now.ToString("HHmmss"),
                EndDate = fistStagingTransLine.OrderDate.Date.ToString("yyyyMMdd") + DateTime.Now.AddMinutes(5).ToString("HHmmss"),
                TransactionCurrency = "VND",
                MemberCardNo = "",
                MemberPointsEarn = 0,
                MemberPointsRedeem = 0,
                RefKey = "",
                UpdateFlg = "N",
                IssuedVATInvoice = false,
                IsEOD = false,
                CrtDate = DateTime.Now
            };
            //FileHelper.WriteLogs(JsonConvert.SerializeObject(transHeader));
            dbContext.OCCTransHeader.Add(transHeader);
            dbContext.SaveChanges();
            return transHeader;
        }
        private List<OCCTransLine> SaveTransLine_PLH_KIOS(InboundDbContext dbContext, OrderRequestBody transRaw, string orderNo)
        {
            List<OCCTransLine> transLines = new List<OCCTransLine>();
            if (transRaw.Items.Count > 0)
            {
                foreach (var item in transRaw.Items)
                {
                    decimal VatPercent = VATConst.MappingTaxPLH()[int.Parse(item.TaxGroupCode)];
                    decimal NetPrice = 0;
                    decimal VatAmount = 0;
                    
                    decimal discountZB21 = 0;
                    var checkZB21 = item.DiscountEntry.Where(x => x.OfferType == "ZB21").FirstOrDefault();
                    if (checkZB21 != null)
                    {
                        discountZB21 = checkZB21.DiscountAmount;
                    }

                    decimal LineAmountIncVAT = (item.UnitPrice * item.Quantity - item.DiscountAmount) + discountZB21;
                    decimal DiscountAmount = item.DiscountAmount - discountZB21;

                    if (item.UnitPrice > 0)
                    {
                        VatPercent = KIOSConts.MappingTax()[item.TaxGroupCode];
                        
                        NetPrice = Math.Round((LineAmountIncVAT / (1 + VatPercent / 100)) / item.Quantity, 0);

                        VatAmount = LineAmountIncVAT - NetPrice * item.Quantity;
                    }

                    transLines.Add(new OCCTransLine()
                    {
                        OrderNo = orderNo,
                        LineType = item.UnitPrice > 0 ? 0 : 9,
                        LineNo = item.LineId,
                        LineNo2 = item.LineId,
                        ParentLineNo = item.ParentLineId,
                        ItemNo = item.ItemNo,
                        ItemNo2 = item.ItemNo,
                        ItemName = item.ItemName,
                        Uom = item.Uom,
                        Quantity = item.Quantity,
                        OriginPrice = item.UnitPrice,
                        UnitPrice = item.UnitPrice,
                        NetPrice = NetPrice,
                        DiscountAmount = DiscountAmount,
                        VATPercent = VatPercent,
                        VATGroup = item.TaxGroupCode??"0",
                        VATAmount = VatAmount,
                        LineAmountExcVAT = LineAmountIncVAT - VatAmount,
                        LineAmountIncVAT = LineAmountIncVAT,
                        OriginLineAmountIncVAT = item.UnitPrice * item.Quantity,
                        RevenueShareRatio = 0,
                        CommissionsAmount = 0,
                        MemberPointsEarn = 0,
                        MemberPointsRedeem = 0,
                        CupType = item.CupType??"",
                        Size = item.Size??"",
                        IsCombo = item.IsCombo,
                        IsTopping = item.IsTopping,
                        ScanTime = DateTime.Now,
                        UpdateFlg = "N",
                        CrtDate = DateTime.Now
                    }); 
                }
                transLines.ForEach(n => dbContext.OCCTransLine.Add(n));
                dbContext.SaveChanges();
                return transLines;
            }
            else
            {
                return null;
            }
        }
        private List<OCCTransPaymentEntry> SaveTransPayment_PLH_KIOS(InboundDbContext dbContext, OrderRequestBody transRaw, string orderNo)
        {
            List<OCCTransPaymentEntry> transPaymentEntries = new List<OCCTransPaymentEntry>();

            var transPayments = transRaw.Payments;
            decimal paymentZB21 = 0;
            foreach (var line in transRaw.Items)
            {
                if (line.DiscountEntry.Count > 0)
                {
                    paymentZB21 += line.DiscountEntry.Where(x => x.OfferType == "ZB21").Sum(x => x.DiscountAmount);
                }
            }

            if (transRaw.Payments.Count > 0)
            {
                int i = 0;
                foreach (var item in transRaw.Payments)
                {
                    i++;
                    transPaymentEntries.Add(new OCCTransPaymentEntry()
                    {
                        OrderNo = orderNo,
                        PaymentDate = transRaw.OrderDate,
                        LineNo = i,
                        StoreNo = transRaw.StoreNo,
                        PosNo = transRaw.OrderNo.Substring(0,6),
                        TenderType = item.PaymentMethod,
                        ExchangeRate = 1,
                        CurrencyCode = "VND",
                        AmountInCurrency = item.AmountTendered,
                        AmountTendered = item.AmountTendered,
                        ReferenceNo = item.ApprovalCode!= null ? item.ApprovalCode: (item.TraceCode??""),
                        TransactionNo = item.TransactionNo,
                        ApprovalCode = item.ApprovalCode??"",
                        TraceCode = item.TraceCode??""
                    });
                }

                if(paymentZB21 > 0)
                {
                    transPaymentEntries.Add(new OCCTransPaymentEntry()
                    {
                        OrderNo = orderNo,
                        PaymentDate = transRaw.OrderDate,
                        LineNo = i+10,
                        StoreNo = transRaw.StoreNo,
                        PosNo = transRaw.OrderNo.Substring(0, 6),
                        TenderType = "ZTSP",
                        ExchangeRate = 1,
                        CurrencyCode = "VND",
                        AmountInCurrency = paymentZB21,
                        AmountTendered = paymentZB21,
                        ReferenceNo = "ZB21",
                        TransactionNo = "",
                        ApprovalCode = "",
                        TraceCode = ""
                    });
                }
                //FileHelper.WriteLogs(JsonConvert.SerializeObject(paymentEntry));
                transPaymentEntries.ForEach(n => dbContext.OCCTransPaymentEntry.Add(n));
                dbContext.SaveChanges();
                return transPaymentEntries;
            }
            else
            {
                return null;
            }
        }

        //private
        /*
        private KIOS_TransDiscountEntry AddTransDiscountEntryWCM(string orderNo, int lineNo, int orderLineNo,string offerNo, string offerType, int quantity, decimal discountAmount)
        {
            return new KIOS_TransDiscountEntry()
            {
                OrderNo = orderNo,
                OrderId = orderLineNo,
                LineId = lineNo,
                LineNo = lineNo,
                OrderLineNo = orderLineNo,
                OfferNo = offerNo,
                OfferType = offerType,
                Quantity = quantity,
                DiscountType = 0,
                DiscountAmount = discountAmount,
                ParentLineNo = orderLineNo,
                ItemNo = "",
                LineGroup = ""
            };
        }
        private List<KIOS_TransDiscountEntry> DistributionAmountToLine(List<KIOS_TransLine> transLine, string offerType, decimal specailSaleAmount)
        {
            List<KIOS_TransDiscountEntry> result = new List<KIOS_TransDiscountEntry>();
            try
            {
                decimal totalAmount = transLine.Sum(x => x.LineAmountIncVAT);
                decimal totalTempAmount = 0;
                decimal valueDiscountAmount = 0;
                int checkLine = 0;
                int tatalRows = transLine.Where(x => x.LineAmountIncVAT > 0).ToList().Count;
                int lineNo = 100;
                foreach (var item in transLine)
                {
                    if (item.LineAmountIncVAT > 0)
                    {
                        var percent = Math.Round((item.LineAmountIncVAT * 100) / totalAmount, 2);
                        valueDiscountAmount = Math.Round((specailSaleAmount * percent) / 100, 0);
                        totalTempAmount += valueDiscountAmount;
                        checkLine++;
                        if (checkLine == tatalRows)
                        {
                            if (totalTempAmount != specailSaleAmount)
                            {
                                valueDiscountAmount -= (totalTempAmount - specailSaleAmount);
                            }
                        }

                        result.Add(new KIOS_TransDiscountEntry()
                        {
                            OrderNo = item.OrderNo,
                            OrderId = item.LineId,
                            LineId = item.LineId,
                            LineNo = lineNo + 1,
                            OrderLineNo = item.LineId,
                            OfferNo = offerType,
                            OfferType = offerType,
                            Quantity = item.Quantity,
                            DiscountType = 0,
                            DiscountAmount = valueDiscountAmount,
                            ParentLineNo = item.LineId,
                            ItemNo = item.ItemNo,
                            LineGroup = ""
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("DistributionAmountToLine.Exception: " + ex.Message.ToString());
                result = null;
            }
            return result;
        }
        */
    
    }
}
