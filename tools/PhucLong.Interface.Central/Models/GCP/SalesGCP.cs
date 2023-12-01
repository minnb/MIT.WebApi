using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.GCP
{
    public class SalesGCP: SalesGCPHeader
    {
        public List<SalesGCPDetail> TransLine { set; get; }
        public List<SalesGCPPayment> TransPaymentEntry { set; get; }
    }
    public class SalesGCPHeader
    {
        public string StoreNo { set; get; }
        public string OrderNo { set; get; }
        public string OrderDate { set; get; }
        public string SaleType { set; get; }
        public string TransactionType { set; get; }
        public string MemberCardNo { set; get; }
        public string SalesStoreNo { set; get; }
        public string SalesPosNo { set; get; }
        public string RefNo { set; get; }
    }
    public class SalesGCPDetail
    {
        public int LineNo { set; get; }
        public int ParentLineNo { set; get; }
        public string ItemNo { set; get; }
        public string ItemName { set; get; }
        public string Uom { set; get; }
        public decimal Quantity { set; get; }
        public decimal UnitPrice { set; get; }
        public decimal DiscountAmount { set; get; }
        public decimal VATPercent { set; get; }
        public decimal LineAmount { set; get; }
        public decimal MemberPointsEarn { set; get; }
        public decimal MemberPointsRedeem { set; get; }
        public string CupType { set; get; }
        public string Size { set; get; }
        public bool IsTopping { set; get; }
        public bool IsCombo { set; get; }
        public DateTime ScanTime { set; get; }
        public List<SalesGCPDiscount> TransDiscountEntry { get; set; }
    }
    public class SalesGCPPayment
    {
        public int LineNo { set; get; }
        public string TenderType { set; get; }
        public string CurrencyCode { set; get; }
        public decimal ExchangeRate { set; get; }
        public decimal PaymentAmount { set; get; }
        public string ReferenceNo { set; get; }
    }
    public class SalesGCPDiscount
    {
        public int LineNo { set; get; }
        public int OrderLineNo { set; get; }
        public string OfferNo { set; get; }
        public string OfferType { set; get; }
        public decimal Quantity { set; get; }
        public decimal DiscountAmount { set; get; }
        public string Description { set; get; }
    }

    public class TempSalesGCP
    {
        public string SalesType { set; get; }
        public string OrderNo { set; get; }
        public DateTime OrderDate { set; get; }
        public DateTime CrtDate { set; get; }
        public string Batch { set; get; }
    }
}
