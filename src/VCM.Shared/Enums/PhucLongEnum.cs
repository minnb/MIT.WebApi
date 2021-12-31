using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Enums
{
    public enum TransactionTypePLG
    {
        PLT1 = 1,
        PLT2 = 2,
        PLT3 = 3,
    }

    public enum OrderStateEnumPLG
    {
        Paid = 1,
        Cancel = 0,
        Draft = 99
    }
    public enum LineTypeEnumPLG
    {
        SALES = 0,
        CUP = 1,
        MARTERIAL = 2,
        COMBO = 3,
        PROMO = 4,
        TOPPING = 5,
        TICKET = 9
    }
    public enum VoucherStateEnumPLG
    {
        Create,
        Close,
        Cancel
    }
}
