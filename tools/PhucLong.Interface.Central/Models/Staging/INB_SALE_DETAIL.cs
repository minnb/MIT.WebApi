using PhucLong.Interface.Central.Models.Inbound;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Staging
{
    public class INB_SALE_DETAIL: E1BPRETAILLINEITEM
    {
        public int TAXSEQUENCENUMBER { get; set; }
        public string TAXTYPECODE { get; set; }
        public decimal TAXAMOUNT { get; set; }
        public string FIELDGROUP { get; set; }
        public string FIELDNAME { get; set; }
        public string FIELDVALUE { get; set; }
        public string UPDATE_FLG { get; set; }
    }
}
