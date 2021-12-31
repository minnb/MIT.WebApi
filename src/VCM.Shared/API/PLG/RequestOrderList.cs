using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.API.PLG
{
    public class RequestOrderList
    {
        [Required]
        [DefaultValue("20211001")]
        public string payment_date { get; set; } = DateTime.Now.AddDays(-5).ToString("yyyyMMdd");

        [Required]
        [Range(1, 999999)]
        [DefaultValue(1626)]
        public int location_id { get; set; } = 1626;

        [Required]
        [Range(1, 999999)]
        [DefaultValue(215)]
        public int warehouse_id { get; set; } = 215;

        [Required]
        [DefaultValue(30)]
        public int[] payment_method { get; set; } = { 30 };

        [Required]
        [DefaultValue(1)]
        public int set { get; set; } = 1;

    }
}
