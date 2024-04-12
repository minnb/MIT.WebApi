using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Interface.Services.GCP
{
    public class GCP_MCH_INFO: GCP_MCH_INFO_DATA
    {
        public DateTime INSERT_DATE { get; set; }
    }
    public class GCP_MCH_INFO_DATA 
    {
        public string FIELD_ID { get; set; }
        public string MCH1_ID { get; set; }
        public string MCH2_ID { get; set; }
        public string MCH3_ID { get; set; }
        public string MCH4_ID { get; set; }
        public string MCH5_ID { get; set; }
        public string MCH6_ID { get; set; }
        public string MCH1_NAME { get; set; }
        public string MCH2_NAME { get; set; }
        public string MCH3_NAME { get; set; }
        public string MCH4_NAME { get; set; }
        public string MCH5_NAME { get; set; }
        public string MCH6_NAME { get; set; }
    }
}
