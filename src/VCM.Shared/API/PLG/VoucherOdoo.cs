using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.PLG
{
    public class RequestVoucherCheckOdoo
    {
        [Required]
        [DefaultValue("6100459252")]
        public string SERIALNUMBER { get; set; } = "98864484701211020210847";

        [DefaultValue("X")]
        public string ISVOUCHER { get; set; } = "";
        [DefaultValue("1")]
        public string ISEMPLOYEE { get; set; } = "0";
    }
    public class ResponseVoucherCheckOdoo
    {
        public string SERIALNUMBER { get; set; }
        public string FROMDATE { get; set; }
        public string TODATE { get; set; }
        public string VALUE { get; set; }
        public string REMAINAMOUNT { get; set; }
        public string STATUSCODE { get; set; }
        public string MATERIALNUMBER { get; set; }
        public string SALESDATE { get; set; }
        public string SALESTRANS { get; set; }
        public string REDEEMTRANS { get; set; }
        public string REDEEMDATE { get; set; }
        public string REDEEMPLANT { get; set; }
        public string ISEMPLOYEE { get; set; }
        public string STATUSMESSAGE { get; set; }
        public string STATUSMESSAGECODE { get; set; }
        public string SALESPLANT { get; set; }
        public string ISVOUCHER { get; set; }

    }
    public class RequestVoucherRedeemOdoo
    {
        [Required]
        [DefaultValue("98864484701211020210847")]
        public string SeriNo { get; set; } = "98864484701211020210847";
        public string IsVoucher { get; set; } 
        public string StatusCode { get; set; } 
        public string SalePlant { get; set; }
        public string SalePrice { get; set; } 
        public string Discount { get; set; } 
        public string SaleDate { get; set; }
        public string SaleTrans { get; set; } 
        public string RedeemAmount { get; set; } 
        public string RedeemPlant { get; set; }
        public string RedeemDate { get; set; } 
        public string RedeemTrans { get; set; } 
    }
    public class ResponseVoucherRedeemOdoo
    {
        public string REDEEMPLANT { get; set; }
        public string SERIALNUMBER { get; set; } 
        public string STATUSCODE { get; set; } 
        public string MATERIALNUMBER { get; set; }
        public string REMAINAMOUNT { get; set; } 
        public string STATUSMESSAGE { get; set; } 
        public string STATUSMESSAGECODE { get; set; }

    }
}
