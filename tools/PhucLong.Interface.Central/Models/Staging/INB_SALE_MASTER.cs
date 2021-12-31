using PhucLong.Interface.Central.Models.Inbound;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Staging
{
    public class INB_SALE_MASTER : E1BPTRANSACTION
    {
        public int LOYALTYSEQUENCENUMBER { get; set; }
        public string CUSTOMERCARDNUMBER { get; set; }
        public string FIELDGROUP  { get;set;}
        public string FIELDNAME { get; set; }
        public string FIELDVALUE { get; set; }
        public string UPDATE_FLG { get; set; }
        public string CRT_DATE { get; set; }
        public string CHGE_DATE { get; set; }
        public Guid ID { get; set; }
    }
}
