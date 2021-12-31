using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.ApiModel.CRX
{
    public class VoucherStock
    {
        public string MerchantId { get; set; }
        public string ChannelId { get; set; }
        public List<VoucherCRX> VoucherList { get; set; }
    }
    
    public class VoucherCRX
    {
        public string VoucherSerial { get; set; }
        public string Status { get; set; }
        public long UpdatedDate { get; set; }
    }
    public class RedeemVoucherSAP
    {
        public string Partner { get; set; }
        public string OrderNo { get; set; }
        public string StoreNo { get; set; }
        public string PosID { get; set; }
        public string StaffCode { get; set; }
        public decimal TotalBill { get; set; }
        public List<ListSeriNoSAP> ListSeriNo { get; set; }
    }
    public class ListSeriNoSAP
    {
        public string SeriNo { get; set; }
        public bool IsVoucher { get; set; }
        public decimal Value { get; set; }
    }
}
