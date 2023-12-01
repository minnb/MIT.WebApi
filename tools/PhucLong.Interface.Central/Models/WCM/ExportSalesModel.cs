using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.WCM
{
    public class ExportSalesModel
    {
        public DateTime OrderDate { get; set; }
        public string StoreNo { get; set; }
        public string StoreName { get; set; }
        public string OrderNo { get; set; }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal LineAmount { get; set; }
    }
}
