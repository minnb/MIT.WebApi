using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.API.O2
{
    public class QualifyO2Response
    {
        public DataQualifyO2 Data { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
        public string Verdict { get; set; }

    }

    public class DataQualifyO2
    {
        public string Phone_number { get; set; }
        public string Tier { get; set; }
        public bool Potential_vip { get; set; }
        public ProductsQualifyO2 Products { get; set; }
    }

    public class ProductsQualifyO2
    {
        public bool Evo { get; set; }
        public bool Tpay { get; set; }
        public bool Wintel { get; set; }
    }
}
