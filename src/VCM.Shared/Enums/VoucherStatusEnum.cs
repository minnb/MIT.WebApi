using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Enums
{
    public enum SAPVoucherStatusEnum
    {
        NOTEXIST = 0,
        NEW = 10,
        AVL = 20,
        PEDDINGPRINT = 30,
        INSTOCK = 40,
        SOLD = 50,
        RDM = 60,
        CANCELLED = 70,
        EXPIRED = 80,
        EXP = 90
    }
    public enum VoucherStatusEnum
    {
        SOLD,
        REDE,
        AVAI,
        ERROR
    }
    public enum VoucherUpdateStatusEnum
    {
        Success,
        Activated,
        NotExist,
        OutOfDate,
        UsedOrExpired,
        Errors
    }
}
