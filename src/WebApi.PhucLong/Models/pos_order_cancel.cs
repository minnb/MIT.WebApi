using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.PhucLong.Models
{
    public class pos_order_cancel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public int location_id { get; set; }
    }
}