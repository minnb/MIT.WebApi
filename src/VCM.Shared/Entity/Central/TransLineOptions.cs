using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Central
{
    [Table("TransLineOptions")]
    public class TransLineOptions
    {
        public string OrderNo { get; set; }
        public int LineNo { get; set; }
        public int OrderLineNo { get; set; }
        public int OptionId { get; set; }
        public string OptionType { get; set; }
        public string OptionsName { get; set; }
        public int ProductId { get; set; }
        public int ProductUomId { get; set; }
        public string Uom { get; set; }
        public string UomVN { get; set; }
        public int ProductMaterialId { get; set; }
        public decimal ProductQty { get; set; }
    }
}
