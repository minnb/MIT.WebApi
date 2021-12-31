using System;
using System.Collections.Generic;
using System.Text;

namespace ShopInShop.Interface.Models.Central
{
    public class TransLineCentral
    {
        public int ID { get; set; }
        public int LineNo { get; set; }
        public string DocumentNo { get; set; }
        public string ItemNo { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal VATPercent { get; set; }
        public decimal VATAmount { get; set; }
        public decimal LineAmountIncVAT { get; set; }
        public string VATCode { get; set; }
        public decimal MemberPointsEarn { get; set; }
        public decimal MemberPointsRedeem { get; set; }
        public decimal AmountCalPoint { get; set; }
        public decimal LineAmountIncVATOrig { get; set; }
        public DateTime CreatedDate { get; set; }
        public string OrigTransPos { get; set; }
        public string Barcode { get; set; }
        public string OfferNo { get; set; }
        public DateTime ScanTime { get; set; }
        public int SnGLineID { get; set; }
        public bool IsSendSAP { get; set; }
        public bool IsIssueVAT { get; set; }
    }
}
