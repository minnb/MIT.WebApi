using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhucLong.Interface.Central.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VCM.Common.Helpers;
using VCM.Shared.Const;
using VCM.Shared.Entity.Central;
using VCM.Shared.Entity.PhucLong;

namespace PhucLong.Interface.Central.AppService
{
    public class TransAppService
    {
        private IConfiguration _config;
        private List<InterfaceEntry> _interfaceEntry;
        private CentralDbContext _dbContext;
        private List<Product_Product> _lstProduct_Product;
        private List<Product_Template> _lstProduct_Product_Template;
        private List<Stock_Location> _lstStock_Location;
        private List<Payment_Method> _lstPayment_Method;
        private List<Stock_Warehouse> _lstStock_Warehouse;
        private List<Uom_Uom> _lstUom_Uom;
        private List<Product_Taxes_Rel> _lstProduct_Taxes_Rel;
        private List<Pos_Config> _lstPos_Config;
        public TransAppService
            (
                IConfiguration config,
                List<InterfaceEntry> interfaceEntry,
                CentralDbContext dbContext
            )
        {
            _config = config;
            _interfaceEntry = interfaceEntry;
            _dbContext = dbContext;
        }
        private string GetMasterDataOdoo(string dataType, string pathLocalMaster)
        {
            var lstFile = DirectoryHelper.GetFileFromDirOrderByDes(pathLocalMaster, "*.txt");
            string strJson = string.Empty;
            try
            {
                if (lstFile.Count > 0)
                {
                    var fileSelected = lstFile.Where(x => x.Contains(dataType)).FirstOrDefault();
                    strJson = System.IO.File.ReadAllText(pathLocalMaster + fileSelected);
                    FileHelper.WriteLogs(pathLocalMaster + fileSelected);
                }
            }
            catch
            {
                strJson = "";
            }
            return strJson;
        }
        private void InitMaster(string pathLocalMaster)
        {
            _lstProduct_Product = JsonConvert.DeserializeObject<List<Product_Product>>(GetMasterDataOdoo(OdooConst.MappingMD_Odoo(10).ToString(), pathLocalMaster));
            _lstUom_Uom = JsonConvert.DeserializeObject<List<Uom_Uom>>(GetMasterDataOdoo(OdooConst.MappingMD_Odoo(14).ToString(), pathLocalMaster));
            _lstProduct_Taxes_Rel = JsonConvert.DeserializeObject<List<Product_Taxes_Rel>>(GetMasterDataOdoo(OdooConst.MappingMD_Odoo(13).ToString(), pathLocalMaster));
            _lstStock_Location = JsonConvert.DeserializeObject<List<Stock_Location>>(GetMasterDataOdoo(OdooConst.MappingMD_Odoo(30).ToString(), pathLocalMaster));
            _lstPayment_Method = JsonConvert.DeserializeObject<List<Payment_Method>>(GetMasterDataOdoo(OdooConst.MappingMD_Odoo(21).ToString(), pathLocalMaster));
            _lstPos_Config = JsonConvert.DeserializeObject<List<Pos_Config>>(GetMasterDataOdoo(OdooConst.MappingMD_Odoo(20).ToString(), pathLocalMaster));
            _lstProduct_Product_Template = JsonConvert.DeserializeObject<List<Product_Template>>(GetMasterDataOdoo(OdooConst.MappingMD_Odoo(12).ToString(), pathLocalMaster));
            _lstStock_Warehouse = JsonConvert.DeserializeObject<List<Stock_Warehouse>>(GetMasterDataOdoo(OdooConst.MappingMD_Odoo(31).ToString(), pathLocalMaster));
        }
        public void SaveTransaction_V2(string pathLocalMaster)
        {
            int affectedRows = 0;
            var interfaceInfo = _interfaceEntry.Where(x => x.JobName == "INB-SALES-V2").FirstOrDefault();
            bool isMoveFile = interfaceInfo.IsMoveFile;
            try
            {
                string pathError = interfaceInfo.SftpPath.ToString();
                string pathLocal = interfaceInfo.LocalPath.ToString();
                var appCode = "PLG";
                var lstFile = FileHelper.GetFileFromDir(pathLocal, "*.txt");
                FileHelper.WriteLogs("Scan: " + pathLocal + "===> " + lstFile.Count.ToString());
                if (lstFile.Count > 0)
                {
                    InitMaster(pathLocalMaster);

                    foreach (string file in lstFile)
                    {
                        Console.WriteLine(file);
                        if (file.ToString().Substring(0, 3).ToUpper() == appCode)
                        {
                            using var transaction = _dbContext.Database.BeginTransaction();
                            try
                            {
                                var transRaw = JsonConvert.DeserializeObject<PosRawDto>(System.IO.File.ReadAllText(pathLocal + file));
                                if (transRaw != null)
                                {
                                    var storeNo = transRaw.TransHeader.location_id.ToString().PadRight(4, '0');
                                    var orderNo = storeNo + transRaw.TransHeader.id.ToString().PadLeft(12, '0');   //transRaw.TransHeader.name.ToString().Replace("-", "");
                                    decimal discount_header = transRaw.TransHeader.discount_amount * (-1);

                                    var stock_warehouse = _lstStock_Warehouse.Where(x => x.lot_stock_id == transRaw.TransHeader.location_id).OrderBy(x => x.name).FirstOrDefault();
                                    var saveTransLine = SaveTransLine(transRaw.TransLine, stock_warehouse, discount_header, orderNo, storeNo, transRaw.TransHeader.sale_type_id);
                                    if (saveTransLine == null)
                                    {
                                        FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                                    }
                                    else
                                    {
                                        var saveTransHeader = SaveTransHeader(transRaw.TransHeader, stock_warehouse, appCode, orderNo);
                                        if (SaveTransPayment(transRaw.TransPaymentEntry, stock_warehouse, orderNo, transRaw.TransHeader.cashier, transRaw.TransHeader.sale_type_id) != null && saveTransHeader != null)
                                        {
                                            transaction.Commit();
                                            Console.WriteLine("Done: " + orderNo);
                                            if (isMoveFile) FileHelper.MoveFileToFolder(interfaceInfo.LocalPathArchived, pathLocal + file);
                                        }
                                        else
                                        {
                                            if (isMoveFile) FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                                        }
                                    }
                                }
                                else
                                {
                                    if (isMoveFile) FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                                }
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                if (isMoveFile) FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                                ExceptionHelper.WriteExptionError("INB-SALES: ", ex);
                                //emailService.SendEmailNoAttach("WARRING: PHUC LONG - TRANSACTION - " + ex.Message.ToString(), _configuration["EmailConfig:jobs:NpgsqlConnection"].ToString(), ex.Message.ToString());
                            }
                        }
                        affectedRows++;
                        if (affectedRows == interfaceInfo.MaxFile)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.WriteExptionError("PushTransHeader ", ex);
                //emailService.SendEmailNoAttach("WARRING: PHUC LONG - " + ex.Message.ToString(), _configuration["EmailConfig:jobs:NpgsqlConnection"].ToString(), ex.Message.ToString());
            }
        }
        private TransHeader SaveTransHeader(Pos_Order transHeader, Stock_Warehouse stock_warehouse, string appCode, string orderNo)
        {
            TransHeader tran = new TransHeader()
            {
                OrderNo = orderNo,
                OrderDate = transHeader.date_order,
                AppCode = appCode,
                StoreNo = stock_warehouse.code,
                LocationId = transHeader.location_id,
                PosNo = transHeader.cashier.ToString(),
                ShiftNo = transHeader.sale_journal,
                CashierId = transHeader.cashier_id,
                DiscountAmount = transHeader.discount_amount,
                VATAmount = transHeader.amount_tax,
                AmountExclVAT = transHeader.amount_total - transHeader.amount_tax,
                AmountInclVAT = transHeader.amount_total,
                AmountPaid = transHeader.amount_paid,
                AmountReturn = transHeader.amount_return,
                State = transHeader.state,
                CustNo = transHeader.partner_id.ToString(),
                CustName = transHeader.note_label.ToString(),
                CustPhone = RegularHelper.RemoveNonNumeric(transHeader.partner_id.ToString()),
                CustAddress = stock_warehouse.name.ToString(),
                CustEmail = "",
                DeliveringMethod = transHeader.sale_type_id,
                DeliveryComment = transHeader.note.ToString(),
                DeliveryDate = transHeader.create_date,
                GeneralComment = "",
                ZoneNo = stock_warehouse.code.ToString(),
                IssuedVATInvoice = transHeader.invoice_request,
                TransactionType = 0,
                OriginOrderNo = "",
                MemberCardNo = transHeader.partner_id.ToString(),
                MemberPointsEarn = transHeader.point_won,
                MemberPointsRedeem = transHeader.discount_amount * (-1),
                MemberPoint = 0,
                //InvoiceInfo = transHeader.invoice_request == true ? JsonConvert.SerializeObject(new { transHeader.invoice_vat, transHeader.invoice_name, transHeader.invoice_contact, transHeader.invoice_address, transHeader.invoice_email, transHeader.invoice_note }).ToString() : "",
                StartingTime = transHeader.create_date,
                EndingTime = transHeader.date_last_order,
                BillCreationTime = transHeader.hanging_time.ToString(),
                RefKey1 = transHeader.name,
                RefKey2 = "",
                StepProcess = 0,
                UpdateFlg = "N",
                CrtDate = DateTime.Now,
                ChgeDate = DateTime.Now
            };
            FileHelper.WriteLogs(JsonConvert.SerializeObject(tran));
            _dbContext.TransHeader.Add(tran);
            int row = _dbContext.SaveChanges();
            return tran;
        }
        private List<TransLine> SaveTransLine(List<Pos_Order_Line> transLinePLG, Stock_Warehouse stock_warehouse, decimal discount_amount, string orderNo, string storeNo, int orderType)
        {
            int tatalRows = transLinePLG.Count;
            int line = 0;
            if (tatalRows > 0)
            {
                List<TransLine> transLine = new List<TransLine>();
                List<TransDiscountEntry> transDiscountEntry = new List<TransDiscountEntry>();
                decimal total_amount_inc_vat = transLinePLG.Sum(x => x.price_subtotal_incl);

                foreach (var item in transLinePLG)
                {
                    line++;
                    var tax = _lstProduct_Taxes_Rel.Where(x => x.prod_id == item.product_id).FirstOrDefault();
                    var product = _lstProduct_Product.Where(x => x.id == item.product_id).FirstOrDefault();
                    var itemUom = _lstUom_Uom.Where(x => x.id == item.uom_id).FirstOrDefault();
                    var product_tmpl = _lstProduct_Product_Template.Where(x => x.id == product.product_tmpl_id).FirstOrDefault();

                    transLine.Add(new TransLine()
                    {
                        LineNo = line, //item.id,
                        OrderNo = orderNo,
                        LineType = item.is_promotion_line == true ? 9 : (item.price_unit == 0 ? 1 : 0),
                        ItemNo = product_tmpl != null ? product_tmpl.ref_code : item.product_id.ToString().PadLeft(8, '0'),
                        ItemName = product_tmpl.name,
                        Uom = StringHelper.MakeVNtoEN(itemUom.name).ToUpper(),
                        Quantity = item.qty,
                        UnitPrice = item.price_unit,
                        DiscountPercent = 0,
                        DiscountAmount = item.discount_amount,
                        VATGroup = tax!=null ? tax.tax_id.ToString() : "4",
                        VATPercent = VATConst.MappingTax()[tax != null ? tax.tax_id : 4],
                        VATAmount = Math.Round(item.price_subtotal_incl, 0) - Math.Round(item.price_subtotal, 0),
                        LineAmountExcVAT = Math.Round(item.price_subtotal,0),
                        LineAmountIncVAT = Math.Round(item.price_subtotal_incl,0),
                        LocationId = item.warehouse_id,
                        WarehouseId = item.warehouse_id,
                        DivisionCode = stock_warehouse.code,
                        CategoryCode = item.id.ToString(),
                        ProductGroupCode = "",
                        SerialNo = "",
                        VariantNo = "",
                        ItemType = item.product_id.ToString(),
                        OrderType = orderType,
                        LotNo = itemUom.name.ToUpper(),
                        ComboId = string.IsNullOrEmpty(item.combo_id.ToString()) ? 0 : item.combo_id,
                        IsDoneCombo = item.is_done_combo,
                        ComboQty = string.IsNullOrEmpty(item.combo_qty) ?  "" : item.combo_qty.ToString(),
                        ComboSeq = string.IsNullOrEmpty(item.combo_seq) ? "" : item.combo_seq.ToString(),
                        ExpireDate = item.date_order,
                        BlockedMemberPoint = true,
                        BlockedPromotion = true,
                        MemberPointsEarn = 0,
                        MemberPointsRedeem = 0,
                        DeliveringMethod = 0,
                        ReturnedQuantity = 0,
                        DeliveryQuantity = 0,
                        DeliveryStatus = 0,
                        UpdateFlg = "N",
                        ChgeDate = DateTime.Now
                    });
                }
                //FileHelper.WriteLogs(JsonConvert.SerializeObject(transLine));
                transLine.ForEach(n => _dbContext.TransLine.Add(n));
                _dbContext.SaveChanges();
                return transLine;
            }
            else
            {
                return null;
            }
        }
        private List<TransPaymentEntry> SaveTransPayment(List<Pos_Payment> transPayments, Stock_Warehouse stock_warehouse, string orderNo, string posNo, int orderType)
        {
            try
            {
                if (transPayments.Count > 0)
                {
                    List<TransPaymentEntry> payments = new List<TransPaymentEntry>();

                    foreach (var item in transPayments)
                    {
                        var paymentMethod = _lstPayment_Method.Where(x => x.id == item.payment_method_id).FirstOrDefault();

                        payments.Add(new TransPaymentEntry()
                        {
                            PaymentTime = DateTime.Now,
                            PaymentDate = item.payment_date,
                            LineNo = item.id,
                            WarehouseId = item.warehouse_id,
                            OrderNo = orderNo,
                            StoreNo = stock_warehouse.code,
                            PosNo = posNo,
                            ShiftNo = "",
                            StaffID = item.cashier_id.ToString(),
                            ExchangeRate = item.exchange_rate,
                            PaymentMethod = item.payment_method_id,
                            TenderType = paymentMethod.name,
                            TenderTypeName = paymentMethod.name,
                            AmountTendered = item.amount,
                            CurrencyCode = "VND",
                            AmountInCurrency = item.amount,
                            ReferenceNo = item.mercury_card_number ?? "",
                            CardOrAccount = "",
                            PayForOrderNo = "",
                            ApprovalCode = "",
                            BankCardType = "",
                            BankPOSCode = "",
                            IsOnline = false,
                            PayInfo = ""
                        });
                    }

                    FileHelper.WriteLogs(JsonConvert.SerializeObject(payments));
                    payments.ForEach(n => _dbContext.TransPaymentEntry.Add(n));
                    _dbContext.SaveChanges();
                    return payments;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("SaveTransPayment Exception: " + ex.InnerException.ToString());
                return null;
            }
        }
    }
}
