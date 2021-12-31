using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCM.Shared.Partner
{
    public class InfoDto
    {
        public int CrtUser { get; set; }
        public DateTime CrtDate { get; set; }
        public int ChgeUser { get; set; }
        public DateTime ChgeDate { get; set; }
    }

    public class InfoItemPartner
    {
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Qty { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal LineAmountInclVAT { get; set; }
        public bool IsLoyalty { get; set; }
    }
}
