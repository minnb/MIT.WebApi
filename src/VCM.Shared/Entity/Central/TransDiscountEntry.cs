using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Central
{
	[Table("TransDiscountEntry")]
	public class TransDiscountEntry
    {
		public string OrderNo { get; set; }
		public int OrderId { get; set; }
		public int LineId { get; set; }
		public int LineNo { get; set; }
		public int OrderLineNo { get; set; }
		public int ParentLineNo { get; set; }
		public string ItemNo { get; set; }
		public string OfferNo { get; set; }
		public string OfferType { get; set; }
		public int DiscountType { get; set; }
		public decimal Quantity { get; set; }
		public decimal DiscountAmount { get; set; }
		public string LineGroup { get; set; }
	}
}
