using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Const
{
    public static class OrderStatusConst
    {
        public static Dictionary<int, string> MappingOrderStatus()
        {
            Dictionary<int, string> openWith = new Dictionary<int, string>
            {
                { 0, "Đơn hàng chờ thanh toán" },
                { 1, "Đơn hàng đã thanh toán" },
                { 2, "OrderPicking" },
                { 3, "OrderDelivering" },
                { 4, "OrderFinished" },
                { 5, "OrderCanceled" },
                { 6, "OrderRemove" },
                { 10, "Đơn trả hàng" },
                { 11, "Đơn hàng hủy không thanh toán" },
                { 70, "Đơn trả hàng toàn bộ" },
                { 71, "Đơn trả hàng một phần" },
                { 99, "Đơn hàng mới tạo" },
            };
            return openWith;
        }


    }
}
