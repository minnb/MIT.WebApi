using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.DrWin
{
    public class Phieu_xuat_thuoc
    {
        [Required]
        [DefaultValue("P100")]
        public string Ma_sap { get; set; } = "P100";
        [Required]
        public string Ma_phieu { get; set; }
        public string Ma_co_so { get; set; }
        [Required]
        public string Ngay_xuat { get; set; } //yyyyMMdd
        [Required]
        [DefaultValue(1)]
        public int Loai_phieu_xuat { get; set; } = 1;
        public string Ghi_chu { get; set; }
        public string Ten_co_so_nhan { get; set; }
    }
    public class Phieu_xuat_thuoc_chi_tiet
    {
        [Required]
        public string Ma_thuoc { get; set; }
        [Required]
        public string Ten_thuoc { get; set; }
        [Required]
        public string So_lo { get; set; }
        public string Ngay_san_xuat { get; set; }//yyyyMMdd
        [Required]
        public string Han_dung { get; set; }//yyyyMMdd
        [Required]
        public string So_dklh { get; set; }
        [Required]
        public int So_luong { get; set; }
        [Required]
        public int Don_gia { get; set; }
        [Required]
        public string Don_vi_tinh { get; set; }
    }
}
