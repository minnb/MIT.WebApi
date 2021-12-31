using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.POS
{
    public class MemberInfoDto
    {
        public string MemberId { get; set; }
        public string Name { get; set; }
        public string Level { get; set; }
        public int CurrentPoint { get; set; }
        public int TotalPoint { get; set; }
        public int PointEarn { get; set; }
        public string ExpiredDate { get; set; }
    }
    public class StoreInfoDto
    {
        public int StoreId { get; set; }
        public string StoreNo { get; set; }
        public string StoreName { get; set; }
        public string PosNo { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Hotline { get; set; }
        public string OpenTime { get; set; }
        public string WebSite { get; set; }
        public string ReceiptHeader { get; set; }
        public string ReceiptFooter { get; set; }
        //public string QRCode { get; set; }
    }
}
