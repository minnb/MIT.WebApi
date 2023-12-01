using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.ApiModel.PLH
{
    public class SalesTrainningRequest
    {
        public string StoreNo { get; set; }
        public string PosID { get; set; }
        public SalesTrainningHeader ReqHeader { get; set; }
        public List<SalesTrainningItem> ReqItem { get; set; }
    }

    public class SalesTrainningHeader
    {
        public string Plant { get; set; }
        public string ResDate { get; set; }
        public string MoveType { get; set; }
        public string CostCenter { get; set; }
        public string RefNo { get; set; }
        public string Order { get; set; }
    }

    public class SalesTrainningItem
    {
        public string StorageLocation { get; set; }
        public string Material { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public string ShortText { get; set; }
        public string Movement { get; set; }
    }

    public class SalesTrainningTemp
    {
        public string StoreNo { get; set; }
        public string PosNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderNo { get; set; }
        public string OrderType { get; set; }
        public int LineNo { get; set; }
        public string ItemNo { get; set; }
        public string Uom { get; set; }
        public decimal Quantity { get; set; }
        public string UpdateFlg { get; set; }
        public string StatusSAP { get; set; }
        public string Message { get; set; }
        public string ResNum { get; set; }
        public string CrtDate { get; set; }
        public string MoveType { get; set; }
        public string CostCenter { get; set; }
        public string ShortText { get; set; }
        public string Movement { get; set; }
        public string StorageLocation { get; set; }
        public string RefNo { get; set; }
        public string Order { get; set; }
    }

    public class SalesTrainningResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public RspDataSalesTrainning Data { get; set; }
    }
    public class RspDataSalesTrainning
    {
        public string StatusSAP { get; set; }
        public string Message { get; set; }
        public string ResNum { get; set; }
    }
}
