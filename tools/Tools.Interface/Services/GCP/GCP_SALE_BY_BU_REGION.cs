using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Interface.Services.GCP
{
    public class GCP_SALE_BY_BU_REGION
    {
        public string FIELD_ID { get; set; }
        public string PRODUCT_ID { get; set; }
        public string BU { get; set; }
        public string REGION { get; set; }
        public double AVG_REVENUE { get; set; }
        public double AVG_QTY { get; set; }
        public DateTime INSERT_DATE { get; set; }
    }
}
