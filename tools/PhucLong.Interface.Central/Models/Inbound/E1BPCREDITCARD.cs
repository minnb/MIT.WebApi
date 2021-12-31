using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Inbound
{
    public class E1BPCREDITCARD: INB_HEADER
    {
        public int TENDERSEQUENCENUMBER { get; set; }
        public string PAYMENTCARD { get; set; }
        public string CARDNUMBER { get; set; }
        public string CARDNUMBERSUFFIX { get; set; }
        public string CARDEXPIRATIONDATE { get; set; }
        public string CARDHOLDERNAME { get; set; }
        public string AUTHORIZINGTERMID { get; set; }
        public string ADJUDICATIONCODE { get; set; }
        public string AUTHORIZATIONDATETIME { get; set; }
        public string AUTHORIZATIONCODE { get; set; }
        public decimal REQUESTEDAMOUNT { get; set; }
        public string CARDTYPE { get; set; }
    }
}
