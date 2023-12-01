using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.Central
{
    public class StoreSetServer
    {
        public string StoreNo { get; set; }
        public string SubSet { get; set; }
        public string ServerIP { get; set; }
        public string ServerRead { get; set; }
        public bool Blocked { get; set; }
        public string PosType { get; set; }
        public string StoreNoWPH { get; set; }
        public string StoreNoPLH { get; set; }
    }
    
}
