using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Inbound
{
    public class E1BPTRANSACTEXTENSIO : INB_HEADER
    {
        public string FIELDGROUP { get; set; }
        public string FIELDNAME { get; set; }
        public string FIELDVALUE { get; set; }
    }
}
