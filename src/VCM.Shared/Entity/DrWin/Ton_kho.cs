using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VCM.Shared.Entity.DrWin
{
    public class Ton_kho
    {
        public string Ma_sap { get; set; }
        public string Ma_thuoc { get; set; }
        public string Don_vi_tinh { get; set; }
        public int So_luong_ton_dau { get; set; }
        public int So_luong_xuat { get; set; }
        public int So_luong_ton { get; set; }
        public string So_lo { get; set; }
        public string Han_dung { get; set; }
        public int Priority { get; set; }
        public string Ma_phieu { get; set; }
        public string Loai_phieu { get; set; }
        public DateTime Ngay_xuat { get; set; }
        public DateTime Thoi_gian { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "decimal(18,0)")]
        public decimal Id { get; set; }
        public bool IsProcess { get; set; }
    }
}
