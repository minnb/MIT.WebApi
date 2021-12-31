using System;
using System.Collections.Generic;
using System.Text;

namespace ShopInShop.Interface.Models.Central
{
    public class TransHeaderCentral
    {
        public int ID { get; set; }
        public string OrderNo { get; set; }
        public string StoreNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string CompCode { get; set; }
        public string Type { get; set; }
        public string OrderNoOrig { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
