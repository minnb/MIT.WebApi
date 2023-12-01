using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.OCC
{
    public class StagingTransLine
    {
        public string StoreNo { get; set; }
        public string StoreNo2 { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderNo { get; set; }
        public string PosNo { get; set; }
        public int OrderType { get; set; }
        public string TransactionType { get; set; }
        public int LineNo { get; set; }
        public int LineParent { get; set; }
        public int LineType { get; set; }
        public string ItemNo { get; set; }
        public string ItemNo2 { get; set; }
        public string ItemName { get; set; }
        public string Uom { get; set; }
        public string UomVN { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal LineAmountExcVAT { get; set; }
        public decimal LineAmountIncVAT { get; set; }
        public string VATGroup { get; set; }
        public string VATPercent { get; set; }
        public decimal VATAmount { get; set; }
        public string CupType { get; set; }
        public decimal MemberPointsEarn { get; set; }
        public decimal MemberPointsRedeem { get; set; }
        public string MemberCardNo { get; set; }
        public string Size { get; set; }
        public bool IsTopping { get; set; }
        public bool IsCombo { get; set; }
        public string RefNo { get; set; }
        public string UpdateFlg { get; set; }
        public DateTime CrtDate { get; set; }

    }
}
