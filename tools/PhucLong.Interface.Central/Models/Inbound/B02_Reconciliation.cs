using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.Inbound
{
    public class B02_Reconciliation
    {
        public string StoreCode { get; set; }
        public string BusinessDate { get; set; }
        public string BillNumber { get; set; }
        public string BillAmount { get; set; }
    }

    public class B02_Reconciliation_Dto: B02_Reconciliation
    {
        public string FileValue { get; set; }
        public string UpdateFlg { get; set; }
        public DateTime CrtDate { get; set; }
        public DateTime ChgDate { get; set; }
    }
}
