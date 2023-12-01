using Newtonsoft.Json;
using PhucLong.Interface.Central.Models.Master.SAP;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Interface.Models.GCP.SAPHCM
{
    public class SAPHCM_Dashboard_Xml
    {
        [JsonProperty("n0:MT_SAPHR_Dashboard")]
        public MT_SAPHR_Dashboard MT_SAPHR_Dashboard { get; set; }
    }
    public class MT_SAPHR_Dashboard
    {
        [JsonProperty("@xmlns:n0")]
        public string xmlns_n { get; set; }
        [JsonProperty("@xmlns:prx")]
        public string xmlns_prx { get; set; }
        public List<SAPHCM_Dashboard_Dto> Items { get; set; }
    }

    public class SAPHCM_Dashboard_Dto
    {
        public string XmlnsPrx { get; set; }
        public string YEAR { get; set; }
        public string MONTH { get; set; }
        public string KEY_DATE { get; set; }
        public string PERNR { get; set; }
        public string FULLNAME { get; set; }
        public string SEX { get; set; }
        public string DOB { get; set; }
        public string AGE { get; set; }
        public string BU { get; set; }
        public string ENTITY { get; set; }
        public string DEPARTMENT { get; set; }
        public string POSITION { get; set; }
        public string RANK { get; set; }
        public string RANK_GROUP { get; set; }
        public string FUNCTION { get; set; }
        public string FUNCTION_GROUP { get; set; }
        public string MAKE { get; set; }
        public string ONBOA_DATE { get; set; }
        public string WORK_PLACE { get; set; }
        public string CONTRACT { get; set; }
        public string EDU_DEGREE { get; set; }
        public string AGE_GROUP { get; set; }
        public string SENIORITY { get; set; }
        public string RATING { get; set; }
        public string DG_9BOX { get; set; }
        public string DIRECT { get; set; }
    }
}
