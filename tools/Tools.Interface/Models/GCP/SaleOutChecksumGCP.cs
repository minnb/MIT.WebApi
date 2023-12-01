using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Interface.Models.GCP
{
    public class SaleOutChecksumGCP
    {
        public string AppCode { get; set; }
        public string CalendarDay { get; set; }
        public int TotalReceiptCount { get; set; }
        public decimal TotalReceiptAmount { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public string UpdateFlg { get; set; }
    }
}
