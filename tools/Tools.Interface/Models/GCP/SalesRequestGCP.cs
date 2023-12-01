using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Interface.Models.GCP
{
    public class PriceRateDaily_GCP: RequestPriceRateDaily_GCP
    {
        public string PRODUCT_ID { get; set; }
        public string UOM { get; set; }
        public string VALID_DATE { get; set; }
        public decimal PRICE_RATE { get; set; }
    }
    public class TablePriceRateDaily_GCP: PriceRateDaily_GCP
    {
        public int TOTAL_SKU { get; set; }
        public string UPDATE_FLG { get; set; }
        public DateTime CRT_DATE { get; set; }
        public string REQUEST { get; set; }
        public string ID { get; set; }
    }

    public class RequestPriceRateDaily_GCP
    {
        public string STORE_ID { get; set; }
        public string CALDAY { get; set; }
    }
    public class RequestPriceRateDailyStoreID_GCP
    {
        public string STORE_ID { get; set; }
        public int TOTAL_SKU { get; set; }
    }
    public class OauthToken_GCP
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
    }
    public class UserOauthToken_GCP
    {
        public string grant_type { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
}
