using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Tools.Interface.Dtos.VinIDDto
{
    public class TopUpVinidReconciliationDto
    {
        public int Stt { get; set; }
        public string Store { get; set; }
        public string Terminal { get; set; }
        public string InvoiceNo { get; set; }
        public string PhoneNumber { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public int TransactionType { get; set; }
        public DateTime TransactionTime { get; set; }
        public int Status { get; set; }
    }

    [Table("VINID_TopUpRecon")]
    public class VINID_TopUpRecon: TopUpVinidReconciliationDto
    {
        public string UpdateFlg { get; set; }
        public string File_Name { get; set; }
        public DateTime CrtDate { get; set; }
    }
}

