using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PhucLong.Interface.Central.Models.OCC
{
    [Table("OCC_TransPaymentEntry")]
    public class OCCTransPaymentEntry
    {
        public string OrderNo { get; set; }
        public int LineNo { get; set; }
        public string StoreNo { get; set; }
        public string PosNo { get; set; }
        public string TenderType { get; set; }
        public decimal ExchangeRate { get; set; }
        public string CurrencyCode { get; set; }
        public decimal AmountTendered { get; set; }
        public decimal AmountInCurrency { get; set; }
        public DateTime PaymentDate { get; set; }
        public string ReferenceNo { get; set; }
        public string TransactionNo { get; set; }
        public string ApprovalCode { get; set; }
        public string TraceCode { get; set; }
    }
}
