using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Common.Models.CRX_PLH
{
    public class SyncSalesPrice
    {
        public string StoreId { get; set; }
        public string ArticleId { get; set; }
        public string ArticleName { get; set; }
        public decimal SoldPrice { get; set; }
        public string UnitCode { get; set; }
        public string ProductBarcode { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
}
