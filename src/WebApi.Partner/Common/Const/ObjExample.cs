using System;
using System.Collections.Generic;
using System.Linq;
using VCM.Common.Helpers;
using VCM.Shared.Dtos.POS;
using VCM.Shared.Entity.Partner;
using VCM.Shared.Partner;

namespace VCM.Partner.API.Common.Const
{
    public static class ObjExample
    {
        public static TransLineDto MappingTransLine(Item item, int lineNo, int parentLineNo, InfoItemPartner infoItemPartner, Remark remark, List<TransDiscountEntryDto> discountEntry = null)
        {
            TransLineDto transLine = new TransLineDto();
            if (item!= null)
            {
                decimal discount = 0;
                if(discountEntry != null && discountEntry.Count > 0)
                {
                    discount = discountEntry.Sum(x => x.DiscountAmount);
                }

                decimal netAmount = MathHelper.CalcNetAmount(infoItemPartner.LineAmountInclVAT, VatConst.MappingTax()[item.VatGroup]);
                transLine = new TransLineDto()
                {
                    LineNo = lineNo,
                    ParentLineNo = parentLineNo,
                    ItemNo = item.ItemNo,
                    ItemName = item.ItemName,
                    ItemName2 = infoItemPartner.ItemName ?? item.ItemName,
                    Barcode = item.Barcode,
                    Uom = item.Uom,
                    UnitPrice = infoItemPartner.UnitPrice,
                    Qty = infoItemPartner.Qty == 0 ? 1 : infoItemPartner.Qty,
                    DiscountAmount = discount,
                    VatGroup = item.VatGroup,
                    VatPercent = VatConst.MappingTax()[item.VatGroup],
                    VatAmount = infoItemPartner.LineAmountInclVAT - netAmount,
                    LineAmountExcVAT = netAmount,
                    LineAmountIncVAT = infoItemPartner.LineAmountInclVAT,
                    IsLoyalty = infoItemPartner.IsLoyalty,
                    ItemType = item.ItemType,
                    Remark = remark,
                    TransDiscountEntry = discountEntry
                };
            }
            
            return transLine;
        }
        public static TransHeaderDto GetTransExample()
        {
            List<TransDiscountEntryDto> discountEntries = new List<TransDiscountEntryDto>
            {
                new TransDiscountEntryDto()
                {
                    LineNo = 4484454,
                    ParentLineNo = 4484454,
                    OfferNo = "115362",
                    OfferType = "Loyalty",
                    DiscountAmount = 6750,
                    Qty = 1,
                    Note = "Discount Loyalty",
                }
            };

            List<TransLineDto> transLine = new List<TransLineDto>
            {
                new TransLineDto()
                {
                    LineNo = 4484454,
                    ItemNo = "10002581",
                    ItemName = "[DMTRA082] Lychee Tea -Jasmine (L)",
                    Barcode = "",
                    Uom = "LY",
                    UnitPrice = 45000,
                    Qty = 1,
                    DiscountAmount = 6750,
                    VatGroup = "1",
                    VatPercent = 10,
                    VatAmount = 3825,
                    LineAmountExcVAT = 34425,
                    LineAmountIncVAT = 38250,
                    Remark = null,
                    TransDiscountEntry = discountEntries
                }
            };

            List<TransPaymentEntryDto> transPayments = new List<TransPaymentEntryDto>
            {
                new TransPaymentEntryDto()
                {
                    LineNo = 4484454,
                    TenderType = "PCTS",
                    PaymentAmount = 2000,
                    ReferenceNo = "LY",
                    TransactionId = "2193165",
                    AdditionalData = null
                }
            };
            return new TransHeaderDto()
            {
                AppCode = "PLG",
                OrderNo = "CH0101-18682-001-0001",
                OrderTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                DeliveryType = 1,
                CustNo = "",
                CustName = "",
                CustPhone = "",
                CustAddress = "",
                CustNote = "",
                CardMember = "",
                TotalAmount = 38250,
                PaymentAmount = 36250,
                Status = 0,
                RefNo = "2129577",
                TransLine = transLine,
                TransPaymentEntry = transPayments
            };
        }
    }
}
