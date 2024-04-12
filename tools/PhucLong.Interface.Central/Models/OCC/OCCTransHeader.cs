using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PhucLong.Interface.Central.Models.OCC
{
    [Table("OCC_TransHeader")]
    public class OCCTransHeader
    {
        public string AppCode { get; set; }
        public string Version {get;set;}
        public DateTime OrderDate { get; set; }
        public string OrderNo { get; set; }
        public string OrderNo2 { get; set; }
        public string StoreNo { get; set; }
        public string StoreNo2 { get; set; }
        public string PosNo { get; set; }
        public string TransactionType { get; set; }
        public string OrderType { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        public string TransactionCurrency { get; set; }
        public string MemberCardNo { get; set; }
        public decimal MemberPointsEarn { get; set; }
        public decimal MemberPointsRedeem { get; set; }
        public bool IssuedVATInvoice { get; set; }
        public string RefKey { get; set; }
        public string UpdateFlg { get; set; }
        public bool IsEOD { get; set; }
        public DateTime CrtDate { get; set; }
        public DateTime EndingTime { get; set; }
    }
}
