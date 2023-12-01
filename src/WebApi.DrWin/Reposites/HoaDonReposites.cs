using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WCM.EntityFrameworkCore.EntityFrameworkCore.DrWin;
using WebApi.DrWin.Models;

namespace WebApi.DrWin.Reposites
{
    public interface IHoaDonReposites
    {
        Task<bool> DeletePhieuXuatThuoc(string Ma_phieu);
        Task<bool> DeletePhieuNhapThuoc(string Ma_phieu);
        Task<bool> SavePhieuNhapThuoc(PhieuNhapThuocModel data, PhieuNhapRsp rsp);
        Task<bool> SavePhieuXuatThuoc(PhieuXuatThuocModel data, PhieuXuatRsp rsp);
        Task<bool> SaveHoaDonThuoc(HoaDonThuocModel data, HoaDonRsp rsp);
        List<HoaDonThuocModel> GetListHoaDonThuoc(string UpdateFlg);
    }
    public class HoaDonReposites: IHoaDonReposites
    {
        private readonly ILogger<HoaDonReposites> _logger;
        private readonly DrWinDbContext _dbContext;
        public HoaDonReposites(
            ILogger<HoaDonReposites> logger,
            DrWinDbContext dbContext
        )
        {
            _logger = logger;
            _dbContext = dbContext; 
        }

        public List<HoaDonThuocModel> GetListHoaDonThuoc(string UpdateFlg)
        {

            throw new System.NotImplementedException();
        }

        public async Task<bool> SavePhieuNhapThuoc(PhieuNhapThuocModel data, PhieuNhapRsp rsp)
        {
            try
            {
                var flag = "N";
                if (!string.IsNullOrEmpty(rsp.Ma_phieu_nhap_quoc_gia))
                {
                    flag = "Y";
                }
                var checkData = _dbContext.M_Phieu_nhap_thuoc.Where(x => x.Ma_phieu == data.Ma_phieu).FirstOrDefault();
                if(checkData != null)
                {
                    _logger.LogWarning("===> UpdatePhieuNhapThuoc: " + rsp.Mess ?? "");
                    checkData.Dien_giai = rsp.Mess ?? "";
                    checkData.Trang_thai = flag;
                    _dbContext.Update(checkData);
                }
                else
                {
                    var PhieuNhapHeader = new M_Phieu_nhap_thuoc()
                    {
                        Ma_sap = data.Ma_sap,
                        Ma_co_so = data.Ma_co_so,
                        Ma_phieu = data.Ma_phieu,
                        Ngay_nhap = data.Ngay_nhap,
                        Loai_phieu_nhap = data.Loai_phieu_nhap,
                        Ghi_chu = data.Ghi_chu ?? "",
                        Ten_co_so_cung_cap = data.Ten_co_so_cung_cap ?? "",
                        Ma_phieu_nhap_quoc_gia = rsp.Ma_phieu_nhap_quoc_gia ?? "",
                        Ngay_gio_tao = DateTime.Now,
                        Dien_giai = rsp.Mess ?? ""
                    };
                    if (string.IsNullOrEmpty(rsp.Ma_phieu_nhap_quoc_gia))
                    {
                        PhieuNhapHeader.Trang_thai = "N";
                    }
                    else
                    {
                        PhieuNhapHeader.Trang_thai = "Y";
                    }
                    List<M_Phieu_nhap_thuoc_chi_tiet> chitiet = new List<M_Phieu_nhap_thuoc_chi_tiet>();
                    foreach (var item in data.Chi_tiet)
                    {
                        chitiet.Add(new M_Phieu_nhap_thuoc_chi_tiet()
                        {
                            Ma_sap = data.Ma_sap,
                            Ma_phieu = data.Ma_phieu,
                            Ma_thuoc = item.Ma_thuoc,
                            Ten_thuoc = item.Ten_thuoc,
                            So_dklh = item.So_dklh,
                            Ngay_san_xuat = item.Ngay_san_xuat,
                            Han_dung = item.Han_dung,
                            So_lo = item.So_lo,
                            So_luong = item.So_luong,
                            Don_gia = item.Don_gia,
                            Don_vi_tinh = item.Don_vi_tinh
                        });
                    }

                    chitiet.ForEach(x => _dbContext.M_Phieu_nhap_thuoc_chi_tiet.Add(x));

                    _logger.LogWarning("===> InsertPhieuNhapThuoc: " + JsonConvert.SerializeObject(PhieuNhapHeader));

                    _dbContext.M_Phieu_nhap_thuoc.Add(PhieuNhapHeader);
                }
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> SavePhieuNhapThuoc.Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public async Task<bool> SavePhieuXuatThuoc(PhieuXuatThuocModel data, PhieuXuatRsp rsp)
        {
            try
            {
                var flag = "N";
                if (!string.IsNullOrEmpty(rsp.Ma_phieu_xuat_quoc_gia))
                {
                    flag = "Y";
                }
                var checkData = _dbContext.M_Phieu_xuat_thuoc.Where(x => x.Ma_phieu == data.Ma_phieu).FirstOrDefault();
                if (checkData != null)
                {
                    checkData.Dien_giai = rsp.Mess ?? "";
                    checkData.Trang_thai = flag;
                    _dbContext.Update(checkData);
                }
                else
                {
                    var Header = new M_Phieu_xuat_thuoc()
                    {
                        Ma_sap = data.Ma_sap,
                        Ma_co_so = data.Ma_co_so,
                        Ma_phieu = data.Ma_phieu,
                        Ngay_xuat = data.Ngay_xuat,
                        Loai_phieu_xuat = data.Loai_phieu_xuat,
                        Ghi_chu = data.Ghi_chu ?? "",
                        Ten_co_so_nhan = data.Ten_co_so_nhan ?? "",
                        Ma_phieu_xuat_quoc_gia = rsp.Ma_phieu_xuat_quoc_gia ?? "",
                        Ngay_gio_tao = DateTime.Now,
                        Dien_giai = rsp.Mess ?? ""
                    };
                    if (string.IsNullOrEmpty(rsp.Ma_phieu_xuat_quoc_gia))
                    {
                        Header.Trang_thai = "N";
                    }
                    else
                    {
                        Header.Trang_thai = "Y";
                    }
                    List<M_Phieu_xuat_thuoc_chi_tiet> chitiet = new List<M_Phieu_xuat_thuoc_chi_tiet>();
                    foreach (var item in data.Chi_tiet)
                    {
                        chitiet.Add(new M_Phieu_xuat_thuoc_chi_tiet()
                        {
                            Ma_sap = data.Ma_sap,
                            Ma_phieu = data.Ma_phieu,
                            Ma_thuoc = item.Ma_thuoc,
                            Ten_thuoc = item.Ten_thuoc,
                            So_dklh = item.So_dklh,
                            Ngay_san_xuat = item.Ngay_san_xuat,
                            Han_dung = item.Han_dung,
                            So_lo = item.So_lo,
                            So_luong = item.So_luong,
                            Don_gia = item.Don_gia,
                            Don_vi_tinh = item.Don_vi_tinh
                        });
                    }

                    chitiet.ForEach(x => _dbContext.M_Phieu_xuat_thuoc_chi_tiet.Add(x));
                    _dbContext.M_Phieu_xuat_thuoc.Add(Header);
                }
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> SavePhieuXuatThuoc.Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public async Task<bool> SaveHoaDonThuoc(HoaDonThuocModel data, HoaDonRsp rsp)
        {
            try
            {
                var flag = "N";
                if (!string.IsNullOrEmpty(rsp.Ma_hoa_don_quoc_gia))
                {
                    flag = "Y";
                }
                var checkData = _dbContext.M_hoa_don_thuoc.Where(x=>x.Ma_hoa_don == data.Ma_hoa_don).FirstOrDefault();
                if(checkData != null)
                {
                    checkData.Trang_thai = flag;
                    checkData.Dien_giai = "Code: " + rsp.Code.ToString() + "-"  + rsp.Mess ?? "";
                    _dbContext.Update(checkData);
                }
                else
                {
                    var Header = new M_hoa_don_thuoc()
                    {
                        Ma_sap = data.Ma_sap,
                        Ma_co_so = data.Ma_co_so,
                        Ma_hoa_don = data.Ma_hoa_don,
                        Ngay_ban = data.Ngay_ban,
                        Ma_don_thuoc_quoc_gia = rsp.Ma_hoa_don_quoc_gia,
                        Ho_ten_khach_hang = data.Ho_ten_khach_hang,
                        Ho_ten_nguoi_ban = data.Ho_ten_nguoi_ban,
                        Ngay_gio_tao = DateTime.Now,
                        Dien_giai = rsp.Mess ?? "",
                        Trang_thai = flag
                    };

                    List<M_hoa_don_chi_tiet> chitiet = new List<M_hoa_don_chi_tiet>();
                    foreach (var item in data.Hoa_don_chi_tiet)
                    {
                        chitiet.Add(new M_hoa_don_chi_tiet()
                        {
                            Ma_sap = data.Ma_sap,
                            Ma_hoa_don = data.Ma_hoa_don,
                            Ma_thuoc = item.Ma_thuoc,
                            Ten_thuoc = item.Ten_thuoc,
                            Ngay_san_xuat = item.Ngay_san_xuat,
                            Han_dung = item.Han_dung,
                            So_lo = item.So_lo,
                            So_luong = item.So_luong,
                            Don_gia = item.Don_gia,
                            Thanh_tien = item.Thanh_tien,
                            Don_vi_tinh = item.Don_vi_tinh,
                            Ham_luong = item.Ham_luong,
                            Lieu_dung = item.Lieu_dung,
                            Duong_dung = item.Duong_dung
                        });
                    }

                    chitiet.ForEach(x => _dbContext.M_hoa_don_chi_tiet.Add(x));
                    _dbContext.M_hoa_don_thuoc.Add(Header);
                }
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> SaveHoaDonThuoc.Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public async Task<bool> DeletePhieuXuatThuoc(string Ma_phieu)
        {
            try
            {
                var data = _dbContext.M_Phieu_xuat_thuoc.Where(x => x.Ma_phieu == Ma_phieu).FirstOrDefault();

                var Header = new M_Phieu_xuat_thuoc()
                {
                    Ma_sap = data.Ma_sap,
                    Ma_co_so = data.Ma_co_so,
                    Ma_phieu = "DEL_" + data.Ma_phieu,
                    Ngay_xuat = data.Ngay_xuat,
                    Loai_phieu_xuat = data.Loai_phieu_xuat,
                    Ghi_chu = data.Ghi_chu ?? "",
                    Ten_co_so_nhan = data.Ten_co_so_nhan ?? "",
                    Ma_phieu_xuat_quoc_gia = data.Ma_phieu_xuat_quoc_gia ?? "",
                    Ngay_gio_tao = DateTime.Now,
                    Dien_giai = "Hủy phiếu Xuất",
                    Trang_thai = "Y"
                };

                var dataChitiet = _dbContext.M_Phieu_xuat_thuoc_chi_tiet.Where(x=>x.Ma_phieu == Ma_phieu).ToList();
                List<M_Phieu_xuat_thuoc_chi_tiet> chitiet = new List<M_Phieu_xuat_thuoc_chi_tiet>();
                foreach (var item in dataChitiet)
                {
                    chitiet.Add(new M_Phieu_xuat_thuoc_chi_tiet()
                    {
                        Ma_sap = item.Ma_sap,
                        Ma_phieu = "DEL_" + item.Ma_phieu,
                        Ma_thuoc = item.Ma_thuoc,
                        Ten_thuoc = item.Ten_thuoc,
                        So_dklh = item.So_dklh,
                        Ngay_san_xuat = item.Ngay_san_xuat,
                        Han_dung = item.Han_dung,
                        So_lo = item.So_lo,
                        So_luong = item.So_luong * (-1),
                        Don_gia = item.Don_gia,
                        Don_vi_tinh = item.Don_vi_tinh
                    });
                }

                chitiet.ForEach(x => _dbContext.M_Phieu_xuat_thuoc_chi_tiet.Add(x));
                _dbContext.M_Phieu_xuat_thuoc.Add(Header);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> DeletePhieuXuatThuoc.Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public async Task<bool> DeletePhieuNhapThuoc(string Ma_phieu)
        {
            try
            {
                var data = _dbContext.M_Phieu_nhap_thuoc.Where(x => x.Ma_phieu == Ma_phieu).FirstOrDefault();
                var PhieuNhapHeader = new M_Phieu_nhap_thuoc()
                {
                    Ma_sap = data.Ma_sap,
                    Ma_co_so = data.Ma_co_so,
                    Ma_phieu = "DEL_" + data.Ma_phieu,
                    Ngay_nhap = data.Ngay_nhap,
                    Loai_phieu_nhap = data.Loai_phieu_nhap,
                    Ghi_chu = data.Ghi_chu ?? "",
                    Ten_co_so_cung_cap = data.Ten_co_so_cung_cap ?? "",
                    Ma_phieu_nhap_quoc_gia = data.Ma_phieu_nhap_quoc_gia ?? "",
                    Ngay_gio_tao = DateTime.Now,
                    Dien_giai = "Hủy phiếu nhập",
                    Trang_thai = "Y"
                };

                List<M_Phieu_nhap_thuoc_chi_tiet> chitiet = new List<M_Phieu_nhap_thuoc_chi_tiet>();
                var dataChitiet = _dbContext.M_Phieu_nhap_thuoc_chi_tiet.Where(x => x.Ma_phieu == Ma_phieu).ToList();
                foreach (var item in dataChitiet)
                {
                    chitiet.Add(new M_Phieu_nhap_thuoc_chi_tiet()
                    {
                        Ma_sap = item.Ma_sap,
                        Ma_phieu = "DEL_" + item.Ma_phieu,
                        Ma_thuoc = item.Ma_thuoc,
                        Ten_thuoc = item.Ten_thuoc,
                        So_dklh = item.So_dklh,
                        Ngay_san_xuat = item.Ngay_san_xuat,
                        Han_dung = item.Han_dung,
                        So_lo = item.So_lo,
                        So_luong = item.So_luong * (-1),
                        Don_gia = item.Don_gia,
                        Don_vi_tinh = item.Don_vi_tinh
                    });
                }

                chitiet.ForEach(x => _dbContext.M_Phieu_nhap_thuoc_chi_tiet.Add(x));
                _dbContext.M_Phieu_nhap_thuoc.Add(PhieuNhapHeader);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("===> DeletePhieuNhapThuoc.Exception: " + ex.Message.ToString());
                return false;
            }
        }
    }
}
