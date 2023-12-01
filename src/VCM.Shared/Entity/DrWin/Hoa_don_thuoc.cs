using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.DrWin
{
    public class Hoa_don_thuoc
    {
        [Required]
        [DefaultValue("P100")]
        public string Ma_sap { get; set; } = "P100";
        [Required]
        public string Ma_hoa_don { get; set; }
        [Required]
        public string Ma_co_so { get; set; }
        public string Ma_don_thuoc_quoc_gia { get; set; }
        [Required]
        public string Ngay_ban { get; set; } //yyyyMMdd
        public string Ho_ten_nguoi_ban { get; set; }
        public string Ho_ten_khach_hang { get; set; }
    }
    public class Hoa_don_chi_tiet
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
        public string Don_vi_tinh { get; set; }
        [Required]
        public string Ham_luong { get; set; }
        [Required]
        public int So_luong { get; set; }
        [Required]
        public int Don_gia { get; set; }
        [Required]
        public int Thanh_tien { get; set; }
        [Required]
        public int Ty_le_quy_doi { get; set; }
        [Required]
        public string Lieu_dung { get; set; }
        public string Duong_dung { get; set; }
    }
}
