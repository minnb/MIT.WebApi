using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.PhucLong.Models
{
    public class check_order_detail
    {
        public string appcode { get; set; }
        public string orderno { get; set; }
        public DateTime orderdate { get; set; }
        public string storeno { get; set; }
        public string storename { get; set; }
        public int salestypeid { get; set; }
        public string salestypename { get; set; }
        public decimal totalamount { get; set; }
        public decimal discountamount { get; set; }
        public string membercardnumber { get; set; }
        public decimal loyaltypointsearn { get; set; }
        public decimal loyaltypointsredeem { get; set; }
        public string status { get; set; }
        public DateTime crtdate { get; set; }
    }
}
