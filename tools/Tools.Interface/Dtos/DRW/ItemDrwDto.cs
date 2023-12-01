using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Interface.Dtos.DRW
{
    public class ItemDrwDto
    {
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string BaseUnit { get; set; }
        public string SalesUnit { get; set; }
        public string TaxGroupCode { get; set; }
        public string ItemCategoryCode { get; set; }
        public int Blocked { get; set; }
        public string MaDQG { get; set; }
        public string SoDK { get; set; }
        public string MadeIn { get; set; }

    }
}
