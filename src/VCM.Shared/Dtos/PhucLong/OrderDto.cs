using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.PhucLong
{
    public class OrderDto
    {
        public string AppCode { get; set; }
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public string StoreNo { get; set; }
        public string CustName { get; set; }
        public string CustAddress { get; set; }
        public string CustPhone { get; set; }
        public string Note { get; set; }
        public int Status { get; set; }
        public int SaleTypeId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public string MemberCardNumber { get; set; }
        public bool HasVatInvoice { get; set; }
        public BillingInfo BillingInfo { get; set; }
        public List<Coupons> UseCoupon { get; set; }
    }
    public class Coupons
    {
        public string CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public string Note { get; set; }
    }
    public class BillingInfo
    {
        public string TaxID { get; set; }
        public string CustName { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }
    }
}
