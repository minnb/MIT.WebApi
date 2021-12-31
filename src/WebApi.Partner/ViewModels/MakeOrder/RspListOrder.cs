using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.WebApi.GPAY.ViewModels.MakeOrder
{
    public class RspListOrder
    {
        public List<ListOrderMOR> Data { get; set; }
        public PagingOrderMOR Paging { get; set; }
    }
    public class ListOrderMOR
    {
        public string OrderCode { get; set; }
        public string Status { get; set; }
        public string StatusName { get; set; }
        public string PartnerKiosk { get; set; }
        public string PartnerKioskName { get; set; }
        public string OrderTime { get; set; }
        public string PayTime { get; set; }
        public int TotalItem { get; set; }
        public decimal TotalBill { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerName { get; set; }
        public string CashierId { get; set; }
        public string CashierName { get; set; }
    }
    public class PagingOrderMOR
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class RequestListOrderMOR
    {
        public string OrderCode { get; set; }
        public string Status { get; set; }
        public string StoreId { get; set; }
        public string PartnerKioskNo { get; set; }
        public string OrderTimeFrom { get; set; }
        public string OrderTimeTo { get; set; }
        public int TotalItemFrom { get; set; }
        public decimal TotalItemTo { get; set; }
        public string CustomerKeyword { get; set; }
        public string ItemKeyword { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}