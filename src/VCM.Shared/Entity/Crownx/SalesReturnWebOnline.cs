using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Crownx
{
    public class SalesReturnWebOnline
    {
        public string PartnerCode { get; set; }
        public string AppCode { get; set; }
        public string InvoiceNo { get; set; }
        public string OrgInvoiceNo { get; set; }
        public string OrgOrderNo { get; set; }
        public DateTime TransactionDatetime { get; set; }
        public string MerchantId { get; set; } //VCM; PLH
        public string StoreNo { get; set; }
        public string Note { get; set; }
        public string TenderType { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal RefundAmount { get; set; }
        public string ReRefundItems { get; set; }
        public string UpdateFlg { get; set; }
        public string Message { get; set; }
        public DateTime CreateTime { get; set; }

    }
}
