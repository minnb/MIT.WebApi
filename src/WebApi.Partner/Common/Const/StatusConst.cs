using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Partner.API.Common.Const
{
    public class StatusConst
    {
        public static Dictionary<int, string> MappingStatus()
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
        public static Dictionary<string, int> StatusDichoWMT()
        {
            Dictionary<string, int> openWith = new Dictionary<string, int>
            {
                {"New", 99 },
                {"Delivered", 1},
                {"Canceled", 10},
                {"Confirmed", 0},
            };
            return openWith;
        }

        public static Dictionary<string, int> StatusPhano()
        {
            Dictionary<string, int> openWith = new Dictionary<string, int>
            {
                { "Sale", 0},
                { "Return", 70},
            };
            return openWith;
        }
        public static Dictionary<string, int> StatusMOR()
        {
            Dictionary<string, int> openWith = new Dictionary<string, int>
            {
                { "NEW", 0},
                { "PAID", 1},
            };
            return openWith;
        }
    }
}
