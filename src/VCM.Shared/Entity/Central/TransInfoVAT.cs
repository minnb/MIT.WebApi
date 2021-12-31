using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Central
{
    [Table("TransInfoVAT")]
    public class TransInfoVAT
    {
        public string OrderNo { get; set; }
        public string RefNo { get; set; }
        public string TaxID { get; set; }
        public string CustName { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }
        public DateTime CrtDate { get; set; }
        public DateTime WriteDate { get; set; }
        public DateTime InsertDate { get; set; }
    }
}
