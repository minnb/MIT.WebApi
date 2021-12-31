using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Entity.PhucLong.Dtos
{
    public class PosRequestVatDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string date_order { get; set; }
        public string state { get; set; }
        public string location_id { get; set; }
        public string warehouse_id { get; set; }
        public DateTime create_date { get; set; }
        public DateTime write_date { get; set; }
        public string note_label { get; set; }
        public string invoice_name { get; set; }
        public string invoice_vat { get; set; }
        public string invoice_address { get; set; }
        public string invoice_email { get; set; }
        public string invoice_contact { get; set; }
        public string invoice_note { get; set; }
        public bool invoice_request { get; set; }
    }
}
