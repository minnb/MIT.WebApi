using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCM.Shared.Entity.PhucLong
{
    public class Product_Category
    {
        public int id { get; set; }
        public string parent_path { get; set; }
        public string name { get; set; }
        public string complete_name { get; set; }
        public int parent_id { get; set; }
        public int create_uid { get; set; }
        public DateTime create_date { get; set; }
        public int write_uid { get; set; }
        public DateTime write_date { get; set; }
        public int removal_strategy_id { get; set; }
        public int sequence { get; set; }
        public string code { get; set; }
        public string type { get; set; }
        public int level { get; set; }
        public string fnb_type { get; set; }
    }
}
