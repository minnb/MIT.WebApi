namespace Hifresh.Interface.Models.Hifresh
{
    public class SalesExpModel
    {
        public string VENDOR { get; set; }
        public string STOREID { get; set; }
        public string KEY { get; set; }
        public string SALESORDERNUMBER { get; set; }
        public bool ISSUEINVOICE { get; set; }
        public string CUSTOMERNAME { get; set; }
        public string CUSTOMERADDRESS { get; set; }
        public string CUSTOMEREMAIL { get; set; }
        public string CUSTOMERTAXNUMBER { get; set; }
        public string ITEMCODE { get; set; }
        public string ITEMNAME { get; set; }
        public string UNIT { get; set; }
        public decimal QUANTITY { get; set; }
        public string CURRENCY { get; set; }
        public bool PRICEINCLUDESTAX { get; set; }
        public decimal TAXOUT { get; set; }
        public decimal UNITPRICE { get; set; }
        public decimal DISCOUNTAMOUNT { get; set; }
        public decimal AMOUNT { get; set; }
        public string SALESDATE { get; set; }
        public decimal COSTAMOUNT { get; set; }
        public decimal TAXIN { get; set; }
    }
}
