using System;
using System.Collections.Generic;
using System.Text;

namespace ShopInShop.Interface.Models.Odoo
{
    public class TransRawOdoo
    {
        public string OrderNo { get; set; }
        public string StoreNo { get; set; }
        public string UpdateFlg { get; set; }
        public string RawData { get; set; }
        public DateTime CrtDate { get; set; }
    }
}
