using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API
{
    public class ResponseCheckOrder
    {
        public string AppCode { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string StoreNo { get; set; }
        public string StoreName { get; set; }
        public int SalesTypeId { get; set; }
        public string SalesTypeName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string MemberCardNumber { get; set; }
        public decimal LoyaltyPointsEarn { get; set; }
        public decimal LoyaltyPointsRedeem { get; set; }
        public string Status { get; set; }
        public DateTime CrtDate { get; set; }
    }
}
