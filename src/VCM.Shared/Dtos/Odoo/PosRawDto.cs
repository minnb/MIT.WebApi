using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.PhucLong
{
    public class PosRawDto
    {
        public Pos_Order TransHeader { get; set; }
        public List<Pos_Order_Line> TransLine { get; set; }
        public List<Pos_Payment> TransPaymentEntry { get; set; }
        public List<Pos_Order_Line_Option> PosOrderLineOption { get; set; }
    }
}
