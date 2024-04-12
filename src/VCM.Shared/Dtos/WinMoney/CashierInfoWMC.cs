using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Dtos.WinMoney
{
    public class CashierInfoWMC
    {
        public string StoreNo { get; set; }
        public string CashierID { get; set; }
        public string CashierName { get; set; }
        public string CashierPhone { get; set; }
        public bool CashierStatus { get; set; }
        public bool CashierGender { get; set; }
        public bool CashierJob { get; set; }
    }
}
