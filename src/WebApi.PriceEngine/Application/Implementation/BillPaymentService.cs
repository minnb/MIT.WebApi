using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using VCM.Common.Database;
using VCM.Common.Helpers;
using VCM.Shared.API;
using VCM.Shared.Dtos.PhucLong;
using WebApi.PriceEngine.Application.Interfaces;
using Dapper;
using WebApi.PriceEngine.Models.API.KIOS;
using VCM.Shared.Dtos.BLUEPOS;
using WebApi.PriceEngine.Helpers;

namespace WebApi.PriceEngine.Application.Implementation
{
    public class BillPaymentService : IBillPaymentService
    {
        private readonly ILogger<BillPaymentService> _logger;
        private readonly IRedisCacheService _redisCacheService;

        public BillPaymentService(
              ILogger<BillPaymentService> logger,
              IRedisCacheService redisCacheService
          )
        {
            _logger = logger;
            _redisCacheService = redisCacheService;
        }

        public async Task<ResponseClient> OrderDetailKIOS(string UserName, string AppCode, string OrderNo)
        {
            try
            {
                var storeSet = _redisCacheService.GetDataSysStoreSetAsync().Result;

                string function = PriceEngineConfigHelper.GetFunctionSetting(storeSet, OrderNo);

                var sysConfig = _redisCacheService.GetDataSysConfigAsync().Result?.Where(x => x.AppCode == AppCode && x.Name.ToUpper() == function.ToUpper()).FirstOrDefault();

                if (sysConfig != null)
                {
                    DapperContext dapperDbContext = new DapperContext(sysConfig.Prefix);
                    using var conn = dapperDbContext.CreateConnDB;
                    conn.Open();
                    string procedure = string.Empty;

                    var transHeader = await conn.QueryAsync<TransHeaderKios>(@"EXEC SP_API_ORDER_CHECK " + OrderNo.ToString()).ConfigureAwait(false);
                    var dataHeader = transHeader?.FirstOrDefault();
                    if (dataHeader != null)
                    {
                        if(dataHeader.StepProcess == 1)
                        {
                            return ResponseHelper.RspNotWarning(201, "Đơn hàng " + OrderNo + " đã được thanh toán");
                        }
                        //else if (dataHeader.StepProcess == 0 && dataHeader.OrderDate != DateTime.Now.ToString("yyyyMMdd"))
                        //{
                        //    return ResponseHelper.RspNotWarning(201, "Đơn hàng " + OrderNo + " quá hạn thanh toán");
                        //}
                        else
                        {
                            decimal discountAmount = 0;
                            decimal totalAmount = 0;
                            decimal paymentAmount = 0;
                            var transLineKios = conn.Query<TransLineKios>(@"EXEC SP_API_ORDER_LINE " + OrderNo.ToString()).ToList();
                            var transLineOption = conn.Query<TransOptionEntryKios>(@"EXEC SP_API_ORDER_LINE_OPTION " + OrderNo.ToString()).ToList();
                            var transDiscountEntry = conn.Query<TransDiscountEntryKios>(@"EXEC SP_API_ORDER_DISCOUNT_ENTRY " + OrderNo.ToString()).ToList();
                            var transPayments = conn.Query<OrderPaymentDto>(@"EXEC SP_API_ORDER_PAYMENT " + OrderNo.ToString()).ToList();
                            var transMemberOffer = conn.Query<MembershipOfferKIOS>(@"EXEC SP_API_ORDER_MEMBER_OFFER " + OrderNo.ToString()).ToList();

                            discountAmount = transLineKios.Where(x => x.LineType == 0).Sum(x => x.DiscountAmount);
                            totalAmount = transLineKios.Where(x=>x.LineType == 0).Sum(x => x.LineAmountIncVAT);
                            paymentAmount = transPayments.Sum(x=>x.AmountTendered);

                            var data = new TransHeaderKios()
                            {
                                AppCode = PriceEngineConfigHelper.GetAppCode(OrderNo).ToUpper(),
                                OrderDate = dataHeader.OrderDate,
                                OrderNo = dataHeader.OrderNo,
                                PosNo = dataHeader.PosNo,
                                StoreNo = dataHeader.StoreNo,
                                CustName = dataHeader.CustName,
                                CustAddress = dataHeader.CustAddress,
                                CustPhone = dataHeader.CustPhone,
                                Note = dataHeader.Note,
                                DiscountAmount = discountAmount,
                                TotalAmount = totalAmount,
                                PaymentAmount = paymentAmount,
                                StepProcess = dataHeader.StepProcess,
                                MemberCardNumber = dataHeader.MemberCardNumber,
                                MemberPointsEarn = dataHeader.MemberPointsEarn,
                                MemberPointsRedeem = dataHeader.MemberPointsRedeem,
                                Items = MappingTransLineKIOS(transLineKios, transLineOption, transDiscountEntry),
                                Payments = transPayments.Where(x => x.PaymentMethod != "KIOS").ToList(),
                                MembershipOffer = transMemberOffer.ToList()
                            };

                            return ResponseHelper.RspOK(data);
                        }
                    }
                    else
                    {
                        return ResponseHelper.RspNotWarning(404, "Không tìm thấy đơn hàng " + OrderNo);
                    }
                }
                else
                {
                    return ResponseHelper.RspNotWarning(404, "Chưa cấu hình hệ thống, vui lòng liên hệ IT");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("OrderDetailKIOS.Exception: " + ex.Message.ToString());
                return ResponseHelper.RspNotWarning(404, ExceptionHelper.ExptionMessage(ex));
            }
        }
        private List<TransLineKios> MappingTransLineKIOS(List<TransLineKios> transLines, List<TransOptionEntryKios> transLineOption, List<TransDiscountEntryKios> transDiscountEntry)
        {
            try
            {
                List<TransLineKios> transLineKios = new List<TransLineKios>();
                string[] cupType = { "PLASTIC", "PAPER" };
                foreach(var item in transLines)
                {
                    List<TransOptionEntryKios> transOptions = new List<TransOptionEntryKios>();
                    List<TransDiscountEntryKios> transDiscount = new List<TransDiscountEntryKios>();
                    int i = 0;
                    int j = 0;
                    var dataOptions = transLineOption.Where(x => x.OrderNo == item.OrderNo && x.ParentLineId == item.LineId).ToList();
                    if(dataOptions.Count > 0)
                    {
                        foreach(var o  in dataOptions)
                        {
                            i++;
                            transOptions.Add(new TransOptionEntryKios()
                            {
                                OrderNo = item.OrderNo,
                                LineId = i,
                                ParentLineId = o.ParentLineId,
                                Type = o.Type,
                                ItemNo = o.ItemNo,
                                OptionName = o.OptionName??"",
                                OptionType = o.OptionType??"",
                                Uom = o.Uom,
                                Qty = o.Qty,
                                Size = o.Size
                            });
                        }
                    }
                    var dataDiscount = transDiscountEntry.Where(x => x.OrderNo == item.OrderNo && x.ParentLineId == item.LineId).ToList();
                    if (dataDiscount.Count > 0)
                    {
                        foreach (var d in dataDiscount)
                        {
                            j++;
                            transDiscount.Add(new TransDiscountEntryKios()
                            {
                                OrderNo = item.OrderNo,
                                LineId = j,
                                ParentLineId = d.ParentLineId,
                                OfferNo = d.OfferNo,
                                OfferType = d.OfferType,
                                DiscountAmount = d.DiscountAmount,
                                Quantity = d.Quantity,
                                Note = d.Note
                            });
                        }
                    }

                    string size = transOptions.Count > 0 ? (transOptions.Where(x => cupType.Contains(x.OptionType)).FirstOrDefault()?.Size) : "";
                    string cup = transOptions.Count > 0 ? (transOptions.Where(x => cupType.Contains(x.OptionType)).FirstOrDefault()?.OptionType) : "";
                    transLineKios.Add(new TransLineKios()
                    {
                        OrderNo = item.OrderNo,
                        LineId = item.LineId,
                        LineType = item.LineType,
                        ParentLineId = item.ParentLineId,
                        ItemNo = item.ItemNo,
                        ItemName = item.ItemName,
                        Barcode = item.Barcode,
                        Uom = item.Uom,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity,
                        DiscountAmount = item.DiscountAmount,
                        TaxGroupCode = item.TaxGroupCode,
                        VatPercent = item.VatPercent,
                        VatAmount = item.VatAmount,
                        IsTopping = item.IsTopping,
                        CupType = cup??"",
                        Size = size??"",
                        Note = item.Note??"",
                        OptionEntry = transOptions.Count > 0 ? transOptions.ToList() : null,
                        DiscountEntry = transDiscount.Count > 0 ? transDiscount.ToList() : null
                    });
                    transOptions.Clear();
                    transDiscount.Clear();
                }
                return transLineKios;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> MappingTransLineKIOS.Exception: " + ex.Message.ToString());
                return null;
            }
        }
        public async Task<ResponseClient> UpdatePaymentKIOS(string UserName, string AppCode, TransPaymentKios Payments)
        {
            try
            {
                var storeSet = _redisCacheService.GetDataSysStoreSetAsync().Result;

                string function = PriceEngineConfigHelper.GetFunctionSetting(storeSet, Payments.OrderNo);

                var sysConfig = _redisCacheService.GetDataSysConfigAsync().Result?.Where(x => x.AppCode == AppCode && x.Name.ToUpper() == function.ToUpper()).FirstOrDefault();

                if (sysConfig != null)
                {
                    DapperContext dapperDbContext = new DapperContext(sysConfig.Prefix);
                    using var conn = dapperDbContext.CreateConnDB;
                    conn.Open();
                    string procedure = string.Empty;
                    var transHeader = await conn.QueryAsync<TransHeaderKios>(@"EXEC SP_API_ORDER_CHECK "  + Payments.OrderNo.ToString()).ConfigureAwait(false);
                    var dataHeader = transHeader?.FirstOrDefault();
                    if (dataHeader != null)
                    {
                        if (dataHeader.StepProcess == 0)
                        {
                            decimal paymentAmountKIOS = Payments.Payments.Sum(x => x.AmountTendered);

                            var transLineKios = conn.Query<TransLineKios>(@"EXEC SP_API_ORDER_LINE " + Payments.OrderNo.ToString()).ToList();
                            var transPayments = conn.Query<OrderPaymentDto>(@"EXEC SP_API_ORDER_PAYMENT " + Payments.OrderNo.ToString()).ToList();

                            decimal mustPaymentAmount = (transLineKios.Sum(x => x.LineAmountIncVAT) - transPayments.Sum(x => x.AmountTendered));

                            if (paymentAmountKIOS != mustPaymentAmount)
                            {
                                return ResponseHelper.RspNotWarning(201, "Số tiền thanh toán " + paymentAmountKIOS.ToString() + " không khớp " + mustPaymentAmount.ToString());
                            }

                            decimal loyaltyPointsEarn = 0;
                            decimal loyaltyPointsRedeem = 0;
                            if (Payments.Loyalty != null && Payments.Loyalty.Count > 0)
                            {
                                loyaltyPointsEarn = Payments.Loyalty.Sum(x => x.LoyaltyPointsEarn);
                                loyaltyPointsRedeem = Payments.Loyalty.Sum(x => x.LoyaltyPointsRedeem);
                                DistributionPointLoyalty(transLineKios, loyaltyPointsEarn, loyaltyPointsRedeem);
                            }

                            using var transaction = conn.BeginTransaction();
                            try
                            {
                                List<TransPaymentEntryDto> transPaymentEntryDtos = new List<TransPaymentEntryDto>();
                                foreach(var item in Payments.Payments)
                                {
                                    string BankCardType = "";
                                    string ApprovalCode = "";
                                    if (!string.IsNullOrEmpty(item.ApprovalCode))
                                    {
                                        BankCardType = "BANK_POS";
                                        ApprovalCode = item.ApprovalCode;
                                    }
                                    else if (!string.IsNullOrEmpty(item.TraceCode))
                                    {
                                        BankCardType = "QRCode";
                                        ApprovalCode = item.TraceCode;
                                    }
                                    transPaymentEntryDtos.Add(new TransPaymentEntryDto()
                                    {
                                        LineNo = item.LineId * 1001,
                                        OrderNo = Payments.OrderNo,
                                        TransactionNo = "",
                                        ReceiptNo = "",
                                        StatementCode = "",
                                        CardNo = "",
                                        ExchangeRate = item.ExchangeRate,
                                        TenderType = item.PaymentMethod,
                                        TenderTypeName = "",
                                        AmountTendered = item.AmountTendered,
                                        CurrencyCode = item.CurrencyCode,
                                        AmountInCurrency = item.AmountInCurrency,
                                        CardOrAccount = "",
                                        PaymentDate = StringHelper.ConvertStringToDate(Payments.PaymentDate),
                                        PaymentTime = DateTime.Now,
                                        ShiftNo = "1",
                                        ShiftDate = DateTime.Now.Date,
                                        StaffID = "KIOS",
                                        CardPaymentType = 0,
                                        CardValue = 0,
                                        ReferenceNo = item.TransactionNo,
                                        PayForOrderNo = Payments.OrderNo,
                                        Counter = 0,
                                        ApprovalCode = ApprovalCode,
                                        BankPOSCode = item.ApprovalCode,
                                        BankCardType = BankCardType,
                                        IsOnline = true,
                                        LastUpdated = DateTime.Now
                                    });
                                }

                                string insQuery = @"INSERT INTO dbo.TransPaymentEntry
                                                (OrderNo,[LineNo],StoreNo,POSTerminalNo,TransactionNo,ReceiptNo,StatementCode,CardNo,ExchangeRate,TenderType,TenderTypeName,AmountTendered,CurrencyCode
                                                ,AmountInCurrency,CardOrAccount,PaymentDate,PaymentTime,ShiftNo,ShiftDate,StaffID,CardPaymentType,CardValue,ReferenceNo,PayForOrderNo,Counter,ApprovalCode
                                                ,BankPOSCode,BankCardType,IsOnline,LastUpdated) "
                                          + " VALUES (@OrderNo,@LineNo,@StoreNo,@POSTerminalNo,@TransactionNo,@ReceiptNo,@StatementCode,@CardNo,@ExchangeRate,@TenderType,@TenderTypeName,@AmountTendered,@CurrencyCode "
                                          + "      , @AmountInCurrency, @CardOrAccount, @PaymentDate, @PaymentTime, @ShiftNo, @ShiftDate, @StaffID, @CardPaymentType, @CardValue, @ReferenceNo, @PayForOrderNo, @Counter, @ApprovalCode "
                                          + "      , @BankPOSCode, @BankCardType, @IsOnline, @LastUpdated) ";

                                string updateQuery = @"Update dbo.TransHeader SET StepProcess = 1, HouseNo = 'CX' WHERE OrderNo = @OrderNo";
                                await conn.ExecuteAsync(updateQuery, Payments, transaction);
                                await conn.ExecuteAsync(insQuery, transPaymentEntryDtos, transaction);
                                
                                if(loyaltyPointsEarn > 0 || loyaltyPointsRedeem > 0)
                                {
                                    string queryUpdate = @"UPDATE dbo.TransLine SET MemberPointsEarn = @MemberPointsEarn, MemberPointsRedeem = @MemberPointsRedeem "
                                                        + " WHERE DocumentNo = @OrderNo AND [LineNo] = @LineId; ";
                                    await conn.ExecuteAsync(queryUpdate, transLineKios, transaction);
                                }

                                transaction.Commit();
                                return ResponseHelper.RspNotWarning(200, "Cập nhật thanh toán thành công đơn hàng " + Payments.OrderNo);
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                _logger.LogWarning("UpdatePaymentKIOS.BeginTransaction.Rollback: " + ex.Message.ToString());
                                return ResponseHelper.RspNotWarning(404, "Lỗi cập nhật thanh toán đơn hàng " + Payments.OrderNo);
                            }
                        }
                        else
                        {
                            return ResponseHelper.RspNotWarning(201, "Đơn hàng " + Payments.OrderNo + " đã được thanh toán");
                        }
                    }
                    else
                    {
                        return ResponseHelper.RspNotWarning(404, "Không tìm thấy đơn hàng " + Payments.OrderNo);
                    }
                }
                else
                {
                    return ResponseHelper.RspNotWarning(404, "Chưa cấu hình hệ thống, vui lòng liên hệ IT");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("UpdatePaymentKIOS.Exception: " + ex.Message.ToString());
                return ResponseHelper.RspNotWarning(404, ExceptionHelper.ExptionMessage(ex));
            }
        }
        private void DistributionPointLoyalty(List<TransLineKios> transLine, decimal loyaltyPointsEarn, decimal loyaltyPointsRedeem)
        {
            try
            {
                decimal totalAmount = transLine.Sum(x => x.LineAmountIncVAT);
                decimal totalEarn = 0;
                decimal totalRedeem = 0;
                int checkLine = 0;
                int tatalRows = transLine.Where(x => x.LineAmountIncVAT > 0).ToList().Count;
                foreach (var item in transLine)
                {
                    if(item.LineAmountIncVAT > 0)
                    {
                        var percent = Math.Round((item.LineAmountIncVAT * 100)/totalAmount, 2);
                        item.MemberPointsEarn = Math.Round((loyaltyPointsEarn * percent) / 100, 0);
                        item.MemberPointsRedeem = Math.Round((loyaltyPointsRedeem * percent) / 100, 0);
                        totalEarn += item.MemberPointsEarn;
                        totalRedeem += item.MemberPointsRedeem;
                        checkLine++;

                        if (checkLine == tatalRows)
                        {
                            if (totalEarn != loyaltyPointsEarn)
                            {
                                item.MemberPointsEarn -= (totalEarn - loyaltyPointsEarn);
                            }
                            if (totalRedeem != loyaltyPointsRedeem)
                            {
                                item.MemberPointsRedeem -= (totalRedeem - loyaltyPointsRedeem);
                            }
                        }
                    }
                   
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning("DistributionPointLoyalty.Exception: " + ex.Message.ToString());
            }
        }
    }
}
