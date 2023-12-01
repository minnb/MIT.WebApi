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
    public enum OrderStateEnumPLH
    {
        Processing = 1,
        Delivery = 2,
        Success = 3,
        Cancel = 4,
        Returned = 5
        //Đang xử lý: 1
        //Đang giao: 2
        //Hoàn thành: 3
        //Hủy: 4
    }
    public enum LineTypeEnumPLG
    {
        SALES = 0,
        CUP = 1,
        MARTERIAL = 2,
        COMBO = 3,
        PROMO = 4,
        TOPPING = 5,
        FEE = 6,
        TICKET = 9
    }
    public enum ProductSize
    {
        M = 1,
        L = 2,
        Hot = 3
    }
    public enum VoucherStateEnumPLG
    {
        Create,
        Close,
        Cancel
    }
}
