using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Common.Models.Loyalty
{
    public class RspLoyalty
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public DataLoyalty Data { get; set; }
    }
    public class LoyaltyLogging : RspLoyalty
    {
        public string CardNumber { get; set; }
        public string OrderNo { get; set; }
        public decimal BillAmount { get; set; }
        public string UpdateFlg { get; set; }
    }

    public class DataLoyalty
    {
        public int PointEarn { get; set; }
        public int PointRedeem { get; set; }
        public decimal CurrentRate { get; set; }
        public bool IsOfflineVinID { get; set; }
    }
}
