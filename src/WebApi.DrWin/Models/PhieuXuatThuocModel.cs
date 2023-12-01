using System.Collections.Generic;
using VCM.Shared.Entity.DrWin;

namespace WebApi.DrWin.Models
{
    public class PhieuXuatThuocModel: Phieu_xuat_thuoc
    {
        public List<Phieu_xuat_thuoc_chi_tiet> Chi_tiet { get; set; }
    }
}
