using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.PhucLong
{
    public class OrderPaymentDto
    {
        public int LineId { get; set; }
        public string PaymentMethod { get; set; }
        public string CurrencyCode { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal AmountTendered { get; set; }
        public decimal AmountInCurrency { get; set; }
        public string TransactionNo { get; set; }

    }
}
