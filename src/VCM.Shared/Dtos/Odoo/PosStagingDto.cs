using System.Collections.Generic;

namespace VCM.Shared.Entity.PhucLong
{
    public class PosStagingDto
    {
        public Pos_Order Pos_Order { get; set; }
        public List<Pos_Order_Line> Pos_Order_Line { get; set; }
        public List<Pos_Payment> Pos_Payment { get; set; }
    }
}
