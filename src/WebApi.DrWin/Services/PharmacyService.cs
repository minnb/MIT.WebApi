using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using VCM.Shared.Dtos;
using VCM.Shared.Entity.DrWin;
using WebApi.DrWin.Models;
using WebApi.DrWin.Reposites;

namespace WebApi.DrWin.Services
{
    public interface IPharmacyService
    {
        Task<HoaDonRsp> Lien_thong_hoa_don_GPP(SysWebApiDto webApiInfo, HoaDonThuocModel hoaDonThuocModel, string token);
        Task<PhieuNhapRsp> Lien_thong_nhap_don_thuoc_GPP(SysWebApiDto webApiInfo, PhieuNhapThuocModel phieunhap, string token);
        Task<PhieuXuatRsp> Lien_thong_xuat_don_thuoc_GPP(SysWebApiDto webApiInfo, PhieuXuatThuocModel phieunhap, string token);
        Task<PhieuNhapRsp> Lien_thong_thuoc_co_so_them_thuoc_GPP(SysWebApiDto webApiInfo, Them_moi_thuoc_co_so request, string token);
        Task<DeleteRsp> Xoa_Lien_thong_hoa_don_ban_thuoc_GPP(SysWebApiDto webApiInfo, string ma_co_so, string ma_phieu, string token);
        Task<DeleteRsp> Xoa_Lien_thong_nhap_don_thuoc_GPP(SysWebApiDto webApiInfo, string ma_co_so, string ma_phieu, string token);
        Task<DeleteRsp> Xoa_Lien_thong_xuat_don_thuoc_GPP(SysWebApiDto webApiInfo, string ma_co_so, string ma_phieu, string token);
    }
    public class PharmacyService: IPharmacyService
    {
        private readonly ILogger<PharmacyService> _logger;
        private readonly IHoaDonReposites _hoaDonReposites;
        private readonly IApiService _apiService;
        private int ErrorApi = 4010;
        public PharmacyService(
            ILogger<PharmacyService> logger,
            IApiService apiService,
            IHoaDonReposites hoaDonReposites
        )
        {
            _logger = logger;
            _apiService = apiService;
            _hoaDonReposites = hoaDonReposites;
        }

        public async Task<HoaDonRsp> Lien_thong_hoa_don_GPP(SysWebApiDto webApiInfo, HoaDonThuocModel hoaDonThuocModel, string token)
        {
            string strResponse = "";
            HoaDonRsp hoaDonRsp = new HoaDonRsp();
            string function = "lien_thong_hoa_don";
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    strResponse = await _apiService.CallApiHttpWeb(webApiInfo,JsonConvert.SerializeObject(hoaDonThuocModel), null, function, token, "POST", hoaDonThuocModel.Ma_hoa_don);
                    _logger.LogWarning(function + ": " + strResponse);
                    if (!string.IsNullOrEmpty(strResponse))
                    {
                        hoaDonRsp = JsonConvert.DeserializeObject<HoaDonRsp>(strResponse);
                        if (hoaDonRsp != null && hoaDonRsp.Code == 200 && !string.IsNullOrEmpty(hoaDonRsp.Ma_hoa_don_quoc_gia))
                        {
                            _logger.LogWarning(function + ": " + hoaDonThuocModel.Ma_hoa_don + " ===> OK");
                            //await _hoaDonReposites.SaveHoaDonThuoc(hoaDonThuocModel, hoaDonRsp);
                        }
                        else
                        {
                            _logger.LogWarning(function + ": " + strResponse);
                            hoaDonRsp.Code = ErrorApi;
                            hoaDonRsp.Mess = strResponse != null ? JsonConvert.DeserializeObject<BadRequestCPP>(strResponse).Message.ToString() : "ERROR1";
                        }
                    }
                    else
                    {
                        _logger.LogWarning(function + ": " + strResponse);
                        hoaDonRsp.Code = ErrorApi;
                        hoaDonRsp.Mess = strResponse != null ? JsonConvert.DeserializeObject<BadRequestCPP>(strResponse).Message.ToString() : "ERROR1";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception: " + ex.Message.ToString());
                hoaDonRsp.Code = ErrorApi;
                hoaDonRsp.Mess = strResponse;
            }
            return hoaDonRsp;
        }
        public async Task<PhieuNhapRsp> Lien_thong_nhap_don_thuoc_GPP(SysWebApiDto webApiInfo, PhieuNhapThuocModel body, string token)
        {
            string strResponse = string.Empty;
            PhieuNhapRsp hoaDonRsp = new PhieuNhapRsp();
            string function = "lien_thong_phieu_nhap";
            if (string.IsNullOrEmpty(token))
            {
                hoaDonRsp.Code = 400;
                hoaDonRsp.Mess = "Lỗi đăng nhập api Cục Dược";
            }
            else
            {
                try
                {
                    _logger.LogWarning("request to Cuc Duoc: " + JsonConvert.SerializeObject(body).ToString());
                    strResponse = await _apiService.CallApiHttpWeb(webApiInfo, JsonConvert.SerializeObject(body).ToString(), null, function, token, "POST", body.Ma_phieu.ToString());
                    _logger.LogWarning("response to Cuc Duoc: " + strResponse);
                    if (!string.IsNullOrEmpty(strResponse))
                    {
                        hoaDonRsp = JsonConvert.DeserializeObject<PhieuNhapRsp>(strResponse);
                        if (hoaDonRsp != null && !string.IsNullOrEmpty(hoaDonRsp.Ma_phieu_nhap_quoc_gia))
                        {
                            await _hoaDonReposites.SavePhieuNhapThuoc(body, hoaDonRsp);
                        }
                        else
                        {
                            hoaDonRsp.Code = 400;
                            hoaDonRsp.Mess = strResponse;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Exception.Lien_thong_nhap_don_thuoc_GPP: " + ex.Message.ToString());
                    hoaDonRsp.Code = ErrorApi;
                    hoaDonRsp.Mess = strResponse;
                }
            }           
            return hoaDonRsp;
        }
        public async Task<PhieuNhapRsp> Lien_thong_thuoc_co_so_them_thuoc_GPP(SysWebApiDto webApiInfo, Them_moi_thuoc_co_so request, string token)
        {
            PhieuNhapRsp hoaDonRsp = new PhieuNhapRsp();
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    var strResponse = await _apiService.CallApiHttpWeb(webApiInfo,JsonConvert.SerializeObject(request), null, "lien_thong_thuoc_co_so_them_thuoc", token, "POST", request.Ma_sap + request.Ma_co_so);
                    if (!string.IsNullOrEmpty(strResponse))
                    {
                        _logger.LogWarning("Result: " + strResponse);
                        hoaDonRsp = JsonConvert.DeserializeObject<PhieuNhapRsp>(strResponse);
                    }
                    else
                    {
                        _logger.LogWarning("Result: " + strResponse);
                        hoaDonRsp.Code = ErrorApi;
                        hoaDonRsp.Mess = strResponse != null ? JsonConvert.DeserializeObject<BadRequestCPP>(strResponse).Message.ToString() : "ERROR1";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception: " + ex.Message.ToString());
                hoaDonRsp.Code = ErrorApi;
                hoaDonRsp.Mess = ex.Message.ToString();
            }
            return hoaDonRsp;
        }
        public async Task<PhieuXuatRsp> Lien_thong_xuat_don_thuoc_GPP(SysWebApiDto webApiInfo, PhieuXuatThuocModel data, string token)
        {
            string strResponse = string.Empty;
            PhieuXuatRsp hoaDonRsp = new PhieuXuatRsp();
            string function = "lien_thong_phieu_xuat";
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    strResponse = await _apiService.CallApiHttpWeb(webApiInfo, JsonConvert.SerializeObject(data), null, function, token, "POST", data.Ma_phieu);
                    if (!string.IsNullOrEmpty(strResponse))
                    {
                        hoaDonRsp = JsonConvert.DeserializeObject<PhieuXuatRsp>(strResponse);
                        if(hoaDonRsp != null && hoaDonRsp.Ma_phieu_xuat_quoc_gia != null)
                        {
                            await _hoaDonReposites.SavePhieuXuatThuoc(data, hoaDonRsp);
                        }
                        else
                        {
                            var mess = JsonConvert.DeserializeObject<BadRequestCPP>(strResponse);
                            _logger.LogWarning(function + ": " + strResponse);
                            hoaDonRsp.Code = ErrorApi;
                            hoaDonRsp.Mess = strResponse != null ? JsonConvert.DeserializeObject<BadRequestCPP>(strResponse).Message.ToString() : "ERROR1";
                        }
                    }
                    else
                    {
                        _logger.LogWarning(function + ": " + strResponse);
                        hoaDonRsp.Code = ErrorApi;
                        hoaDonRsp.Mess = strResponse != null ? JsonConvert.DeserializeObject<BadRequestCPP>(strResponse).Message.ToString() : "ERROR1";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception: " + ex.Message.ToString());
                hoaDonRsp.Code = ErrorApi;
                hoaDonRsp.Mess = strResponse;
            }
            return hoaDonRsp;
        }

        public async Task<DeleteRsp> Xoa_Lien_thong_hoa_don_ban_thuoc_GPP(SysWebApiDto webApiInfo, string ma_co_so, string ma_phieu, string token)
        {
            string strResponse = string.Empty;
            DeleteRsp rsp = new DeleteRsp();
            string function = "xoa_lien_thong_hoa_don_thuoc";
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    string param = "/" + ma_co_so + "/" + ma_phieu;

                    strResponse = await _apiService.CallApiHttpWeb(webApiInfo, null, param, function, token, "DELETE", ma_phieu);
                    if (!string.IsNullOrEmpty(strResponse))
                    {
                        rsp = JsonConvert.DeserializeObject<DeleteRsp>(strResponse);
                        if (rsp != null && rsp.Code == 200)
                        {
                            //await _hoaDonReposites.DeletePhieuNhapThuoc(ma_phieu);
                        }
                        else
                        {
                            var mess = JsonConvert.DeserializeObject<BadRequestCPP>(strResponse);
                            _logger.LogWarning(function + ": " + strResponse);
                            rsp.Code = ErrorApi;
                            rsp.Mess = strResponse != null ? JsonConvert.DeserializeObject<BadRequestCPP>(strResponse).Message.ToString() : "ERROR1";
                        }
                    }
                    else
                    {
                        _logger.LogWarning(function + ": " + strResponse);
                        rsp.Code = ErrorApi;
                        rsp.Mess = strResponse != null ? JsonConvert.DeserializeObject<BadRequestCPP>(strResponse).Message.ToString() : "ERROR1";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception: " + ex.Message.ToString());
                rsp.Code = ErrorApi;
                rsp.Mess = strResponse;
            }
            return rsp;
        }

        public async Task<DeleteRsp> Xoa_Lien_thong_nhap_don_thuoc_GPP(SysWebApiDto webApiInfo, string ma_co_so, string ma_phieu, string token)
        {
            string strResponse = string.Empty;
            DeleteRsp rsp = new DeleteRsp();
            string function = "xoa_lien_thong_phieu_nhap";
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    string param = "/" + ma_co_so + "/" + ma_phieu;
                    _logger.LogWarning("===> start request: " + param);
                    strResponse = await _apiService.CallApiHttpWeb(webApiInfo, null, param, function, token, "DELETE", ma_phieu);

                    _logger.LogWarning(function + ": " + strResponse);
                    if (!string.IsNullOrEmpty(strResponse))
                    {
                        rsp = JsonConvert.DeserializeObject<DeleteRsp>(strResponse);
                        if (rsp != null && rsp.Code == 200)
                        {
                            //await _hoaDonReposites.DeletePhieuNhapThuoc(ma_phieu);
                        }
                        else
                        {
                            var mess = JsonConvert.DeserializeObject<BadRequestCPP>(strResponse);
                            _logger.LogWarning(function + ": " + strResponse);
                            rsp.Code = ErrorApi;
                            rsp.Mess = strResponse != null ? JsonConvert.DeserializeObject<BadRequestCPP>(strResponse).Message.ToString() : "ERROR1";
                        }
                    }
                    else
                    {
                        rsp.Code = ErrorApi;
                        rsp.Mess = strResponse != null ? JsonConvert.DeserializeObject<BadRequestCPP>(strResponse).Message.ToString() : "ERROR1";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception: " + ex.Message.ToString());
                rsp.Code = ErrorApi;
                rsp.Mess = strResponse;
            }
            return rsp;
        }
        public async Task<DeleteRsp> Xoa_Lien_thong_xuat_don_thuoc_GPP(SysWebApiDto webApiInfo, string ma_co_so, string ma_phieu, string token)
        {
            string strResponse = string.Empty;
            DeleteRsp rsp = new DeleteRsp();
            string function = "xoa_lien_thong_phieu_xuat";
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    string param = "/" + ma_co_so + "/" + ma_phieu;

                    strResponse = await _apiService.CallApiHttpWeb(webApiInfo, null, param, function, token, "DELETE", ma_phieu);
                    if (!string.IsNullOrEmpty(strResponse))
                    {
                        rsp = JsonConvert.DeserializeObject<DeleteRsp>(strResponse);
                        if (rsp != null && rsp.Code == 200)
                        {
                            //await _hoaDonReposites.DeletePhieuXuatThuoc(ma_phieu);
                        }
                        else
                        {
                            var mess = JsonConvert.DeserializeObject<BadRequestCPP>(strResponse);
                            _logger.LogWarning(function + ": " + strResponse);
                            rsp.Code = ErrorApi;
                            rsp.Mess = strResponse != null ? JsonConvert.DeserializeObject<BadRequestCPP>(strResponse).Message.ToString() : "ERROR1";
                        }
                    }
                    else
                    {
                        _logger.LogWarning(function + ": " + strResponse);
                        rsp.Code = ErrorApi;
                        rsp.Mess = strResponse != null ? JsonConvert.DeserializeObject<BadRequestCPP>(strResponse).Message.ToString() : "ERROR1";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception: " + ex.Message.ToString());
                rsp.Code = ErrorApi;
                rsp.Mess = strResponse;
            }
            return rsp;
        }
    }
}
