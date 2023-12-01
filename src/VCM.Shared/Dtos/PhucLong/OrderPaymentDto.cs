using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace VCM.Shared.Dtos.PhucLong
{
    public class OrderPaymentDto
    {
        [Required]
        public int LineId { get; set; }
        [Required]
        //[StringRange(AllowableValues = new[] { "", "C000", "ZCRE", "ZTPA", "TQRP", "ZNAP" }, ErrorMessage = "Hình thức thanh toán không đúng")]
        public string PaymentMethod { get; set; }
        [Required]
        public string CurrencyCode { get; set; }
        [Required]
        public decimal ExchangeRate { get; set; }
        [Required]
        [Column(TypeName = "decimal(18, 3)")]
        public decimal AmountTendered { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal AmountInCurrency { get; set; }
        public string TransactionNo { get; set; }
        public string ApprovalCode { get; set; }
        public string TraceCode { get; set; }
        public string ReferenceId { get; set; }
    }
}
