using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Partner.ESTELLA
{
    public class Sales_ESTELLA
    {
        public int MachineID { get; set; }
        public int BatchID { get; set; }
        public string Date { get; set; }
        public string Hour { get; set; }
        public int ReceiptCount { get; set; }
        public decimal GTO { get; set; }
        public decimal GST { get; set; }
        public decimal Discount { get; set; }
        public decimal ServiceCharge { get; set; }
        public int NoOfPax  { get; set; }
        public decimal Cash { get; set; }
        public decimal DebitCard { get; set; }
        public decimal VisaCard { get; set; }
        public decimal MasterCard { get; set; }
        public decimal Amex { get; set; }
        public decimal Voucher { get; set; }
        public decimal OthersAmount { get; set; }
        public string Registered { get; set; }
        public string UpdateFlg { get; set; }
        public string OrderDate { get; set; }
    }
}
