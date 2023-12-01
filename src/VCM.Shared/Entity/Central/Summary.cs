using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.Central
{
    public class Summary
    {
        public string Code { get; set; }
        public string EntryDate { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public int TotalBill { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string UpdateFlg { get; set; }
        public DateTime CrtDate { get; set; }
        public Guid Id { get; set; }
    }
}
