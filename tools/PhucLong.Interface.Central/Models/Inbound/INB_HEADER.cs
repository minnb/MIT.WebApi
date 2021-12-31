using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Inbound
{
    public class INB_HEADER
    {
        public string RETAILSTOREID { get; set; }
        public string BUSINESSDAYDATE { get; set; }
        public string TRANSACTIONTYPECODE { get; set; }
        public string WORKSTATIONID { get; set; }
        public string TRANSACTIONSEQUENCENUMBER { get; set; }
    }
}
