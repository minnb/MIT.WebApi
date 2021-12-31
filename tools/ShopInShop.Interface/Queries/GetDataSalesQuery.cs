namespace ShopInShop.Interface.Queries
{
    public static class GetDataSalesQuery
    {
        public static string GetTransHeaderQuery(string appCode, string type, string orderDate, int top)
        {
            return @"SELECT TOP "+ top  + @" H.[ID],H.[OrderNo],H.[StoreNo],H.[OrderDate],H.[CompCode],H.[Type],H.[CreatedDate], OrderNoOrig
                    FROM [dbo].[TransHeaderV2] (NOLOCK) H
                    WHERE NOT EXISTS (SELECT OrderNo FROM [dbo].TransRaw (NOLOCK) T WHERE H.OrderNo = T.OrderNo AND H.StoreNo = T.StoreNo)
	                      AND UPPER(H.[CompCode]) = '" + appCode + @"' AND H.Type = '" + type + @"'
                          AND H.OrderDate < CAST(getdate() AS DATE) AND H.OrderDate >= '" + orderDate + @"'; ";
        }

        public static string GetTransLineQuery()
        {
            return @"SELECT [ID],[LineNo],[DocumentNo],[ItemNo],[Description],[UnitOfMeasure],[Quantity],[UnitPrice],[DiscountAmount],[VATPercent],[VATAmount],[LineAmountIncVAT]
		                    ,[VATCode],[MemberPointsEarn],[MemberPointsRedeem],[AmountCalPoint],[LineAmountIncVATOrig],[CreatedDate],[OrigTransPos],[Barcode],[OfferNo],[ScanTime]
		                    ,[SnGLineID],[IsSendSAP],[IsIssueVAT]
                    FROM [dbo].[TransLineV2] (NOLOCK) 
                    WHERE [DocumentNo] IN @OrderNo ;";
        }
        public static string GetTransPaymentEntryQuery()
        {
            return @"SELECT [OrderNo],[LineNo],[StoreNo],[CardNo],[TenderType],[TenderTypeName],[AmountTendered],[CurrencyCode],[AmountInCurrency],[CardOrAccount]
	                    ,[PaymentDate],[PaymentTime],[ShiftNo],[ShiftDate],[StaffID],[CardPaymentType],[CardValue],[ReferenceNo],[PayForOrderNo],[ApprovalCode]
	                    ,[BankPOSCode],[BankCardType],[IsOnline],[CreatedDate],[StatementCode]
                    FROM [dbo].[TransPaymentEntryV2] (NOLOCK) 
                    WHERE [OrderNo] IN @OrderNo ;";
        }
    }
}
