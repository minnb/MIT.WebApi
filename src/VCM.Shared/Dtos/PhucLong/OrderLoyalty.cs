using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.PhucLong
{
    public class OrderLoyalty
    {
        public int LineId { get; set; }
        public string MemberCardNumber { get; set; }
        public string ClubCode { get; set; }
        public decimal LoyaltyPointsEarn { get; set; }
        public decimal LoyaltyPointsRedeem { get; set; }
    }

    public class MembershipCard : OrderLoyalty
    {
        public string CustName { get; set; }
        public string CardClass { get; set; }
        public decimal TotalPoints { get; set; }
        public decimal AvailabilityPoints { get; set; }
    }
}