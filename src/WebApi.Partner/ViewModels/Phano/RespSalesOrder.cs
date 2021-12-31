using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VCM.Partner.API.ViewModels.Phano
{
    public class RespSalesOrderPNP
    {
        public int Status { get; set; }
        public string Description { get; set; }
        public DataSales Data { get; set; }
    }

    public class DataSales
    {
        public string Serial { get; set; }
        public string DocumentTime { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public decimal TotalAmount { get; set; }
        public string Type { get; set; }
        public string Ref { get; set; }
        public bool Paid { get; set; }
    }
    public class RespUpdateSalesOrderPNP
    {
        public int Status { get; set; }
        public string Description { get; set; }
        public bool Data { get; set; }
    }
}
