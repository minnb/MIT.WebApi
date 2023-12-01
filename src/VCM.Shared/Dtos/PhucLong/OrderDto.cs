using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VCM.Shared.Const;

namespace VCM.Shared.Dtos.PhucLong
{
    public class RspOrderDto: OrderDto
    {
        //[Required]
        //[StringLength(3)]
        //[DefaultValue("PLH")]
        //public string PartnerCode { get; set; }
    }
    public class OrderDto
    {
        [Required]
        [StringLength(10)]
        [DefaultValue("PLH")]
        public string AppCode { get; set; } = "PLH";
        public string PartnerCode
        {
            get
            {
                return AppPartnerPLHConst.GetPartnerCode(AppCode);
            }
        }

        [Required]
        [StringLength(20)]
        public string OrderNo { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        [Required]
        [DefaultValue("2001")]
        public string StoreNo { get; set; } = String.Empty;
        public string CustName { get; set; } = String.Empty;
        public string CustAddress { get; set; } = String.Empty;
        [RegularExpression(@"^\$?\d+(\.(\d{2}))?$")]
        public string CustPhone { get; set; } = String.Empty;
        public string Note { get; set; } = String.Empty;
        [Range(0, 999)]
        [DefaultValue(0)]
        public int Status { get; set; }
        [Range(10, 99)]
        [DefaultValue(0)]
        public int SaleTypeId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public bool HasVatInvoice { get; set; }
        public bool IsLoyalty { get; set; }
        public string WinCode { get; set; }
        public BillingInfo BillingInfo { get; set; }
        public List<Coupons> UseCoupon { get; set; }
        public ShippingInfo ShippingInfo { get; set; }
        public string Serial { get; set; }
    }
    public class Coupons
    {
        [Required]
        public string CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public string Note { get; set; } = String.Empty;
    }
    public class BillingInfo
    {
        public string TaxID { get; set; }
        public string CustName { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; } = String.Empty;
        public string Note { get; set; } = String.Empty;
    }
    public class ShippingInfo
    {
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverAddress { get; set; }
    }
}
