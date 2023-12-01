using System;
using System.Collections.Generic;
using System.Text;

namespace ShopInShop.Interface.Models.Central
{
    public class TransPaymentEntryCentral
    {
        public string OrderNo { get; set; }
        public int LineNo { get; set; }
        public string StoreNo { get; set; }
        public string CardNo { get; set; }
        public string TenderType { get; set; }
        public string TenderTypeName { get; set; }
        public decimal AmountTendered { get; set; }
        public string CurrencyCode { get; set; }
        public decimal AmountInCurrency { get; set; }
        public string CardOrAccount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PaymentTime { get; set; }
        public string ShiftNo { get; set; }
        public DateTime ShiftDate { get; set; }
        public string StaffID { get; set; }
        public int CardPaymentType { get; set; }
        public decimal CardValue { get; set; }
        public string ReferenceNo { get; set; }
        public string PayForOrderNo { get; set; }
        public string ApprovalCode { get; set; }
        public string BankPOSCode { get; set; }
        public string BankCardType { get; set; }
        public bool IsOnline { get; set; }
        public DateTime CreatedDate { get; set; }
        public string StatementCode { get; set; }
    }
}
