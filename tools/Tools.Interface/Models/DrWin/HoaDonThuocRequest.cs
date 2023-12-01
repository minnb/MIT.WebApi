using System;
using System.Collections.Generic;
using System.Text;
using VCM.Shared.Entity.DrWin;

namespace Tools.Interface.Models.DrWin
{
    public class HoaDonThuocRequest : Hoa_don_thuoc
    {
        public bool Is_Return { get; set; }
        public string Ma_hoa_don_goc { get; set; }
        public List<Hoa_don_chi_tiet> Hoa_don_chi_tiet { get; set; }
    }
    public class HoaDonThuocResponse
    {
        public string Ma_hoa_don_quoc_gia { get; set; }
        public int Code { get; set; }
        public string Mess { get; set; }
    }
}
