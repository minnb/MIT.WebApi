using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PhucLong.Interface.Central.Models.OCC
{
    [Table("OCC_TransLine")]
    public class OCCTransLine
    {
        public string OrderNo { get; set; }
        public int LineType { get; set; }
        public int LineNo { get; set; }
        public int LineNo2 { get; set; }
        public int ParentLineNo { get; set; }
        public string ItemNo { get; set; }
        public string ItemNo2 { get; set; }
        public string ItemName { get; set; }
        public string Uom { get; set; }
        public decimal Quantity { get; set; }
        public decimal OriginPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal NetPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal VATPercent { get; set; }
        public decimal VATAmount { get; set; }
        public decimal LineAmountExcVAT { get; set; }
        public decimal LineAmountIncVAT { get; set; }
        public decimal OriginLineAmountIncVAT { get; set; }
        public decimal RevenueShareRatio { get; set; }
        public decimal CommissionsAmount { get; set; }
        public string VATGroup { get; set; }
        public decimal MemberPointsEarn { get; set; }
        public decimal MemberPointsRedeem { get; set; }
        public string CupType { get; set; }
        public string Size { get; set; }
        public bool IsTopping { get; set; }
        public bool IsCombo { get; set; }
        public DateTime ScanTime { get; set; }
        public string UpdateFlg { get; set; }
        public DateTime CrtDate { get; set; }
    }
}
