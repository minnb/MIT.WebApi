using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API
{
    public class ResponseOrderList
    {
        public string PartnerCode { get; set; }
        public string StoreNo { get; set; }
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public string CustName { get; set; }
        public string CustPhone { get; set; }
        public string CustAddress { get; set; }
        public int TotalItem { get; set; }
        public decimal PaymentAmount { get; set; }
        public string CashierId { get; set; }
        public string CashierName { get; set; }
        public int Status { get; set; }
        public object Remark { get; set; }
    }
}
