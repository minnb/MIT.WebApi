using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.BLUEPOS
{
    public class TransPaymentEntry
    {
        public string OrderNo { get; set; }
        public int LineNo { get; set; }
        public decimal ExchangeRate { get; set; }
        public string CurrencyCode { get; set; }
        public string TenderType { get; set; }
        public string TenderTypeName { get; set; }
        public decimal AmountInCurrency { get; set; }
        public decimal AmountTendered { get; set; }
        public DateTime PaymentTime { get; set; }
        public string ReferenceNo { get; set; }
    }
}
