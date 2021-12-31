using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Inbound
{
    public class E1BPRETAILLINEITEM:INB_HEADER
    {
        public int RETAILSEQUENCENUMBER { get; set; }
        public string RETAILTYPECODE { get; set; }
        public string ITEMIDQUALIFIER { get; set; }
        public string ITEMID { get; set; }
        public decimal RETAILQUANTITY { get; set; }
        public string SALESUNITOFMEASURE { get; set; }
        public string SALESUNITOFMEASURE_ISO { get; set; }
        public decimal SALESAMOUNT { get; set; }
        public decimal NORMALSALESAMOUNT { get; set; }
        public decimal COST { get; set; }
        public string BATCHID { get; set; }
        public string SERIALNUMBER { get; set; }
        public decimal  ACTUALUNITPRICE { get; set; }
        public string ORDER_CHANNEL { get; set; }
    }
}
