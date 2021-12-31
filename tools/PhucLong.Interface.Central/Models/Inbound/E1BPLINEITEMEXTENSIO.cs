using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Inbound
{
    public class E1BPLINEITEMEXTENSIO: INB_HEADER
    {
        public int RETAILSEQUENCENUMBER { get; set; }
        public string RETAILTYPECODE { get; set; }
        public string FIELDGROUP { get; set; }
        public string FIELDNAME { get; set; }
        public string FIELDVALUE { get; set; }
    }
}
