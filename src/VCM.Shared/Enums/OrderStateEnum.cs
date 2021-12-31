using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Enums
{
    public enum OrderStatusEnum
    {
        Waiting = 0,
        Paid = 1,
        OrderPicking = 2,
        OrderDelivering = 3,
        OrderFinished = 4,
        OrderCanceled = 5,
        OrderRemove = 6,
        SaleReturn = 10,
        SaleDelete = 11,
        SaleReturnFull = 70,
        SaleReturPart = 71,
        SaleNew = 99
    }
}