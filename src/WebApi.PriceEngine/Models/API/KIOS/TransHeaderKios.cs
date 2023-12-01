using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VCM.Shared.Dtos.PhucLong;

namespace WebApi.PriceEngine.Models.API.KIOS
{
    public class TransHeaderKios
    {
        [Required]
        [StringLength(10)]
        [DefaultValue("WIN_LIFE")]
        public string AppCode { get; set; }
        [Required]
        [StringLength(20)]
        public string OrderNo { get; set; }
        [Required]
        [StringLength(8)]
        public string OrderDate { get; set; }
        [Required]
        [DefaultValue("2001")]
        public string StoreNo { get; set; } = String.Empty;
        public string PosNo { get; set; } = String.Empty;
        public string CustName { get; set; } = String.Empty;
        public string CustAddress { get; set; } = String.Empty;
        [RegularExpression(@"^\$?\d+(\.(\d{2}))?$")]
        public string CustPhone { get; set; } = String.Empty;
        public string Note { get; set; } = String.Empty;
        [Range(0, 999)]
        [DefaultValue(0)]
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public int StepProcess { get; set; }
        public string MemberCardNumber { get; set; }
        public decimal MemberPointsEarn { get; set; }
        public decimal MemberPointsRedeem { get; set; }
        public List<TransLineKios> Items { get; set; }
        public List<OrderPaymentDto> Payments { get; set; }
        public List<MembershipOfferKIOS> MembershipOffer { get; set; }
    }

    public class MembershipOfferKIOS
    {
        public string OrderNo { get; set; }
        public int LineId { get; set; }
        public string CardNumber { get; set; }
        public string OfferNo { get; set; }
        public string OfferType { get; set; }
        public decimal DiscountAmount { get; set; }
        public string ItemNo { get; set; } = String.Empty;
        public decimal Quantity { get; set; }
        public string Note { get; set; } = String.Empty;
    }
}
