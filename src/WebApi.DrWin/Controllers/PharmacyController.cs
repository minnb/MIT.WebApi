using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VCM.Shared.Entity.DrWin;
using WebApi.DrWin.Models;
using WebApi.DrWin.Services;

namespace WebApi.DrWin.Controllers
{
    public class PharmacyController:BaseController
    {
        private readonly ILogger<PharmacyController> _logger;
        private readonly IPharmacyService _pharmacyService;
        private readonly IRedisService _redisService;
        public PharmacyController(
              ILogger<PharmacyController> logger,
              IPharmacyService pharmacyService,
              IRedisService redisService
            )
        {
            _logger = logger;
            _pharmacyService = pharmacyService;
            _redisService = redisService;
        }

        [HttpPost]
        [Route("api/lien-thong/hoa-don")]
        //[PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<HoaDonRsp> HoaDonThuocGPP([FromBody] HoaDonThuocModel hoaDonThuocModel)
        {
            if (ModelState.IsValid)
            {
                _logger.LogWarning("===> request: " + JsonConvert.SerializeObject(hoaDonThuocModel));
                HoaDonRsp responseObject = new HoaDonRsp();

                var _webApiAirPayInfo = _redisService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == "DWN").SingleOrDefault();

                var token = await _redisService.GetTokenAsync(_webApiAirPayInfo, hoaDonThuocModel.Ma_sap);
                if(token != null)
                {
                    hoaDonThuocModel.Ma_co_so = token.Ma_co_so; ;
                    return await _pharmacyService.Lien_thong_hoa_don_GPP(_webApiAirPayInfo, hoaDonThuocModel, token.Token);
                }
                else
                {
                    return new HoaDonRsp()
                    {
                        Code = 501,
                        Ma_hoa_don_quoc_gia="",
                        Mess="Lỗi đăng nhập api"
                    };
                }
            }
            else
            {
                return new HoaDonRsp()
                {
                    Code = 501,
                    Ma_hoa_don_quoc_gia = "",
                    Mess = ModelState.Values.First().Errors[0].ErrorMessage.ToString()
                };
            }
        }


        [HttpDelete]
        [Route("api/lien-thong/hoa-don/{ma_sap}/{ma_phieu}")]
        //[PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<DeleteRsp> XoaHoaDonBanThuocGPP([Required] string ma_sap, [Required] string ma_phieu)
        {
            if (ModelState.IsValid)
            {
                HoaDonRsp responseObject = new HoaDonRsp();

                var _webApiAirPayInfo = _redisService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == "DWN").SingleOrDefault();

                var token = await _redisService.GetTokenAsync(_webApiAirPayInfo, ma_sap);
                if (token != null)
                {
                    string ma_co_so = token.Ma_co_so;
                    return await _pharmacyService.Xoa_Lien_thong_hoa_don_ban_thuoc_GPP(_webApiAirPayInfo, ma_co_so, ma_phieu, token.Token);
                }
                else
                {
                    return new DeleteRsp()
                    {
                        Code = 501,
                        Mess = "Lỗi đăng nhập api"
                    };
                }

            }
            else
            {
                return new DeleteRsp()
                {
                    Code = 501,
                    Mess = ModelState.Values.First().Errors[0].ErrorMessage.ToString()
                };
            }
        }


        [HttpPost]
        [Route("api/lien-thong/phieu-nhap-thuoc")]
        //[PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<PhieuNhapRsp> NhapThuocGPP([FromBody] PhieuNhapThuocModel request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var checkTaiKhoan = _redisService.GetTaiKhoanKetNoiCD().Result;
                    if(checkTaiKhoan.Where(x=>x.Ma_sap == request.Ma_sap).FirstOrDefault() == null)
                    {
                        _logger.LogWarning("===> warring: " + request.Ma_sap + " chưa được cấp tài khoản từ Cục Dược hoặc chưa khai báo");
                        return new PhieuNhapRsp()
                        {
                            Code = 400,
                            Mess = request.Ma_sap + " chưa được cấp tài khoản từ Cục Dược hoặc chưa khai báo"
                        };
                    }

                    _logger.LogWarning("===> phieu-nhap-thuoc request: " + JsonConvert.SerializeObject(request));
                    HoaDonRsp responseObject = new HoaDonRsp();

                    var _webApiAirPayInfo = _redisService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == "DWN").SingleOrDefault();

                    var token = await _redisService.GetTokenAsync(_webApiAirPayInfo, request.Ma_sap);
                    if (token!= null && !string.IsNullOrEmpty(token.Token))
                    {
                        request.Ma_co_so = token.Ma_co_so;
                        return await _pharmacyService.Lien_thong_nhap_don_thuoc_GPP(_webApiAirPayInfo, request, token.Token);
                    }
                    else
                    {
                        return new PhieuNhapRsp()
                        {
                            Code = 400,
                            Mess = request.Ma_sap + " lỗi đăng nhập api"
                        };
                    }

                }
                else
                {
                    return new PhieuNhapRsp()
                    {
                        Code = 501,
                        Mess = ModelState.Values.First().Errors[0].ErrorMessage.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                return new PhieuNhapRsp()
                {
                    Code = 400,
                    Mess = ex.Message.ToString()
                };
            }
        }

        [HttpDelete]
        [Route("api/lien-thong/phieu-nhap-thuoc/{ma_sap}/{ma_phieu}")]
        //[PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<DeleteRsp> XoaNhapThuocGPP([Required] string ma_sap, [Required] string ma_phieu)
        {
            if (ModelState.IsValid)
            {
                //_logger.LogWarning("===> request: " + JsonConvert.SerializeObject(request));
                HoaDonRsp responseObject = new HoaDonRsp();

                var _webApiAirPayInfo = _redisService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == "DWN").SingleOrDefault();

                var token = await _redisService.GetTokenAsync(_webApiAirPayInfo, ma_sap);
                if (token != null)
                {
                    string ma_co_so = token.Ma_co_so;
                    return await _pharmacyService.Xoa_Lien_thong_nhap_don_thuoc_GPP(_webApiAirPayInfo, ma_co_so, ma_phieu, token.Token);
                }
                else
                {
                    return new DeleteRsp()
                    {
                        Code = 501,
                        Mess = "Lỗi đăng nhập api"
                    };
                }

            }
            else
            {
                return new DeleteRsp()
                {
                    Code = 501,
                    Mess = ModelState.Values.First().Errors[0].ErrorMessage.ToString()
                };
            }
        }

        [HttpPost]
        [Route("api/lien-thong/phieu-xuat-thuoc")]
        //[PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<PhieuXuatRsp> PhieuXuatThuocGPP([FromBody] PhieuXuatThuocModel request)
        {
            if (ModelState.IsValid)
            {
                _logger.LogWarning("===> request: " + JsonConvert.SerializeObject(request));
                var _webApiAirPayInfo = _redisService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == "DWN").SingleOrDefault();

                var token = await _redisService.GetTokenAsync(_webApiAirPayInfo, request.Ma_sap);
                if (token != null)
                {
                    request.Ma_co_so = token.Ma_co_so;
                    return await _pharmacyService.Lien_thong_xuat_don_thuoc_GPP(_webApiAirPayInfo, request, token.Token);
                }
                else
                {
                    return new PhieuXuatRsp()
                    {
                        Code = 501,
                        Mess = "Lỗi đăng nhập api"
                    };
                }

            }
            else
            {
                return new PhieuXuatRsp()
                {
                    Code = 501,
                    Mess = ModelState.Values.First().Errors[0].ErrorMessage.ToString()
                };
            }
        }

        [HttpDelete]
        [Route("api/lien-thong/phieu-xuat-thuoc/{ma_sap}/{ma_phieu}")]
        //[PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<DeleteRsp> XoaXuatThuocGPP([Required] string ma_sap, [Required] string ma_phieu)
        {
            if (ModelState.IsValid)
            {
                //_logger.LogWarning("===> request: " + JsonConvert.SerializeObject(request));
                HoaDonRsp responseObject = new HoaDonRsp();

                var _webApiAirPayInfo = _redisService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == "DWN").SingleOrDefault();

                var token = await _redisService.GetTokenAsync(_webApiAirPayInfo, ma_sap);
                if (token != null)
                {
                    string ma_co_so = token.Ma_co_so;
                    return await _pharmacyService.Xoa_Lien_thong_xuat_don_thuoc_GPP(_webApiAirPayInfo, ma_co_so, ma_phieu, token.Token);
                }
                else
                {
                    return new DeleteRsp()
                    {
                        Code = 501,
                        Mess = "Lỗi đăng nhập api"
                    };
                }

            }
            else
            {
                return new DeleteRsp()
                {
                    Code = 501,
                    Mess = ModelState.Values.First().Errors[0].ErrorMessage.ToString()
                };
            }
        }

        [HttpPost]
        [Route("api/lien-thong/thuoc-co-so/them-thuoc")]
        //[PermissionAttribute(new[] { PermissionEnum.ADMIN, PermissionEnum.ALL, PermissionEnum.POS })]
        public async Task<PhieuNhapRsp> TaoMoiMaThuocGPP([FromBody] Them_moi_thuoc_co_so request)
        {
            if (ModelState.IsValid)
            {
                _logger.LogWarning("===> request: " + JsonConvert.SerializeObject(request));
                HoaDonRsp responseObject = new HoaDonRsp();

                var _webApiAirPayInfo = _redisService.GetDataWebApiAsync().Result?.Where(x => x.AppCode == "DWN").SingleOrDefault();

                var token = await _redisService.GetTokenAsync(_webApiAirPayInfo, request.Ma_sap);
                if (token != null)
                {
                    request.Ma_co_so = token.Ma_co_so;
                    return await _pharmacyService.Lien_thong_thuoc_co_so_them_thuoc_GPP(_webApiAirPayInfo, request, token.Token);
                }
                else
                {
                    return new PhieuNhapRsp()
                    {
                        Code = 501,
                        Mess = "Lỗi đăng nhập api"
                    };
                }

            }
            else
            {
                return new PhieuNhapRsp()
                {
                    Code = 501,
                    Mess = ModelState.Values.First().Errors[0].ErrorMessage.ToString()
                };
            }
        }
    }
}
