using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Shared.Dtos.POS
{
    public class TransHeaderDto
    {
        public string AppCode { get; set; }
        public string OrderNo { get; set; }
        public string OrderTime { get; set; }
        public int DeliveryType { get; set; }
        public string CustNo { get; set; }
        public string CustName { get; set; }
        public string CustPhone { get; set; }
        public string CustAddress { get; set; }
        public string CustNote { get; set; }
        public string CardMember { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public int Status { get; set; }
        public bool IsPromotion { get; set; }
        public string RefNo { get; set; }
        public string CashierId { get; set; }
        public string CashierName { get; set; }
        public decimal PromoAmount { get; set; }
        public string PromoName { get; set; }
        public MemberInfoDto MemberInfo { get; set; }
        public StoreInfoDto StoreInfo { get; set; }
        public List<TransLineDto> TransLine { get; set; }
        public List<TransPaymentEntryDto> TransPaymentEntry { get; set; }
    }
}
