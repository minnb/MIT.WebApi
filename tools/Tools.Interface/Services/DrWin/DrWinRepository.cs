using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools.Interface.Database;
using Tools.Interface.Dtos.DRW;
using Tools.Interface.Helpers;
using Tools.Interface.Models;
using Tools.Interface.Models.DrWin;
using VCM.Common.Helpers;
using VCM.Shared.Entity.DrWin;

namespace Tools.Interface.Services.DrWin
{
    public class DrWinRepository
    {
        public DrWinDbContext _drwDbContext;
        public readonly string AppCodeDrWin = "DRW_JOB";
        private readonly string _connectString;
        public DrWinRepository
            (
                string connectString
            )
        {
            _connectString = connectString;
            _drwDbContext = new DrWinDbContext(_connectString);
        }

        public bool RunFromSqlRaw(string job_name, string procedure)
        {
            try
            {
                var ressult = _drwDbContext.QueryResultDRW.FromSqlRaw("EXEC " + procedure).ToList();
                if(ressult != null)
                {
                    if(ressult.FirstOrDefault().Result == "OK")
                    {
                        return true;
                    }
                    else
                    {
                        FileHelper.Write2Logs(job_name, "===> RunFromSqlRaw Exception: " + ressult.FirstOrDefault().Result??"eee");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> RunFromSqlRaw Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public List<ItemDrwDto> GetItemDrw(string job_name)
        {
            try
            {
                var items = _drwDbContext.ItemDrwDto.FromSqlRaw("EXEC SP_GET_MASTER_ITEM;").ToList();
                if (items != null && items.Count > 0)
                {
                    return items;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> GetHoaDonThuocRequest Exception: " + ex.Message.ToString());
                return null;
            }
        }
        public List<HoaDonThuocRequest> GetHoaDonThuocRequest(string job_name)
        {
            var hoaDonThuocRequest = new List<HoaDonThuocRequest>();
            try
            {
                var hoadonthuoc = _drwDbContext.M_hoa_don_thuoc.Where(x => x.Trang_thai == "N").ToList();
                if(hoadonthuoc!=null && hoadonthuoc.Count > 0)
                {
                    var listMaPhieu = hoadonthuoc.Select(x => x.Ma_hoa_don).ToArray();
                    var listChitietHoadon = _drwDbContext.M_hoa_don_chi_tiet.Where(x => listMaPhieu.Contains(x.Ma_hoa_don)).ToList();
                    foreach(var item in hoadonthuoc)
                    {
                        var hoa_don_chi_tiet = new List<Hoa_don_chi_tiet>();
                        foreach(var chitiet in listChitietHoadon.Where(x=>x.Ma_hoa_don == item.Ma_hoa_don).ToList())
                        {
                            hoa_don_chi_tiet.Add(new Hoa_don_chi_tiet()
                            {
                                Ma_thuoc = chitiet.Ma_thuoc,
                                Ten_thuoc = chitiet.Ten_thuoc,
                                So_lo = chitiet.So_lo,
                                Ngay_san_xuat = chitiet.Ngay_san_xuat??"",
                                Han_dung = chitiet.Han_dung, // StringHelper.ConvertStringToDate(chitiet.Han_dung, "ddMMyyyy").ToString("yyyyMMdd"),
                                Don_vi_tinh = chitiet.Don_vi_tinh,
                                Don_gia = chitiet.Don_gia,
                                Ham_luong = chitiet.Ham_luong,
                                So_luong = chitiet.So_luong,
                                Thanh_tien = chitiet.Thanh_tien,
                                Ty_le_quy_doi = chitiet.Ty_le_quy_doi,
                                Lieu_dung = !string.IsNullOrEmpty(chitiet.Lieu_dung) ? chitiet.Lieu_dung :"0",
                                Duong_dung = chitiet.Duong_dung
                            });
                        }

                        bool isReturn = false;
                        if (!string.IsNullOrEmpty(item.Ma_hoa_don_goc))
                        {
                            isReturn = true;
                        }
                        hoaDonThuocRequest.Add(new HoaDonThuocRequest()
                        {
                            Ma_hoa_don = item.Ma_hoa_don,
                            Ma_don_thuoc_quoc_gia = item.Ma_don_thuoc_quoc_gia,
                            Ma_co_so = item.Ma_co_so,
                            Ho_ten_nguoi_ban = item.Ho_ten_nguoi_ban,
                            Ho_ten_khach_hang = item.Ho_ten_khach_hang,
                            Ngay_ban = item.Ngay_ban,
                            Ma_sap = item.Ma_sap,
                            Is_Return = isReturn,
                            Ma_hoa_don_goc = item.Ma_hoa_don_goc,
                            Hoa_don_chi_tiet = hoa_don_chi_tiet,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> GetHoaDonThuocRequest Exception: " + ex.Message.ToString());
                return null;
            }
            return hoaDonThuocRequest;
        }
        public WebApiInfo GetDrwWebApiInfo(string job_name)
        {
            try
            {
                var header = _drwDbContext.SysWebApi.Where(x => x.AppCode == AppCodeDrWin && x.Blocked == false).FirstOrDefault();
                if (header != null)
                {
                    var detail = _drwDbContext.SysWebRoute.Where(x => x.AppCode == AppCodeDrWin && x.Blocked == false).ToList();
                    return WebApiInfoHelper.WebApiInfoResult(header, detail);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> GetDrwWebApiInfo Exception: " + ex.Message.ToString());
                return null;
            }
        }

        public async void InsertLoggingApi(string job_name, string OrderNo, string StoreNo, string message, string url)
        {
            var loggingApi = new WCM.EntityFrameworkCore.EntityFrameworkCore.DrWin.M_Logging_Api()
            {
                Ma_phieu = OrderNo,
                Ma_sap = StoreNo,
                Url = url,
                Thoi_gian = DateTime.Now,
                Dien_giai = message,
                Id = Guid.NewGuid().ToString()
            };
            try
            { 
                _drwDbContext.M_Logging_Api.Add(loggingApi);
                await _drwDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> InsertLoggingApi Exception: " + ex.Message.ToString());
                FileHelper.Write2Logs(job_name, "===> M_Logging_Api Exception: " + JsonConvert.SerializeObject(loggingApi));
            }
        }
        public bool UpdateStatusHoaDonThuoc(string job_name, HoaDonThuocResponse response, HoaDonThuocRequest hoaDonThuocRequest)
        {
            try
            {
                var data = _drwDbContext.M_hoa_don_thuoc.Where(x => x.Ma_hoa_don == hoaDonThuocRequest.Ma_hoa_don).FirstOrDefault();
                if (data != null)
                {
                    if(response.Code == 200)
                    {
                        data.Ma_don_thuoc_quoc_gia = response.Ma_hoa_don_quoc_gia??"";
                        data.Trang_thai = "Y";
                    }
                    else
                    {
                        data.Trang_thai = "E";
                    }
                    data.Dien_giai = response.Mess ?? "";
                    _drwDbContext.Update(data);
                    _drwDbContext.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> UpdateStatusHoaDonThuoc Exception: " + ex.Message.ToString());
                return false;
            }
        }
        public bool SaveTonKhoDrw(string job_name, List<WCM.EntityFrameworkCore.EntityFrameworkCore.DrWin.M_ton_kho> dataStock)
        {
            try
            {
                if (dataStock != null && dataStock.Count > 0)
                {
                    dataStock.ForEach(x=>_drwDbContext.Add(x));
                    _drwDbContext.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> SaveTonKhoDrw Exception: " + ex.Message.ToString());
                return false;
            }
        }
    }
}
