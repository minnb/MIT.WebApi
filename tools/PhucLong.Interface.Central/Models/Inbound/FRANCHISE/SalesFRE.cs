using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Inbound.FRANCHISE
{
    public class SalesFRE
    {
        public string PL_STORE { get; set; }
        public string SAP_STORE { get; set; }
        public string POSTING_DATE { get; set; }
        public string PL_POS_NO { get; set; }
        public string TRANS_NO { get; set; }
        public string TRANS_TYPE { get; set; }
        public string REF_DOC { get; set; }
        public decimal SALE_AMT { get; set; }
        public string SALE_CURR { get; set; }
    }
    public class FRANCHISE_Dto : SalesFRE
    {
        public string FileValue { get; set; }
        public string UpdateFlg { get; set; }
        public DateTime CrtDate { get; set; }
        public DateTime ChgDate { get; set; }
    }
}
