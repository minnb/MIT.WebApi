using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Inbound
{
    public class E1BPLINEITEMDISCOUNT:INB_HEADER
    {
        public int RETAILSEQUENCENUMBER { get; set; }
        public int DISCOUNTSEQUENCENUMBER { get; set; }
        public string DISCOUNTTYPECODE { get; set; }
        //public string DISCOUNTREASONCODE { get; set; }
        public decimal REDUCTIONAMOUNT { get; set; }
        public string DISCOUNTID { get; set; }
        public string BONUSBUYID { get; set; }
        public string OFFERID { get; set; }
    }
}
