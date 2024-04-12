using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.Dtos.WinMoney
{
    public class GetTokenWMC
    {
        public string Cashier_id { get;set; }
        public string Cashier_name { get; set; }
        public string Pos_no { get; set; }
        public string Store_no { get; set; }
        public string Merchant_no { get; set; }
        public string Pos_browser_uid { get; set; }
    }
    public class Response_WMC 
    {
        public string Error_code { get; set; }
        public Error_message Error_message { get; set; }
    }
    public class Url_Success_WMC
    {
        public string Error_code { get; set; }
        public Error_message Error_message { get; set; }
        public DataToken_WMC Data { get; set; }
    }

    public class Error_message
    {
        public string Vi {  get; set; }
        public string En { get; set; }
    }
    public class DataToken_WMC 
    {
        public string Token { get; set; }
        public string ExpiredTime { get; set; }
    }
    public class POSRequestUrl_WMC 
    {
        [Required]
        public string StoreNo { get; set; }
        [Required]
        public string PosNo { get; set; }
        [Required]
        public string CashierID { get; set; }
        [Required]
        public string CashierName { get; set; }
        [Required]
        public string MerchantNo { get; set; }
        [Required]
        public string PosBrowserUID { get; set; }
    }
}
