using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.BLUEPOS
{
    public class TransHeader
    {
        public string OrderNo { get; set; }
        public string Channel { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerNo { get; set; }
        public string CustomerName { get; set; }
        public string StoreNo { get; set; }
        public string POSTerminalNo { get; set; }
        public string CashierID { get; set; }
        public string MemberCardNo { get; set; }
        public string PhoneNumber { get { return MemberCardNo; } }
        public string RefKey1 { get; set; }
        public int TransactionType { get; set; }
        public bool IsFullReturn { get; set; }
        public string ReturnedOrderNo { get; set; }
        public List<TransLine> TransLines { get; set; }
        public List<TransPaymentEntry> TransPaymentEntry { get; set; }
    }
}
