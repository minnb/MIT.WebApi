using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.PhucLong
{
    public class OrderLineOptionDto
    {
        public int LineId { get; set; }
        public string ItemNo { get; set; }
        public string OptionType { get; set; }
        public string OptionName { get; set; } 
        public string Uom { get; set; }
        public decimal Qty { get; set; }
    }
    public class RspOrderLineOptionDto
    {
        public int LineId { get; set; }
        public string Type { get; set; }
        public string ItemNo { get; set; }
        public string Uom { get; set; }
        public string OptionType { get; set; } = String.Empty;
        public string OptionName { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public decimal Qty { get; set; }
        public string Note { get; set; }
        public string ItemNoRef { get; set; }
    }
}
