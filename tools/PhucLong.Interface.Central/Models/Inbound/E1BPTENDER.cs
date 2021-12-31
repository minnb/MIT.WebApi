using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Inbound
{
    public class E1BPTENDER:INB_HEADER
    {
        public int TENDERSEQUENCENUMBER { get; set; }
        public string TENDERTYPECODE { get; set; }
        public string TAXTYPECODE { get; set; }
        public decimal TENDERAMOUNT { get; set; }
        public string TENDERCURRENCY { get; set; }
        public string TENDERCURRENCY_ISO { get; set; }
        public string REFERENCEID { get; set; }
    }
}
