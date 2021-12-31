using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.Central
{
    public class MappingChannel
    {
        public string AppCode { get; set; }
        public int SaleTypeId { get; set; }
        public string OrderChannel { get; set; }
        public string Description { get; set; }
        public bool Blocked { get; set; }
        public bool IsDiscount { get; set; }
        public decimal DiscountPercent { get; set; }
        public string TenderType { get; set; }
    }
}
