using System.Collections.Generic;
using VCM.Shared.Entity.DrWin;

namespace WebApi.DrWin.Models
{
    public class PhieuNhapThuocModel: Phieu_nhap_thuoc
    {
        public List<Phieu_nhap_thuoc_chi_tiet> Chi_tiet { get; set; }

    }
}
