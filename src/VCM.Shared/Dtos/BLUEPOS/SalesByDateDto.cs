using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.BLUEPOS
{
    public class SalesByDateDto
    {
        public string PnL {  get; set; }
        public string OrderDate { get; set; }
        public decimal AmountExclVAT { get; set; }
        public decimal AmountIncVAT { get; set; }
    }
}
