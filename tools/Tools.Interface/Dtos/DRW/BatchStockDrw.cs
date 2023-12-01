using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Interface.Dtos.DRW
{
    public class BatchStockXmlDrw
    {
        [JsonProperty("ns0:MT_BLUEPOS_BatchStock")]
        public MT_BLUEPOS_BatchStock MT_BLUEPOS_BatchStock { get; set; }

    }
    public class BatchStockXmlDrw_1
    {
        [JsonProperty("ns0:MT_BLUEPOS_BatchStock")]
        public MT_BLUEPOS_BatchStock_1 MT_BLUEPOS_BatchStock { get; set; }

    }
    public class MT_BLUEPOS_BatchStock
    {
        public List<BatchStockDrw> LineItem { get; set; }
    }
    public class MT_BLUEPOS_BatchStock_1
    {
        public BatchStockDrw LineItem { get; set; }
    }

    public class BatchStockDrw
    {
        public string Site { set; get; }
        public string ArticleNumber { set; get; }
        public string StorageLocation { set; get; }
        public string Batch { set; get; }
        public string Quantity { set; get; }
        public string SLED { set; get; }
        public string Priority { set; get; }
    }
}
