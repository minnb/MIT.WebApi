using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Queries
{
    public static class SalesGCPQuery
    {
        public static string SalesDetailQuery()
        {
            return @"SELECT [OrderNo],[LineType],[LineNo],[LineNo2],[ParentLineNo],[ItemNo],[ItemNo2]
                    ,[ItemName],[Uom],[Quantity],[OriginPrice],[UnitPrice],[NetPrice],[DiscountAmount]
                    ,[VATPercent],[VATAmount],[LineAmountExcVAT],[LineAmountIncVAT],[OriginLineAmountIncVAT]
                    ,[RevenueShareRatio],[VATGroup],[MemberPointsEarn],[MemberPointsRedeem],[CupType]
                    ,[Size],[IsTopping],[IsCombo],[ScanTime],[UpdateFlg],[CrtDate]
                    FROM [dbo].[OCC_TransLine] NOLOCK WHERE [OrderNo] IN @orderNo;";
        }
        public static string SalesPaymentQuery()
        {
            return @"SELECT [OrderNo],[LineNo],[StoreNo],[PosNo],[TenderType],[CurrencyCode]
                    ,[ExchangeRate],[AmountTendered],[AmountInCurrency],[PaymentDate],[ReferenceNo]
                      FROM [dbo].[OCC_TransPaymentEntry] NOLOCK WHERE [OrderNo] IN @orderNo;";
        }

        public static string TransLineQuery()
        {
            return @"SELECT DocumentNo [OrderNo], [LineNo] LineId, [LineType] ParentLineId, ItemNo, [Description] ItemName, [UnitOfMeasure] Uom, [UnitPrice] OldPrice,[UnitPrice] UnitPrice, [Quantity] Qty, DiscountAmount,[LineAmountIncVAT] LineAmount, 
		                [VATCode] VatGroup, [VATPercent] VatPercent, Note, '' CupType, '' Size, ISNULL(IsTopping, 0) IsTopping, IsCombo, 0 ComboId, ArticleType, Barcode, BlockedMemberPoint IsLoyalty
                        FROM CentralSales.dbo.TransLine (NOLOCK) WHERE DocumentNo IN @documentNo AND LineType IN (0, 1) ";
        }
        public static string TransLineQueryArchive()
        {
            return @"SELECT DocumentNo [OrderNo], [LineNo] LineId, [LineType] ParentLineId, ItemNo, [Description] ItemName, [UnitOfMeasure] Uom, [UnitPrice] OldPrice,[UnitPrice] UnitPrice, [Quantity] Qty, DiscountAmount,[LineAmountIncVAT] LineAmount, 
		                [VATCode] VatGroup, [VATPercent] VatPercent, Note, '' CupType, '' Size, ISNULL(IsTopping, 0) IsTopping, IsCombo, 0 ComboId, ArticleType, Barcode, BlockedMemberPoint IsLoyalty
                        FROM CentralSalesArchive.dbo.TransLine (NOLOCK) WHERE DocumentNo IN @documentNo AND LineType IN (0, 1) ";
        }
        public static string TransPaymentEntryQuery()
        {
            return @"SELECT [OrderNo], [LineNo] LineId, [TenderType] PaymentMethod, CurrencyCode, ExchangeRate, AmountTendered, AmountInCurrency, TransactionNo
                        FROM CentralSales.dbo.TransPaymentEntry (NOLOCK) WHERE OrderNo IN @orderNo";
        }
        public static string TransPaymentEntryQueryArchive()
        {
            return @"SELECT [OrderNo], [LineNo] LineId, [TenderType] PaymentMethod, CurrencyCode, ExchangeRate, AmountTendered, AmountInCurrency, TransactionNo
                        FROM CentralSalesArchive.dbo.TransPaymentEntry (NOLOCK) WHERE OrderNo IN @orderNo";
        }
        public static string TransDiscountEntryQuery()
        {
            return @"SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,[OfferNo] PromotionNo,[OfferType] PromotionType,[Quantity] Qty, DiscountAmount,[LineGroup] Note
                     FROM CentralSales.dbo.[TransDiscountEntry] NOLOCK WHERE OrderNo IN @orderNo
                     UNION
                     SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,[OfferNo] PromotionNo,[OfferType] PromotionType,[Quantity] Qty, DiscountAmount,[LineGroup] Note
                     FROM CentralSales.dbo.TransDiscountCouponEntry NOLOCK 
                     WHERE OrderNo IN @orderNo AND OfferType IN ('FamilyDay')";
        }
        public static string TransDiscountEntryQueryArchive()
        {
            return @"SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,[OfferNo] PromotionNo,[OfferType] PromotionType,[Quantity] Qty, DiscountAmount,[LineGroup] Note
                     FROM CentralSalesArchive.dbo.[TransDiscountEntry] NOLOCK WHERE OrderNo IN @orderNo
                     UNION
                     SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,[OfferNo] PromotionNo,[OfferType] PromotionType,[Quantity] Qty, DiscountAmount,[LineGroup] Note
                     FROM CentralSalesArchive.dbo.TransDiscountCouponEntry NOLOCK 
                     WHERE OrderNo IN @orderNo AND OfferType IN ('FamilyDay')";
        }
        public static string TransDiscountEntryQuery2()
        {
            return @"SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,[OfferNo],[OfferType],[Quantity], DiscountAmount,[LineGroup] Note
                     FROM CentralSales.dbo.[TransDiscountEntry] NOLOCK WHERE OrderNo IN @orderNo
                     UNION
                     SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,[OfferNo],[OfferType],[Quantity], DiscountAmount,[LineGroup] Note
                     FROM CentralSales.dbo.TransDiscountCouponEntry NOLOCK 
                     WHERE OrderNo IN @orderNo AND OfferType IN ('FamilyDay')";
        }
        public static string TransDiscountEntryQuery2Archive()
        {
            return @"SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,[OfferNo],[OfferType],[Quantity], DiscountAmount,[LineGroup] Note
                     FROM CentralSalesArchive.dbo.[TransDiscountEntry] NOLOCK WHERE OrderNo IN @orderNo
                     UNION
                     SELECT [OrderNo], [LineNo] LineId,[OrderLineNo] ParentLineId,[OfferNo],[OfferType],[Quantity], DiscountAmount,[LineGroup] Note
                     FROM CentralSalesArchive.dbo.TransDiscountCouponEntry NOLOCK 
                     WHERE OrderNo IN @orderNo AND OfferType IN ('FamilyDay')";
        }
        public static string TransPointLineQuery()
        {
            return @"SELECT [OrderNo],[LineNo]  LineId, [OrderLineNo] ParentLineId, [MemberNumber] MemberCardNumber, ClubCode,[EarnPoints] LoyaltyPointsEarn,ISNULL([RedeemPoints], 0) LoyaltyPointsRedeem
                    FROM CentralSales.dbo.[TransPointLine] NOLOCK WHERE OrderNo IN @orderNo";
        }
        public static string TransPointLineQueryArchive()
        {
            return @"SELECT [OrderNo],[LineNo]  LineId, [OrderLineNo] ParentLineId, [MemberNumber] MemberCardNumber, ClubCode,[EarnPoints] LoyaltyPointsEarn,ISNULL([RedeemPoints], 0) LoyaltyPointsRedeem
                    FROM CentralSalesArchive.dbo.[TransPointLine] NOLOCK WHERE OrderNo IN @orderNo";
        }
    }
}
