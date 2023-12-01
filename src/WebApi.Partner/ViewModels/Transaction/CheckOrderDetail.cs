using System.Collections.Generic;

namespace WebApi.Partner.ViewModels.Transaction
{
    public class OrderDetailResponse
    {
        public string StoreNo { get; set; }
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public string OrderType { get; set; }
        public List<OrderDetailData> Items { get; set; }

    }
    public class OrderDetailData
    {
        public int LineNo { get; set; }
        public string ItemNo { get; set; }
        public string Barcode { get; set; }
        public string ItemName { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal LineAmountIncVAT { get; set; }
        public int VatCode { get; set; }
    }
    public class GetOrderDetailFromDB: OrderDetailData
    {
        public string StoreNo { get; set; }
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public string OrderType { get; set; }
    }
}
