using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Inbound
{
    public class E1BPTRANSACTIONLOYAL:INB_HEADER
    {
        public int LOYALTYSEQUENCENUMBER { get; set; }
        public string CUSTOMERCARDNUMBER { get; set; }
    }
}
