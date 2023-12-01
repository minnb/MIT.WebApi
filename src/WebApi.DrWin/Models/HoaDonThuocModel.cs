using System.Collections.Generic;
using VCM.Shared.Entity.DrWin;

namespace WebApi.DrWin.Models
{
    public class HoaDonThuocModel: Hoa_don_thuoc
    {
        public List<Hoa_don_chi_tiet> Hoa_don_chi_tiet { get; set; }
    }

    public class HoaDonRsp
    {
        public string Ma_hoa_don_quoc_gia { get; set; }
        public int Code { get; set; }

        public string Mess { get; set; }
    }
    public class PhieuNhapRsp
    {
        public string Ma_phieu_nhap_quoc_gia { get; set; }
        public int Code { get; set; }

        public string Mess { get; set; }
    }
    public class PhieuXuatRsp
    {
        public string Ma_phieu_xuat_quoc_gia { get; set; }
        public int Code { get; set; }

        public string Mess { get; set; }
    }
    public class DeleteRsp
    {
        public int Code { get; set; }
        public string Mess { get; set; }
    }
}
