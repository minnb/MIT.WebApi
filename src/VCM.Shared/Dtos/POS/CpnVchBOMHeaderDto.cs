using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.POS
{
    public class CpnVchBOMLineDto
    {
        public string ItemNo { get; set; }
        public string LineItemNo { get; set; }
    }
    public class CpnVchBOMHeaderDto
    {
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string UnitOfMeasure { get; set; }
        public int DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal MinAmount { get; set; }
        public decimal ValueOfVoucher { get; set; }
        public string ArticleType { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime EndingDate { get; set; }
        public bool Blocked { get; set; }
        public int LimitQty { get; set; }
        public string CpnVchType { get; set; }
    }
}
