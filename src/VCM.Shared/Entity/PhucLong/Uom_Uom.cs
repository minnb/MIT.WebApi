using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCM.Shared.Entity.PhucLong
{
    public class Uom_Uom
    {
        public int id { get; set; }
        public string name { get; set; }
        public int category_id { get; set; }
        public decimal factor { get; set; }
        public decimal rounding { get; set; }
        public bool active { get; set; }
        public string uom_type { get; set; }
        public string measure_type { get; set; }
        public int create_uid { get; set; }
        public DateTime create_date { get; set; }
        public int write_uid { get; set; }
        public DateTime write_date { get; set; }
    }
}
