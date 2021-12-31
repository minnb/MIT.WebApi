using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Inbound
{
    public class E1BPLINEITEMTAX:INB_HEADER
    {
        public int RETAILSEQUENCENUMBER { get; set; }
        public int TAXSEQUENCENUMBER { get; set; }
        public string TAXTYPECODE { get; set; }
        public decimal TAXAMOUNT { get; set; }
    }
}
