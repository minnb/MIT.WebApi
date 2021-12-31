using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.PhucLong.Dtos
{
    public class PosOrderCancelDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public DateTime date_order { get; set; }
        public string state { get; set; }
        public int location_id { get; set; }
        public int warehouse_id { get; set; }
        public DateTime date_last_order { get; set; }
        public DateTime write_date { get; set; }
        public string note { get; set; }
        public string note_label { get; set; }
    }
}
