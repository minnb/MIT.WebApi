using System;
using System.Collections.Generic;
using System.Text;

namespace ShopInShop.Interface.Models.MasterData
{
    public class AssortmentArticle
    {
        public string Assortment { get; set; }
        public string Article { get; set; }
        public string ValidFrom { get; set; }
        public string ValidTo { get; set; }
        public string RackJobber { get; set; }
    }
}
