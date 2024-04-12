using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.WinMoney
{
    public class WinMoneyTransDto
    {
        public string RequestID { get; set; }
        public string RequestTime { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public decimal Total { get; set; }
        public string Method { get; set; }
        public string MerchantCode { get; set; }
        public string StoreID { get; set; }
        public string CashierID { get; set; }
        public string CustomerName { get; set; }
        public string PhoneCustomer { get; set; }
        public string BankNumber { get; set; }
        public string UpdateFlg { get; set; }
        public string FileName { get; set; }
        public DateTime CrtDate { get; set; }
    }
}
