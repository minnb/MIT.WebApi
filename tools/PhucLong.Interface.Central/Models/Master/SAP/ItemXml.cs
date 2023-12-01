using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Master.SAP
{
    public class ItemXml
    {
        [JsonProperty("ns0:mt_cx_item")]
        public mt_cx_item mt_cx_item { get; set; }
    }

    public class mt_cx_item
    {
        public object IDocInfo { get; set; }
        public DataItemXml Data { get; set; }
    }

    public class ItemXmlList
    {
        [JsonProperty("ns0:mt_cx_item")]
        public List<DataItemXml> Data { get; set; }
    }

    public class DataItemXml
    {
        public string No { get; set; }
        public string Description { get; set; }
        public string LongDescription { get; set; }
        public string BaseUnitOfMeasure { get; set; }
        public bool Blocked { get; set; }
        public string TaxGroupCode { get; set; }
        public string MCH { get; set; }
        public string SIZE_DIM { get; set; }
        public string BASIC_MATL { get; set; }
    }
}
