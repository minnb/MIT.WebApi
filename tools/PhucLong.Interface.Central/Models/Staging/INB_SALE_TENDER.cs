using PhucLong.Interface.Central.Models.Inbound;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Staging
{
    public class INB_SALE_TENDER: E1BPTENDER
    {
        public string PAYMENTCARD { get; set; }
        public string CARDNUMBER { get; set; }
        public string CARDNUMBERSUFFIX { get; set; }
        public string CARDEXPIRATIONDATE { get; set; }
        public string CARDHOLDERNAME { get; set; }
        public string ADJUDICATIONCODE { get; set; }
        public string AUTHORIZATIONDATETIME { get; set; }
        public string AUTHORIZATIONCODE { get; set; }
        public decimal REQUESTEDAMOUNT { get; set; }
        public string CARDTYPE { get; set; }
        public int LOYALTYSEQUENCENUMBER { get; set; }
        public string CUSTOMERCARDNUMBER { get; set; }
        public string UPDATE_FLG { get; set; }
    }
}
