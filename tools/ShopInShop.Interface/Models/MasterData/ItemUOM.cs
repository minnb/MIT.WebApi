using System;
using System.Collections.Generic;
using System.Text;

namespace ShopInShop.Interface.Models.MasterData
{
    public class ItemUOM
    {
        public string ItemNo { get; set; }
        public string Code { get; set; }
        public decimal QtyPerUOM { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Cubage { get; set; }
        public decimal Weight { get; set; }

    }
}


