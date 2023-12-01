using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VCM.Shared.Entity.DrWin
{
    public class Them_moi_thuoc_co_so
    {
        [Required]
        [DefaultValue("P100")]
        public string Ma_sap { get; set; } = "P100";
        [Required]
        public string Ten_thuoc { get; set; }
        [Required]
        public string Ma_co_so { get; set; }
        [Required]
        public string So_dang_ky { get; set; } //yyyyMMdd
        [Required]
        public string Ten_hoat_chat { get; set; }
        [Required]
        public string Ham_luong { get; set; }
        [Required]
        public string Dong_goi { get; set; }
        [Required]
        public string Hang_san_xuat { get; set; }
        [Required]
        public string Nuoc_san_xuat { get; set; }
        [Required]
        public string Don_vi_tinh { get; set; }

    }
}
