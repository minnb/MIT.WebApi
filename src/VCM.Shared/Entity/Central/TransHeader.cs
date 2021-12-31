using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Central
{
    [Table("TransHeader")]
    public class TransHeader
    {
        public string AppCode { get; set; }
        public string Version { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string StoreNo { get; set; }
        public int LocationId { get; set; }
        public string PosNo { get; set; }
        public int ShiftNo { get; set; }
        public int CashierId { get; set; }
        public string CashierName { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal VATAmount { get; set; }
        public decimal AmountExclVAT { get; set; }
        public decimal AmountInclVAT { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountReturn { get; set; }
        public string OrderType { get; set; }
        public string State { get; set; }
        public string CustNo { get; set; }
        public string CustName { get; set; }
        public string CustPhone { get; set; }
        public string CustAddress { get; set; }
        public string CustEmail { get; set; }
        public int DeliveringMethod { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryComment { get; set; }
        public string GeneralComment { get; set; }
        public string ZoneNo { get; set; }
        public bool IssuedVATInvoice { get; set; }
        public int TransactionType { get; set; }
        public string TransactionTypeName { get; set; }
        public string OriginOrderNo { get; set; }
        public string MemberCardNo { get; set; }
        public decimal MemberPointsEarn { get; set; }
        public decimal MemberPointsRedeem { get; set; }
        public decimal MemberPoint { get; set; }
        public DateTime StartingTime { get; set; }
        public DateTime EndingTime { get; set; }
        public string BillCreationTime { get; set; }
        public string RefKey1 { get; set; }
        public string RefKey2 { get; set; }
        public int StepProcess { get; set; }
        public string UpdateFlg { get; set; }
        public bool IsEOD { get; set; }
        public DateTime CrtDate { get; set; }
        public DateTime ChgeDate { get; set; }
    }
}
