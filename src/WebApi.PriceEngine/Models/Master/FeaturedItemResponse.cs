namespace WebApi.PriceEngine.Models.Master
{
    public class FeaturedItemResponse
    {
        public string StoreNo { get; set; }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public string Uom { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public string DiscountType { get; set; }
        public string StartingDate { get; set; }
        public string EndingDate { get; set; }
        public string Size { get; set; }
        public string ImageName { get; set; }
    }
}
