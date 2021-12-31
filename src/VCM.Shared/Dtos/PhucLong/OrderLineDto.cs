using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.PhucLong
{
    public class OrderLineDto
    {
        public int LineId { get; set; }
        public int ParentLineId { get; set; }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string Uom { get; set; }
        public decimal OldPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Qty { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal LineAmount { get; set; }
        public string VatGroup { get; set; }
        public int VatPercent { get; set; }
        public string Note { get; set; }
        public bool IsLoyalty { get; set; }
        public decimal LoyaltyPointsEarn { get; set; }
        public decimal LoyaltyPointsRedeem { get; set; }
        public string CupType { get; set; }
        public bool IsTopping { get; set; }
        public bool IsCombo { get; set; }
        public int ComboId { get; set; }

        public List<OrderLineOptionDto> OptionEntry { get; set; }
        public List<OrderDiscount> DiscountEntry { get; set; }
    }
}
