using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VCM.Shared.Entity.DrWin;

namespace WCM.EntityFrameworkCore.EntityFrameworkCore.DrWin
{

    [Table("m_ton_kho")]
    public class M_ton_kho : Ton_kho
    {
    }

    [Table("m_tai_khoan_ket_noi")]
    public class M_tai_khoan_ket_noi : StoreUser
    {
    }
    [Table("m_hoa_don_thuoc")]
    public class M_hoa_don_thuoc : Hoa_don_thuoc
    {
        public DateTime Ngay_gio_tao { get; set; }
        public string Trang_thai { get; set; }
        public string Dien_giai { get; set; }
        public string Loai_hoa_don { get; set; }
        public string Ma_hoa_don_goc { get; set; }
    }

    [Table("m_hoa_don_chi_tiet")]
    public class M_hoa_don_chi_tiet : Hoa_don_chi_tiet
    {
        public string Ma_sap { get; set; }
        public string Ma_hoa_don { get; set; }
    }

    [Table("m_phieu_xuat")]
    public class M_Phieu_xuat_thuoc : Phieu_xuat_thuoc
    {
        public string Ma_phieu_xuat_quoc_gia { get; set; }
        public DateTime Ngay_gio_tao { get; set; }
        public string Trang_thai { get; set; }
        public string Dien_giai { get; set; }
    }

    [Table("m_phieu_xuat_chi_tiet")]
    public class M_Phieu_xuat_thuoc_chi_tiet : Phieu_xuat_thuoc_chi_tiet
    {
        public string Ma_phieu { get; set; }
        public string Ma_sap { get; set; }
    }

    [Table("m_phieu_nhap")]
    public class M_Phieu_nhap_thuoc : Phieu_nhap_thuoc
    {
        public string Ma_phieu_nhap_quoc_gia { get; set; }
        public DateTime Ngay_gio_tao { get; set; }
        public string Trang_thai { get; set; }
        public string Dien_giai { get; set; }
    }

    [Table("m_phieu_nhap_chi_tiet")]
    public class M_Phieu_nhap_thuoc_chi_tiet : Phieu_nhap_thuoc_chi_tiet
    {
        public string Ma_sap { get; set; }
        public string Ma_phieu { get; set; }
    }

    [Table("m_logging_api")]
    public class M_Logging_Api
    {
        public string Ma_sap { get; set; }
        public string Ma_phieu { get; set; }
        public string Url { get; set; }
        public string Dien_giai { get; set; }
        public DateTime Thoi_gian { get; set; }
        public string Id { get; set; }
    }
}
