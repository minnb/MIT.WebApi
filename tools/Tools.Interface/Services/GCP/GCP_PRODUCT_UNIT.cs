using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Interface.Services.GCP
{
    public class GCP_PRODUCT_UNIT: GCP_PRODUCT_UNIT_DATA
    {
        public DateTime INSERT_DATE { get; set; }

    }
    public class GCP_PRODUCT_UNIT_DATA
    {
        public string FIELD_ID { get; set; }
        public string PRODUCT_ID { get; set; }
        public string UNIT { get; set; }
        public string BARCODE { get; set; }
        public string BARCODE_FULL { get; set; }
        public decimal NUMERATOR { get; set; }
        public string BASE_UOM { get; set; }
    }
}
