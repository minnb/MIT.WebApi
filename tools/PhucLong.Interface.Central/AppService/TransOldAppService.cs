using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhucLong.Interface.Central.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using VCM.Common.Helpers;
using VCM.Shared.Const;
using VCM.Shared.Entity.Central;
using VCM.Shared.Entity.PhucLong;
using VCM.Shared.Entity.PhucLong.Dtos;
using VCM.Shared.Enums;

namespace PhucLong.Interface.Central.AppService
{
    public class TransOldAppService
    {
        private IConfiguration _config;
        private CentralDbContext _dbContext;
        private LoggingService _loggingDb;
        private List<Product_Product> _lstProduct_Product;
        private List<Product_Template> _lstProduct_Product_Template;
        private List<Stock_Location> _lstStock_Location;
        private List<Payment_Method> _lstPayment_Method;
        private List<Stock_Warehouse> _lstStock_Warehouse;
        private List<Uom_Uom> _lstUom_Uom;
        private List<Product_Taxes_Rel> _lstProduct_Taxes_Rel;
        private List<Pos_Config> _lstPos_Config;
        private List<Pos_Sale_Type> _lstPos_Sale_Type;
        private List<ProductMaterialOdooDto> _Product_Material;
        private List<Sale_Promo_Header> _lst_Sale_Promo_Header;
        public TransOldAppService
            (
                IConfiguration config,
                CentralDbContext dbContext
            )
        {
            _config = config;
            _dbContext = dbContext;
            _loggingDb = new LoggingService(_config);
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
            _Product_Material = JsonConvert.DeserializeObject<List<ProductMaterialOdooDto>>(GetMasterDataOdoo(OdooConst.MappingMD_Odoo(15).ToString(), pathLocalMaster));
            _lstPos_Sale_Type = JsonConvert.DeserializeObject<List<Pos_Sale_Type>>(GetMasterDataOdoo(OdooConst.MappingMD_Odoo(16).ToString(), pathLocalMaster));
            _lst_Sale_Promo_Header = JsonConvert.DeserializeObject<List<Sale_Promo_Header>>(GetMasterDataOdoo(OdooConst.MappingMD_Odoo(40).ToString(), pathLocalMaster));
        }
        private string InitOrderNo(DateTime date_order, string warehouse_code, int id)
        {
            return warehouse_code + date_order.ToString("yyMMdd") + id.ToString().PadLeft(8,'0');
        }
        public int SaveTransaction_V1(string pathLocalMaster, string pathLocal, string pathArchive, string pathError, int maxFile, bool isMoveFile)
        {
            int affectedRows = 0;
            var appCode = "PLG";
            var lstFile = FileHelper.GetFileFromDir(pathLocal, "*.txt");
            FileHelper.WriteLogs("Scan: " + pathLocal + "===> " + lstFile.Count.ToString());
            if (lstFile.Count > 0)
            {
                InitMaster(pathLocalMaster);
                List<int> lstOrderId = new List<int>();
                foreach (string file in lstFile)
                {
                    Console.WriteLine(file);
                    string orderNo = string.Empty;
                    if (file.ToString().Substring(0, 3).ToUpper() == appCode)
                    {
                        var transRaw = JsonConvert.DeserializeObject<PosRawDto>(System.IO.File.ReadAllText(pathLocal + file));
                        if (transRaw != null)
                        {
                            using var transaction = _dbContext.Database.BeginTransaction();
                            try
                            {
                                var sap_code_order_type = int.Parse(_lstPos_Sale_Type.Where(x => x.id == transRaw.TransHeader.sale_type_id).FirstOrDefault().sap_code.ToString());

                                decimal discount_header = transRaw.TransHeader.discount_amount * (-1);

                                var stock_warehouse = _lstStock_Warehouse.Where(x => x.id == transRaw.TransHeader.warehouse_id).OrderBy(x => x.name).FirstOrDefault();
                                var pos_config = _lstPos_Config.Where(x => x.warehouse_id == stock_warehouse.id).FirstOrDefault();

                                if (stock_warehouse == null)
                                {
                                    FileHelper.WriteLogs("Không tồn tại warehouse_id: " + transRaw.TransHeader.warehouse_id.ToString());
                                    _loggingDb.LoggingToDB("INB-SALES", orderNo, transRaw.TransHeader.warehouse_id.ToString() + " - mã warehouse_id không tồn tại", "N");
                                    break;
                                }
                                
                                orderNo = InitOrderNo(transRaw.TransHeader.date_order, stock_warehouse.code, transRaw.TransHeader.id);

                                string storeNo = stock_warehouse.code.ToString();
                                string posNo = pos_config.name.ToString();

                                var saveTransLine = SaveTransLine(transRaw.TransHeader, transRaw.TransLine.OrderBy(x => x.id).ToList(), transRaw.PosOrderLineOption, stock_warehouse, orderNo, sap_code_order_type, discount_header);

                                if (saveTransLine == null)
                                {
                                    FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                                    break;
                                }
                                else
                                {
                                    var saveTransHeader = SaveTransHeader(transRaw.TransHeader, stock_warehouse, appCode, orderNo, posNo, saveTransLine.Sum(x => x.LineAmountIncVAT), saveTransLine.Sum(x => x.DiscountAmount), saveTransLine.Sum(x => x.VATAmount));
                                    var savePaymentEntry = SaveTransPayment(transRaw.TransPaymentEntry, stock_warehouse, orderNo, posNo, sap_code_order_type);
                                    if (savePaymentEntry != null && saveTransHeader != null)
                                    {
                                        //SaveTransLineOptions(transRaw.PosOrderLineOption, orderNo);
                                        int lineNumber = savePaymentEntry.LastOrDefault().LineNo;
                                        decimal lineAmountTotal = saveTransLine.Sum(x => x.LineAmountIncVAT);
                                        decimal paymentAmountTotal = savePaymentEntry.Sum(x => x.AmountTendered);

                                        if (lineAmountTotal != paymentAmountTotal)
                                        {
                                            lineNumber++;
                                            decimal forfit = lineAmountTotal - paymentAmountTotal;
                                            string tenderForfit = forfit < 0 ? "CA01" : "CA02";
                                            AddTransPaymentEntry(savePaymentEntry, stock_warehouse, lineNumber, orderNo, posNo, 999, tenderForfit, forfit, "Money Difference");
                                        }

                                        transaction.Commit();
                                        Console.WriteLine("Done: " + orderNo);
                                        if (isMoveFile) FileHelper.MoveFileToFolder(pathArchive, pathLocal + file);
                                    }
                                    else
                                    {
                                        if (isMoveFile) FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                                        break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                if (isMoveFile) FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                                FileHelper.WriteLogs("ExceptionHelper: " + file);
                                ExceptionHelper.WriteExptionError("INB-SALES: ", ex);
                                orderNo = string.Empty;
                                break;
                            }
                        }
                        else
                        {
                            if (isMoveFile) FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                            break;
                        }
                    }

                    if (file.ToString().Substring(0, 16).ToUpper() == "POS_ORDER_CANCEL")
                    {
                        using var transaction = _dbContext.Database.BeginTransaction();
                        try
                        {
                            var posOrderCancel = JsonConvert.DeserializeObject<PosOrderCancelDto>(System.IO.File.ReadAllText(pathLocal + file));
                            if (posOrderCancel != null)
                            {
                                bool isDone = false;
                                var insTransCancel = SaveTransCancel(posOrderCancel);
                                if (insTransCancel != null)
                                {
                                    var upTransHeader = _dbContext.TransHeader.Where(x => x.OrderNo == insTransCancel.OrderNo && x.LocationId == insTransCancel.LocationId).FirstOrDefault();
                                    if (upTransHeader != null)
                                    {
                                        upTransHeader.State = insTransCancel.State;
                                        _dbContext.SaveChanges();
                                        isDone = true;
                                    }
                                }
                                transaction.Commit();
                                Console.WriteLine("Update POS_ORDER_CANCEL: " + posOrderCancel.name);
                                if (isMoveFile && isDone) FileHelper.MoveFileToFolder(pathArchive, pathLocal + file);
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            //if (isMoveFile) FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                            FileHelper.WriteLogs("ExceptionHelper: " + file);
                            ExceptionHelper.WriteExptionError("POS_ORDER_CANCEL: ", ex);
                        }
                    }

                    if (file.ToString().Substring(0, 7).ToUpper() == "POS_VAT")
                    {
                        using var transaction = _dbContext.Database.BeginTransaction();
                        try
                        {
                            var rawData = JsonConvert.DeserializeObject<PosRequestVatDto>(FileHelper.ReadFileTxt(pathLocal + file));
                            if (rawData != null)
                            {
                                if (!lstOrderId.Contains(rawData.id))
                                {
                                    lstOrderId.Add(rawData.id);
                                    SaveTransInfoVat(rawData);
                                    transaction.Commit();
                                    Console.WriteLine("POS_VAT successfully: " + rawData.name);
                                    if (isMoveFile) FileHelper.MoveFileToFolder(pathArchive, pathLocal + file);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            FileHelper.WriteLogs("ExceptionHelper: " + file);
                            ExceptionHelper.WriteExptionError("POS_VAT ", ex);
                            if (isMoveFile) FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                            break;
                        }
                        finally
                        {
                            transaction.Dispose();
                        }
                    }
                    //finished
                    affectedRows++;
                    if (affectedRows == maxFile)
                    {
                        break;
                    }
                }
            }
            return affectedRows;
        }
        private TransHeader SaveTransHeader(Pos_Order transHeader, Stock_Warehouse stock_warehouse, string appCode, string orderNo, string posNo, decimal totalAmount, decimal totalDisc, decimal vatAmount)
        {
            string promotion_name = string.Empty;
            if(transHeader.promotion_id > 0)
            {
                var promotion = _lst_Sale_Promo_Header.Where(x => x.id == transHeader.promotion_id).FirstOrDefault();
                promotion_name = promotion != null ? ("PROMO: " + promotion.id.ToString() +" | " + promotion.name) : "";
            }
            TransHeader tran = new TransHeader()
            {
                OrderNo = orderNo,
                Version = "V1",
                OrderDate = transHeader.date_order,
                AppCode = appCode,
                StoreNo = stock_warehouse.code,
                LocationId = transHeader.location_id,
                PosNo = posNo,
                ShiftNo = transHeader.sale_journal,
                CashierId = transHeader.cashier_id,
                CashierName = transHeader.cashier.ToString(),
                DiscountAmount = (-1) * Math.Round(transHeader.discount_amount, 0),
                VATAmount = vatAmount,
                AmountExclVAT = totalAmount - vatAmount,
                AmountInclVAT = totalAmount,
                AmountPaid = transHeader.amount_paid,
                AmountReturn = transHeader.amount_return,
                State = transHeader.state,
                CustNo = transHeader.partner_id.ToString(),
                CustName = transHeader.note_label.ToString(),
                CustPhone = RegularHelper.RemoveNonNumeric(transHeader.note_label.ToString()),
                CustAddress = stock_warehouse.name.ToString(),
                CustEmail = "",
                OrderType = _lstPos_Sale_Type.Where(x=>x.id == transHeader.sale_type_id).FirstOrDefault().sap_code.ToString(),
                DeliveringMethod = transHeader.sale_type_id,
                DeliveryComment = transHeader.note??"",
                DeliveryDate = transHeader.create_date,
                GeneralComment = promotion_name,
                ZoneNo = stock_warehouse.code.ToString(),
                IssuedVATInvoice = transHeader.invoice_request,
                TransactionType = (int)TransactionTypePLG.PLT1,
                TransactionTypeName = TransactionTypePLG.PLT1.ToString(),
                OriginOrderNo = "",
                MemberCardNo = transHeader.cv_life_partner_card_code??"",
                MemberPointsEarn = 0,
                MemberPointsRedeem = 0,
                MemberPoint = 0,
                //InvoiceInfo = transHeader.invoice_request == true ? JsonConvert.SerializeObject(new { transHeader.invoice_vat, transHeader.invoice_name, transHeader.invoice_contact, transHeader.invoice_address, transHeader.invoice_email, transHeader.invoice_note }).ToString() : "",
                StartingTime = transHeader.create_date,
                EndingTime = transHeader.date_last_order,
                BillCreationTime = transHeader.hanging_time.ToString(),
                RefKey1 = transHeader.name,
                RefKey2 = transHeader.id.ToString(),
                StepProcess = 0,
                UpdateFlg = "N",
                IsEOD = false,
                CrtDate = DateTime.Now,
                ChgeDate = DateTime.Now
            };

            //FileHelper.WriteLogs(JsonConvert.SerializeObject(tran));
            if(transHeader.invoice_request == true)
            {
                _dbContext.TransInfoVAT.Add(new TransInfoVAT()
                {
                    OrderNo = orderNo,
                    TaxID = transHeader.invoice_vat,
                    CustName = transHeader.invoice_name,
                    CompanyName = transHeader.invoice_name,
                    Address = transHeader.invoice_address,
                    Email = transHeader.invoice_email,
                    Phone = transHeader.invoice_contact,
                    Note = transHeader.invoice_note,
                    WriteDate = transHeader.create_date,
                    CrtDate = transHeader.create_date,
                    RefNo = transHeader.name,
                    InsertDate = DateTime.Now
                });
            }
            _dbContext.TransHeader.Add(tran);
           _dbContext.SaveChanges();
            return tran;
        }
        private List<TransLine> SaveTransLine(Pos_Order transHeaderPLG, List<Pos_Order_Line> transLinePLG, List<Pos_Order_Line_Option> transLineOptions, Stock_Warehouse stock_warehouse, string orderNo, int orderType, decimal discount_amount)
        {
            List<TransLine> transLine = new List<TransLine>();
            List<TransDiscountEntry> transDiscountEntry = new List<TransDiscountEntry>();
            int tatalRows = transLinePLG.Where(x=>x.old_price > 0).ToList().Count;
            int checkLine = 0;
            int lineNo = 0;
            if (transLinePLG.Count > 0)
            {
                decimal temp_discount = 0;
                decimal total_amount_inc_vat = transLinePLG.Sum(x => x.price_subtotal_incl);
                //decimal total_amount_inc_vat = transHeaderPLG.amount_total;
                int discountLineNo = 0;
                foreach (var item in transLinePLG.OrderBy(x=>x.id))
                {
                    if(item.product_id == 3 && item.old_price == 0)
                    {
                        continue;
                    }

                    lineNo++;
                    int parentLineNo = lineNo;
                    var product = _lstProduct_Product.Where(x => x.id == item.product_id).FirstOrDefault();
                    var itemUom = _lstUom_Uom.Where(x => x.id == item.uom_id).FirstOrDefault();
                    var product_tmpl = _lstProduct_Product_Template.Where(x => x.id == product.product_tmpl_id).FirstOrDefault();
                    var tax = _lstProduct_Taxes_Rel.Where(x => x.prod_id == product_tmpl.id).FirstOrDefault();

                    if (product_tmpl == null || string.IsNullOrEmpty(product_tmpl.sap_code) || string.IsNullOrEmpty(product_tmpl.sap_uom))
                    {
                        FileHelper.WriteLogs(orderNo + product.display_name + " @chưa mapping mã sản phẩm - Uom SAP");
                        _loggingDb.LoggingToDB("INB-SALES", orderNo, product_tmpl.ref_code + " - " + product_tmpl.name + " -  chưa mapping mã sản phẩm - Uom SAP");
                        return null;
                    }

                    if (tax == null)
                    {
                        FileHelper.WriteLogs(orderNo + product.display_name + " @chưa khai báo thuế theo mã SAP");
                        _loggingDb.LoggingToDB("INB-SALES", orderNo, product_tmpl.ref_code + " - " + product_tmpl.name + " - chưa khai báo thuế theo mã SAP", "N");
                        return null;
                    }
 
                    decimal discount_percent_vcm = 0;
                    decimal discount_amount_vcm = 0;
                    decimal discountLoyalty = 0;
                    decimal discount = 0;
                    decimal discountPercent = 0;
                    string des_loyalty = "";
                    item.price_unit = Math.Round(item.price_unit, 0);
                    item.price_subtotal_incl = Math.Round(item.price_subtotal_incl, 0);
                    item.price_subtotal = Math.Round(item.price_subtotal, 2);

                    if(item.price_unit > 0)
                    {
                        checkLine++;
                    }
                    
                    if (item.is_loyalty_line == true)
                    {
                        if(item.loyalty_discount_percent != 0)
                        {
                            discountLoyalty = Math.Round((item.loyalty_discount_percent * (item.qty * item.price_unit)) / 100, 0);
                            discountPercent = item.loyalty_discount_percent;
                            des_loyalty = "LOY-" + item.loyalty_discount_percent.ToString() + "P";
                        }

                        if (item.loyalty_point_cost > 0)
                        {
                            des_loyalty = "LOY-" + item.loyalty_point_cost.ToString() + "P";
                            //discountLoyalty = item.loyalty_point_cost == 100 ? Math.Round((item.loyalty_point_cost * (item.qty * item.old_price)) / 100, 0) : Math.Round((item.loyalty_point_cost * (item.qty * item.price_unit)) / 100, 0);
                            //discountPercent = item.loyalty_point_cost;
                        }
                    }

                    if (item.is_condition_line )
                    {
                        discountPercent = item.discount;
                        discount = Math.Round(((item.qty * item.price_unit) * item.discount) / 100, 0);
                    }

                    if (item.product_id > 0 || item.promotion_condition_id > 0)
                    {
                        if (item.discount_amount > 0)
                        {
                            discount = item.discount_amount;
                            discountPercent = 100 - Math.Round((item.discount_amount * 100) / item.price_subtotal, 0);
                        }
                        else if (item.discount > 0)
                        {
                            discountPercent = item.discount;
                            discount = Math.Round((item.discount * item.price_unit * item.qty) / 100, 0);
                        }
                    }

                    if (discount_amount > 0 && item.price_unit > 0)
                    {
                        decimal discount_amount_CK = Math.Round(discount_amount * 100 / (100), 0);
                        if(total_amount_inc_vat > 0)
                        {
                            discount_percent_vcm = Math.Round((item.price_subtotal_incl * 100) / total_amount_inc_vat, 0);
                        }
                        else
                        {
                            discount_percent_vcm = Math.Round((item.price_subtotal_incl * 100) / discount_amount, 0);
                        }

                        discount_amount_vcm = Math.Round((discount_amount_CK * discount_percent_vcm) / 100, 0);
                        temp_discount += discount_amount_vcm;

                        if (checkLine == tatalRows)
                        {
                            if (temp_discount != discount_amount_CK)
                            {
                                discount_amount_vcm -= (temp_discount - discount_amount_CK);
                            }
                        }
                    }

                    decimal OriginalPrice = item.old_price; // Math.Round((item.price_unit * (100 - nowFood)) / 100, 0);
                    
                    decimal TotalDiscount = discount_amount_vcm + discount + discountLoyalty;
                    //if (TotalDiscount > Math.Round(OriginalPrice * item.qty, 0))
                    //{
                    //    TotalDiscount = OriginalPrice * item.qty;
                    //}

                    decimal ActualAmountIncVAT = item.price_unit > 0 ? (item.price_unit * item.qty - TotalDiscount) : 0;
                    
                    decimal VatPercent = tax.vat_percent;

                    decimal NetPrice = Math.Round((ActualAmountIncVAT/(1 + VatPercent/100))/item.qty, 0);

                    decimal VatAmount = ActualAmountIncVAT - NetPrice * item.qty;

                    decimal CommissionsAmount = 0; // Math.Round(((OriginalPrice * item.qty) * nowFood) / 100, 0);

                    int lineParent = 0;
                    if (item.related_line_id > 0)
                    {
                        var transLineParent = transLinePLG.Where(x => x.fe_uid == item.related_line_id).FirstOrDefault();
                        lineParent = transLineParent != null ? transLineParent.id : 0;
                    }

                    if(lineParent != item.id && transLine.Count > 0)
                    {
                        lineParent = transLine.Where(x => x.LineId == lineParent).Select(x => x.LineNo).FirstOrDefault();
                    }

                    //Add transLine
                    transLine.Add(new TransLine()
                    {
                        OrderId = item.order_id,
                        LineId = item.id,
                        LineNo = lineNo, //item.id,
                        OrderNo = orderNo,
                        LineParent = lineParent,
                        LineName = item.is_topping_line == true ? LineTypeEnumPLG.TOPPING.ToString() : LineTypeEnumPLG.SALES.ToString(),
                        LineType = item.is_topping_line == true ? (int)LineTypeEnumPLG.TOPPING : (int)LineTypeEnumPLG.SALES,
                        ItemNo = product_tmpl.sap_code,
                        ItemName = product_tmpl.name,
                        Uom = product_tmpl.sap_uom,
                        UomVN = itemUom.name,
                        Quantity = item.qty,
                        OriginPrice = OriginalPrice,
                        UnitPrice = item.price_unit,
                        NetPrice = NetPrice,
                        DiscountPercent = (TotalDiscount > 0 && ActualAmountIncVAT > 0) ? (Math.Round(((TotalDiscount) * 100) / ActualAmountIncVAT, 0)) : 0,
                        DiscountAmount = TotalDiscount,
                        PercentPartner = discount_percent_vcm,
                        DiscountPartner = discount_amount_vcm,
                        CommissionsAmount = CommissionsAmount,
                        VATGroup = tax.vat_group,
                        VATPercent = VatPercent,
                        VATAmount = VatAmount, //Math.Round(item.price_subtotal_incl, 0) - Math.Round(item.price_subtotal, 0),
                        LineAmountExcVAT = ActualAmountIncVAT - VatAmount,   //Math.Round(item.price_subtotal, 0),
                        LineAmountIncVAT = ActualAmountIncVAT, // Math.Round(item.price_subtotal_incl, 0),
                        OdooDiscountAmount = discount + discountLoyalty, //tt giam gia tren odoo
                        OdooAmountExcVat = item.price_subtotal,
                        OdooAmountIncVAT = item.price_subtotal_incl,
                        LocationId = item.warehouse_id,
                        WarehouseId = item.warehouse_id,
                        DivisionCode = stock_warehouse.code,
                        CategoryCode = item.id.ToString(),
                        ProductGroupCode = "",
                        SerialNo = "",
                        VariantNo = VATConst.MappingProductSize()[product_tmpl.size_id != null ? (int)product_tmpl.size_id : 0].ToString(),
                        ItemType = item.product_id.ToString(),
                        OrderType = orderType,
                        LotNo = string.IsNullOrEmpty(des_loyalty) == false ? des_loyalty : "",
                        ComboId = string.IsNullOrEmpty(item.combo_id.ToString()) ? 0 : item.combo_id,
                        IsDoneCombo = item.is_done_combo,
                        ComboQty = string.IsNullOrEmpty(item.combo_qty) ? "" : item.combo_qty.ToString(),
                        ComboSeq = string.IsNullOrEmpty(item.combo_seq) ? "" : item.combo_seq.ToString(),
                        CupType = item.cup_type ?? "",
                        ExpireDate = item.date_order,
                        BlockedMemberPoint = true,
                        BlockedPromotion = true,
                        MemberPointsEarn = item.cv_life_earn,
                        MemberPointsRedeem = item.cv_life_redeem,
                        DeliveringMethod = 0,
                        ReturnedQuantity = 0,
                        DeliveryQuantity = 0,
                        DeliveryStatus = 0,
                        UpdateFlg = "N",
                        ChgeDate = DateTime.Now
                    }); ;

                    //cup
                    if (!string.IsNullOrEmpty(item.cup_type))
                    {
                        lineNo++;
                        transLine.Add(AddTransLineOptions(
                                item,
                                orderNo,
                                lineNo,
                                parentLineNo,
                                (int)LineTypeEnumPLG.CUP,
                                LineTypeEnumPLG.CUP.ToString(),
                                //product_tmpl != null ? product_tmpl.ref_code : item.product_id.ToString().PadLeft(8, '0'),
                                item.cup_type.ToString().ToUpper(),
                                product_tmpl.sap_code,
                                product_tmpl.name,
                                itemUom.name,
                                VATConst.MappingProductSize()[product_tmpl.size_id != null ? (int)product_tmpl.size_id : 0].ToString(),
                                tax.vat_group,
                                orderType,
                                item.qty
                            ));
                    }

                    //DiscountEntry
                    if (TotalDiscount > 0 || item.cv_life_earn > 0 || item.cv_life_redeem > 0)
                    {
                        var discountType = "PLD1";
                        string promotionHeader = string.Empty;
                        Sale_Promo_Header promotion = null;
                        if (item.promotion_id > 0)
                        {
                            promotion = _lst_Sale_Promo_Header.Where(x => x.id == item.promotion_id).FirstOrDefault();
                        }
                       
                        if(string.IsNullOrEmpty(promotionHeader) && discount_amount_vcm > 0)
                        {
                            promotionHeader = "HEAD";
                        }

                        if (item.loyalty_point_cost > 0 || item.is_loyalty_line == true)
                        {
                            promotionHeader = "LOYALTY";
                        }

                        if (TotalDiscount > 0)
                        {
                            discountLineNo++;
                            transDiscountEntry.Add(new TransDiscountEntry()
                            {
                                OrderNo = orderNo,
                                OrderId = item.order_id,
                                LineId = item.id,
                                LineNo = discountLineNo,
                                OrderLineNo = parentLineNo,
                                OfferNo = promotion != null ? promotion.id.ToString() : (!string.IsNullOrEmpty(promotionHeader) ? promotionHeader : "OTHER"),
                                OfferType = discountType,
                                Quantity = 1,
                                DiscountType = 0,
                                DiscountAmount = TotalDiscount,
                                ParentLineNo = parentLineNo,
                                ItemNo = product_tmpl != null ? product_tmpl.sap_code : "",
                                LineGroup = promotion != null ? promotion.id.ToString().ToUpper() : ""
                            }); ;
                        }

                        //Add CVLife
                        if (item.cv_life_earn > 0)
                        {
                            transDiscountEntry.AddRange(AddDiscountEntryCVLife(orderNo, product_tmpl.sap_code,item.order_id, item.id, discountLineNo, parentLineNo, item.cv_life_earn, 10, "EARN"));
                            discountLineNo += 2;
                        }
                        if (item.cv_life_redeem > 0)
                        {
                            transDiscountEntry.AddRange(AddDiscountEntryCVLife(orderNo, product_tmpl.sap_code, item.order_id, item.id, discountLineNo, parentLineNo, item.cv_life_redeem, 10, "REDEEM"));
                            discountLineNo += 2;
                        }
                    }
                }//end foreach

                //Phụ thu TẾT
                if (transHeaderPLG.total_surcharge > 0)
                {
                    int new_lineNo = lineNo + 1;
                    var first_item = transLinePLG.Where(x => x.amount_surcharge > 0).FirstOrDefault();
                    int vat_percent_surcharge = 8;
                    string vat_group_surcharge = "5";

                    if (transHeaderPLG.date_order.ToString("MM") == "01")
                    {
                        vat_percent_surcharge = 10;
                        vat_group_surcharge = "4";
                    }

                    transLine.Add(AddTransLineSurcharge(
                        first_item,
                        transHeaderPLG.total_surcharge,
                        orderNo,
                        new_lineNo,
                        new_lineNo,
                        (int)LineTypeEnumPLG.FEE,
                        LineTypeEnumPLG.FEE.ToString(),
                        "SURCHARGE", //itemNo
                        "", //item parent
                        "Phụ thu Tết",
                        "LAN",
                        1,
                        vat_percent_surcharge,
                        vat_group_surcharge,
                        orderType
                        ));
                }

                //FileHelper.WriteLogs(JsonConvert.SerializeObject(transDiscountEntry));
                transLine.ForEach(n => _dbContext.TransLine.Add(n));
                if (transDiscountEntry.Count > 0)
                {
                    transDiscountEntry.ForEach(n => _dbContext.TransDiscountEntry.Add(n));
                }
                
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
            if (transPayments.Count > 0)
            {
                List<TransPaymentEntry> payments = new List<TransPaymentEntry>();
                int lineNo = 0;
                foreach (var item in transPayments)
                {
                    var paymentMethod = _lstPayment_Method.Where(x => x.id == item.payment_method_id).FirstOrDefault();
                    if (string.IsNullOrEmpty(paymentMethod.sap_method))
                    {
                        FileHelper.WriteLogs(orderNo +  " || chưa mapping TenderType cho SAP-Odoo");
                        _loggingDb.LoggingToDB("INB-SALES", orderNo, paymentMethod.name + " || chưa mapping TenderType mã SAP");
                        return null;
                    }

                    lineNo++;
                    string referenceNo = string.Empty;
                    string payInfo = string.Empty;

                    if (!string.IsNullOrEmpty(item.voucher_code))
                    {
                        referenceNo = item.voucher_code.ToString();
                    }

                    if (!string.IsNullOrEmpty(item.on_account_info))
                    {
                        payInfo = item.on_account_info.ToString();
                        referenceNo = item.employee_id.ToString();
                    }

                    if (!string.IsNullOrEmpty(item.on_account_partner_id.ToString()))
                    {
                        referenceNo = item.on_account_partner_id.ToString();
                    }

                    payments.Add(new TransPaymentEntry()
                    {
                        PaymentTime = DateTime.Now,
                        PaymentDate = item.payment_date,
                        LineNo = lineNo,
                        LineId = item.id,
                        OrderId = item.pos_order_id,
                        WarehouseId = item.warehouse_id,
                        OrderNo = orderNo,
                        StoreNo = stock_warehouse.code,
                        PosNo = posNo,
                        ShiftNo = "",
                        StaffID = item.cashier_id.ToString(),
                        ExchangeRate = item.exchange_rate,
                        PaymentMethod = item.payment_method_id,
                        TenderType = paymentMethod.sap_method,
                        TenderTypeName = paymentMethod.name,
                        AmountTendered = item.amount,
                        CurrencyCode = "VND",
                        AmountInCurrency = item.amount,
                        ReferenceNo = referenceNo,
                        CardOrAccount = "",
                        PayForOrderNo = "",
                        ApprovalCode = "",
                        BankCardType = "",
                        BankPOSCode = "",
                        IsOnline = false,
                        PayInfo = payInfo
                    });
                }

               // FileHelper.WriteLogs(JsonConvert.SerializeObject(payments));
                payments.ForEach(n => _dbContext.TransPaymentEntry.Add(n));
                _dbContext.SaveChanges();
                return payments;
            }
            else
            {
                return null;
            }
        }
        private TransPaymentEntry AddTransPaymentEntry(List<TransPaymentEntry> savePaymentEntry, Stock_Warehouse stock_warehouse, int lineNo, string orderNo, string posNo,int paymentMethod, string tenderType, decimal paymentAmount, string description = "")
        {
            var payment = new TransPaymentEntry()
            {
                PaymentTime = DateTime.Now,
                PaymentDate = savePaymentEntry.FirstOrDefault().PaymentDate,
                LineNo = lineNo,
                WarehouseId = savePaymentEntry.FirstOrDefault().WarehouseId,
                OrderNo = orderNo,
                StoreNo = stock_warehouse.code,
                PosNo = posNo,
                ShiftNo = "",
                StaffID = savePaymentEntry.FirstOrDefault().StaffID,
                ExchangeRate = savePaymentEntry.FirstOrDefault().ExchangeRate,
                PaymentMethod = paymentMethod,
                TenderType = tenderType,
                TenderTypeName = description,
                AmountTendered = paymentAmount,
                CurrencyCode = "VND",
                AmountInCurrency = paymentAmount,
                ReferenceNo = orderNo,
                CardOrAccount = "",
                PayForOrderNo = "",
                ApprovalCode = "",
                BankCardType = "",
                BankPOSCode = "",
                IsOnline = false,
                PayInfo = description

            };
            _dbContext.TransPaymentEntry.Add(payment);
            _dbContext.SaveChanges();
            return payment;
        }
        private TransCancel SaveTransCancel(PosOrderCancelDto posOrderCancel)
        {
            if(posOrderCancel != null)
            {
                var checkTrans = _dbContext.TransHeader.Where(x => x.RefKey1 == posOrderCancel.name && x.RefKey2 == posOrderCancel.id.ToString()).FirstOrDefault();
                if(checkTrans != null)
                {
                    string orderNo = checkTrans.OrderNo;
                    var transCancel = new TransCancel()
                    {
                        Id = posOrderCancel.id,
                        Name = posOrderCancel.name,
                        RefNo = "9" + orderNo,
                        OrderNo = orderNo,
                        OrderDate = posOrderCancel.date_order,
                        State = posOrderCancel.state,
                        LocationId = posOrderCancel.location_id,
                        WarehouseId = posOrderCancel.warehouse_id,
                        DateLastOrder = posOrderCancel.date_last_order,
                        WriteDate = posOrderCancel.write_date,
                        Note = posOrderCancel.note,
                        NoteLabel = posOrderCancel.note_label,
                        CrtDate = DateTime.Now,
                        UpdateFlg = "N",
                        IsEOD = false
                    };
                    _dbContext.TransCancel.Add(transCancel);
                    _dbContext.SaveChanges();
                    return transCancel;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        private TransLine AddTransLineOptions(Pos_Order_Line item,string orderNo, int lineNo, int parentLine, int lineType, string lineName, string itemNo, string sap_code, string itemName, string uom, string size, string vatGroup, int orderType, decimal product_qty)
        {
            return new TransLine()
            {
                OrderId = item.order_id,
                LineId = item.id,
                LineNo = lineNo, //item.id,
                OrderNo = orderNo,
                LineParent = parentLine,
                LineName = lineName,
                LineType = lineType,
                ItemNo = itemNo,
                ItemName = itemName,
                Uom = StringHelper.MakeVNtoEN(uom).ToUpper(),
                UomVN = uom,
                Quantity = product_qty,
                OriginPrice = 0,
                UnitPrice = 0,
                DiscountPercent = 0,
                DiscountAmount = 0,
                PercentPartner = 0,
                DiscountPartner = 0,
                CommissionsAmount = 0,
                VATGroup = vatGroup,
                VATPercent = 0,
                VATAmount = 0, //Math.Round(item.price_subtotal_incl, 0) - Math.Round(item.price_subtotal, 0),
                LineAmountExcVAT = 0,   //Math.Round(item.price_subtotal, 0),
                LineAmountIncVAT = 0, // Math.Round(item.price_subtotal_incl, 0),
                OdooDiscountAmount = 0, //tt giam gia tren odoo
                OdooAmountExcVat = 0,
                OdooAmountIncVAT = 0,
                LocationId = item.warehouse_id,
                WarehouseId = item.warehouse_id,
                DivisionCode = "",
                CategoryCode = item.id.ToString(),
                ProductGroupCode = sap_code,
                SerialNo = "",
                VariantNo = size,
                ItemType = item.product_id.ToString(),
                OrderType = orderType,
                LotNo = "",
                ComboId = string.IsNullOrEmpty(item.combo_id.ToString()) ? 0 : item.combo_id,
                IsDoneCombo = item.is_done_combo,
                ComboQty = string.IsNullOrEmpty(item.combo_qty) ? "" : item.combo_qty.ToString(),
                ComboSeq = string.IsNullOrEmpty(item.combo_seq) ? "" : item.combo_seq.ToString(),
                CupType = "",
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
            };
        }
        private TransLine AddTransLineSurcharge(Pos_Order_Line item,decimal amount_surcharge, string orderNo, int lineNo, int parentLine, int lineType, string lineName, string itemNo, string sap_code, string itemName, string uom, decimal qty,
               decimal vat_percent, string vat_group, int orderType)
        {
            decimal unit_price = Math.Round(amount_surcharge, 0);
            decimal netPrice = Math.Round((unit_price * qty / (1 + vat_percent / 100)) / qty, 0);
            return new TransLine()
            {
                OrderId = item.order_id,
                LineId = item.id,
                LineNo = lineNo, //item.id,
                OrderNo = orderNo,
                LineParent = parentLine,
                LineName = lineName,
                LineType = lineType,
                ItemNo = itemNo,
                ItemName = itemName,
                Uom = StringHelper.MakeVNtoEN(uom).ToUpper(),
                UomVN = uom,
                Quantity = qty,
                OriginPrice = unit_price,
                UnitPrice = unit_price,
                NetPrice = netPrice,
                DiscountPercent = 0,
                DiscountAmount = 0,
                PercentPartner = 0,
                DiscountPartner = 0,
                CommissionsAmount = 0,
                VATGroup = vat_group,
                VATPercent = vat_percent,
                VATAmount = unit_price * qty - netPrice * qty, 
                LineAmountExcVAT = netPrice * qty,   //Math.Round(item.price_subtotal, 0),
                LineAmountIncVAT = unit_price * qty, // Math.Round(item.price_subtotal_incl, 0),
                OdooDiscountAmount = 0, //tt giam gia tren odoo
                OdooAmountExcVat = 0,
                OdooAmountIncVAT = 0,
                LocationId = 0,
                WarehouseId = 0,
                DivisionCode = "",
                CategoryCode = "",
                ProductGroupCode = "",
                SerialNo = "",
                VariantNo = "",
                ItemType = "",
                OrderType = orderType,
                LotNo = "",
                ComboId = 0,
                IsDoneCombo = false,
                ComboQty = "",
                ComboSeq = "",
                CupType = "",
                ExpireDate = DateTime.Now.Date,
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
            };
        }
        private void SaveTransInfoVat(PosRequestVatDto posRequestVatDto)
        {
            var checkTrans = _dbContext.TransHeader.Where(x => x.RefKey1 == posRequestVatDto.name && x.RefKey2 == posRequestVatDto.id.ToString()).FirstOrDefault();
            if (checkTrans != null)
            {
                checkTrans.IssuedVATInvoice = posRequestVatDto.invoice_request;
                _dbContext.TransHeader.Update(checkTrans);

                var checkTransVAT = _dbContext.TransInfoVAT.Where(x => x.RefNo == posRequestVatDto.name).FirstOrDefault();
                if(checkTransVAT == null)
                {
                    var transVat = new TransInfoVAT()
                    {
                        OrderNo = checkTrans.OrderNo,
                        RefNo = posRequestVatDto.name,
                        TaxID = posRequestVatDto.invoice_vat ?? "",
                        CustName = posRequestVatDto.invoice_name,
                        CompanyName = posRequestVatDto.invoice_name,
                        Address = posRequestVatDto.invoice_address,
                        Email = posRequestVatDto.invoice_email??"",
                        Phone = posRequestVatDto.invoice_contact??"",
                        Note = posRequestVatDto.invoice_note??"",
                        WriteDate = posRequestVatDto.create_date,
                        CrtDate = DateTime.Now,
                        InsertDate = DateTime.Now
                    };
                    _dbContext.TransInfoVAT.Add(transVat);
                }
                else
                {
                    checkTransVAT.TaxID = posRequestVatDto.invoice_vat;
                    checkTransVAT.CustName = posRequestVatDto.invoice_name;
                    checkTransVAT.CompanyName = posRequestVatDto.invoice_name;
                    checkTransVAT.Address = posRequestVatDto.invoice_address;
                    checkTransVAT.Email = posRequestVatDto.invoice_email??"";
                    checkTransVAT.Phone = posRequestVatDto.invoice_contact??"";
                    checkTransVAT.Note = posRequestVatDto.invoice_note??"";
                    checkTransVAT.WriteDate = posRequestVatDto.create_date;
                    checkTransVAT.CrtDate = DateTime.Now;
                    _dbContext.TransInfoVAT.Update(checkTransVAT);
                }
                _dbContext.SaveChanges();
            }
        }
        private List<TransDiscountEntry> AddDiscountEntryCVLife(string orderNo, string itemNo, int orderId, int lineId, int lineNo, int orderLineNo, decimal point, decimal vatPercent, string cv_type = "EARN")
        {
            List<TransDiscountEntry> transDiscountEntries = new List<TransDiscountEntry>();
            decimal netPrice = Math.Round(point / (1 + vatPercent / 100), 0);

            if (lineNo == 0)
            {
                lineNo = 1;
            }
            else
            {
                lineNo++;
            }
            transDiscountEntries.Add(new TransDiscountEntry()
            {
                OrderNo = orderNo,
                OrderId = orderId,
                LineId = lineId,
                LineNo = lineNo,
                OrderLineNo = orderLineNo,
                OfferNo = "Revenue_CVLife",
                OfferType = cv_type == "EARN" ? "PLD3" : "PLD5",
                Quantity = point,
                DiscountType = 0,
                DiscountAmount = netPrice,
                ParentLineNo = orderLineNo,
                ItemNo = itemNo,
                LineGroup = cv_type
            });
            transDiscountEntries.Add(new TransDiscountEntry()
            {
                OrderNo = orderNo,
                OrderId = orderId,
                LineId = lineId,
                LineNo = lineNo + 1,
                OrderLineNo = orderLineNo,
                OfferNo = "VAT_CVLife",
                OfferType = cv_type == "EARN" ? "PLD4" : "PLD6",
                Quantity = point,
                DiscountType = 0,
                DiscountAmount = point - netPrice,
                ParentLineNo = orderLineNo,
                ItemNo = itemNo,
                LineGroup = cv_type
            });
            return transDiscountEntries;
        }
        private List<TransLineOptions> SaveTransLineOptions(List<Pos_Order_Line_Option> transLineOptions, string orderNo)
        {
            if (transLineOptions.Count > 0)
            {
                int lineNo = 0;
                List<TransLineOptions> transOptions = new List<TransLineOptions>();
                foreach (var item in transLineOptions)
                {
                    lineNo++;
                    transOptions.Add(new TransLineOptions()
                    {
                        OrderNo = orderNo,
                        LineNo = lineNo,
                        OrderLineNo = item.line_id,
                        OptionId = item.option_id,
                        OptionType = item.option_type.ToString() ?? "",
                        OptionsName = item.name.ToString() ?? "",
                        ProductId = item.product_id,
                        ProductMaterialId = item.product_material_id,
                        ProductUomId = item.product_uom_id,
                        UomVN = _lstUom_Uom.Where(x => x.id == item.product_uom_id).FirstOrDefault().name,
                        ProductQty = item.product_qty,
                        Uom = StringHelper.MakeVNtoEN(_lstUom_Uom.Where(x => x.id == item.product_uom_id).FirstOrDefault().name).ToUpper()
                    });
                }
                transOptions.ForEach(n => _dbContext.TransLineOptions.Add(n));
                _dbContext.SaveChanges();
                return transOptions;
            }
            else
            {
                return null;
            }
        }

    }
}
