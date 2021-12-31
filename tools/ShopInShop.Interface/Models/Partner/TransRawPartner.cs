using System;
using System.Collections.Generic;
using System.Text;

namespace ShopInShop.Interface.Models.Partner
{
    public class TransRawPartner
    {
        public string AppCode { get; set; }
        public string StoreNo { get; set; }
        public string ServiceType { get; set; }
        public string OrderNo { get; set; }
        public string ReferenceNo { get; set; }
        public string RawData { get; set; }
        public string UpdateFlg { get; set; }
        public string IPAddress { get; set; }
        public DateTime CrtDate { get; set; }
        public string CrtUser { get; set; }
        public Guid Id { get; set; }
    }
}
