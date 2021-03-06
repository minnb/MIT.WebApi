using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Shared.Dtos.POS
{
    public class TransLineDto
    {
        public int LineNo { get; set; }
        public int ParentLineNo { get; set; }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string ItemName2 { get; set; }
        public string Barcode { get; set; }
        public string Uom { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Qty { get; set; }
        public decimal DiscountAmount { get; set; }
        public string VatGroup { get; set; }
        public decimal VatPercent { get; set; }
        public decimal VatAmount { get; set; }
        public decimal LineAmountExcVAT { get; set; }
        public decimal LineAmountIncVAT { get; set; }
        public bool IsLoyalty { get; set; }
        public string ItemType { get; set; }
        public Remark Remark { get; set; }
        public List<TransDiscountEntryDto> TransDiscountEntry { get;set;}
    }
    public class Remark
    {
        public string Remark1 { get; set; }
        public string Remark2 { get; set; }
        public string Remark3 { get; set; }
        public string Remark4 { get; set; }
        public string Remark5 { get; set; }
        public string Remark6 { get; set; }
        public string Remark7 { get; set; }
        public string Remark8 { get; set; }
        public string Remark9 { get; set; }
        public string Remark10 { get; set; }
    }
}
