using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Odoo.Models.RECONCILE
{
    public class PaymentReconcileModel
    {
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentName { get; set; }
        public decimal PaymentAmount { get; set; }
    }
    public class SalesReconcileModel
    {
        public DateTime EntryDate { get; set; }
        public string StoreNo { get; set; }
        public int TotalBill { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaymentAmount { get; set; }
    }
}
