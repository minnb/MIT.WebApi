using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.BLUEPOS
{
    public class TransLine
    {
        public string DocumentNo { get; set; }
        public string OrderNo { get { return DocumentNo; } }
        public int LineNo { get; set; }
        public int LineType { get; set; }
        public string ItemNo { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal VATPercent { get; set; }
        public decimal VATAmount { get; set; }
        public decimal LineAmountIncVAT { get; set; }
        public string OfferNo { get; set; }
        public string StaffID { get; set; }
        public string VATCode { get; set; }
        public string Barcode { get; set; }
        public string DivisionCode { get; set; }
        public string SerialNo { get; set; }
    }
}
