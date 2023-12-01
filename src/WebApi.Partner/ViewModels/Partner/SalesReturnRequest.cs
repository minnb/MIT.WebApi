using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Partner.ViewModels.Partner
{
    public class SalesReturnRequest
    {
        [Required]
        public string PartnerCode { get; set; } //VMP, PLH
        public string AppCode { get; set; } //VCM, PLH
        [Required]
        public string PosNo { get; set; }
        [Required]
        public string OrderNo { get; set; }
        [Required]
        public string ReturnedOrderNo { get; set; }
        [Required]
        public string OrgOrderNo { get; set; }
        public DateTime TransactionDatetime { get; set; }
        public string Note { get; set; }
        [Required]
        public string TenderType { get; set; }
        public decimal RefundAmount { get; set; }
        public List<RefundItems> RefundItems { get; set; }
        public string InvoiceNo
        {
            get
            {
                return OrderNo;
            }
        }
        public string OrgInvoiceNo
        {
            get
            {
                return ReturnedOrderNo;
            }
        }
        public string MerchantId { get; set; }
        public string StoreNo { get; set; }
    }

    //InvoiceNo = request.OrderNo,
    //OrgInvoiceNo = request.ReturnedOrderNo,
    //OrgOrderNo = request.OrgOrderNo,
    public class RefundItems
    {
        public int LineId { get; set; }
        public int ParentLineId { get; set; }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string Uom { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OldPrice 
        {
            get
            {
                return UnitPrice;
            }
        }
        public decimal Quantity { get; set; }
        public decimal DiscountAmount { get; set; }
        public string TaxGroupCode { get; set; }
        public decimal VatPercent { get; set; }
        public string Barcode { get; set; }
        public string Note { get; set; }
    }
}
