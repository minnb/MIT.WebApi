using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Enums
{
    public enum VoucherStatusEnum
    {
        SOLD,
        REDE
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
