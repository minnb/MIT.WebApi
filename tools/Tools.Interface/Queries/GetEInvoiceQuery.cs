using System;
using System.Collections.Generic;
using System.Text;

namespace ShopInShop.Interface.Queries
{
    public static class GetEInvoiceQuery
    {
        public static string GetInvoiceCreatedQuery()
        {
            return @"SELECT [SiteNo] as StoreNo, [OrderNo],[CustomerName],[CompanyName],[TaxCode],[PhoneNumber],[Email],[Address]
                     FROM [dbo].[InvoiceCreated] (NOLOCK)
                     WHERE [IsNotVAT] = 1 AND [OrderNo] IN @OrderNo;";
        }
    }
}
