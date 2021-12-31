using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Central
{
    [Table("TransPaymentEntry")]
    public class TransPaymentEntry
    {
        public string OrderNo { get; set; }
        public int OrderId { get; set; }
        public int LineId { get; set; }
        public int LineNo { get; set; }
        public int WarehouseId { get; set; }
        public string StoreNo { get; set; }
        public string PosNo { get; set; }
        public decimal ExchangeRate { get; set; }
        public int PaymentMethod { get; set; }
        public string TenderType { get; set; }
        public string TenderTypeName { get; set; }
        public decimal AmountTendered { get; set; }
        public string CurrencyCode { get; set; }
        public decimal AmountInCurrency { get; set; }
        public string CardOrAccount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PaymentTime { get; set; }
        public string ShiftNo { get; set; }
        public string StaffID { get; set; }
        public string ReferenceNo { get; set; }
        public string PayForOrderNo { get; set; }
        public string ApprovalCode { get; set; }
        public string BankPOSCode { get; set; }
        public string BankCardType { get; set; }
        public bool IsOnline { get; set; }
        public string PayInfo { get; set; }
    }
}
