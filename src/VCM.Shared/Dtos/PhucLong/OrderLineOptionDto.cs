using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.PhucLong
{
    public class OrderLineOptionDto
    {
        public int LineId { get; set; }
        public string OptionType { get; set; }
        public string ItemNo { get; set; }
        public string Uom { get; set; }
        public decimal Qty { get; set; }
    }
}
