using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.Partner
{
    [Table("M_RawData")]
    public class RawData: RawDataDto
    {
    }
    public class RawDataDto
    {
        public string PartnerCode { get; set; }
        public string AppCode { get; set; }
        public string StoreNo { get; set; }
        public string OrderNo { get; set; }
        public string RequestId { get; set; }
        public string JsonData { get; set; }
        public string UpdateFlg { get; set; }
        public string HostName { get; set; }
        public string CrtUser { get; set; }
        public DateTime CrtDate { get; set; }
    }
}
