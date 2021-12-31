using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Central
{
    [Table("TransLine")]
    public class TransLine
    {
        public string OrderNo { get; set; }
        public int OrderId { get; set; }
        public int LineId { get; set; }
        public int LineNo { get; set; }
        public int LineParent { get; set; }
        public int LineType { get; set; }
        public string LineName { get; set; }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string Uom { get; set; }
        public string UomVN { get; set; }
        public decimal Quantity { get; set; }
        public decimal OriginPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal NetPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PercentPartner { get; set; }
        public decimal DiscountPartner { get; set; }
        public decimal CommissionsAmount { get; set; }
        public decimal OdooDiscountAmount { get; set; }
        public decimal OdooAmountExcVat { get; set; }
        public decimal OdooAmountIncVAT { get; set; }
        public string VATGroup { get; set; }
        public decimal VATPercent { get; set; }
        public decimal VATAmount { get; set; }
        public decimal LineAmountExcVAT { get; set; }
        public decimal LineAmountIncVAT { get; set; }
        public int LocationId { get; set; }
        public int WarehouseId { get; set; }
        public string DivisionCode { get; set; }
        public string CategoryCode { get; set; }
        public string ProductGroupCode { get; set; }
        public string VariantNo { get; set; }
        public string SerialNo { get; set; }
        public string ItemType { get; set; }
        public bool BlockedPromotion { get; set; }
        public bool BlockedMemberPoint { get; set; }
        public decimal MemberPointsEarn { get; set; }
        public decimal MemberPointsRedeem { get; set; }
        public int DeliveringMethod { get; set; }
        public int DeliveryStatus { get; set; }
        public decimal DeliveryQuantity { get; set; }
        public decimal ReturnedQuantity { get; set; }
        public int OrderType { get; set; }
        public int ComboId { get; set; }
        public bool IsDoneCombo { get; set; }
        public string ComboSeq { get; set; }
        public string ComboQty { get; set; }
        public string LotNo { get; set; }
        public string CupType { get; set; }
        public DateTime ExpireDate { get; set; }
        public string UpdateFlg { get; set; }
        public DateTime ChgeDate { get; set; }
    }
}
