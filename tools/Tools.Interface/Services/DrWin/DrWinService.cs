using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tools.Interface.Dtos.DRW;
using Tools.Interface.Helpers;
using Tools.Interface.Models;
using Tools.Interface.Models.DrWin;
using VCM.Common.Helpers;
using VCM.Shared.Entity.Central;
using WCM.EntityFrameworkCore.EntityFrameworkCore.DrWin;

namespace Tools.Interface.Services.DrWin
{
    public class DrWinService
    {
        private readonly InterfaceEntry _interfaceEntry;
        private DrWinRepository _drWinRepository;
        public DrWinService
            (
                InterfaceEntry interfaceEntry
            )
        {
            _interfaceEntry = interfaceEntry;
            _drWinRepository = new DrWinRepository(_interfaceEntry.Prefix);
        }
        private string GetAuthorizationBasic(WebApiInfo webApiInfo)
        {
            return webApiInfo.Description + " " + Convert.ToBase64String(Encoding.UTF8.GetBytes(webApiInfo.UserName + ":" + webApiInfo.Password));
        }
        public void GetSaleFromCentral(string job_name, string procedure)
        {
            FileHelper.Write2Logs(job_name, "===> Start GetSaleFromCentral: EXEC " + procedure);
            if (_drWinRepository.RunFromSqlRaw(job_name, procedure))
            {
                FileHelper.Write2Logs(job_name, "===> RunFromSqlRaw: OK");
            }
        }
        public void PostHoaDonThuoc_To_CucDuoc(string job_name, string procedure)
        {
            try
            {
                FileHelper.Write2Logs(job_name, "===> Start PostHoaDonThuoc_To_CucDuoc ");
                var lstHoadonRequest = _drWinRepository.GetHoaDonThuocRequest(job_name);
                if (lstHoadonRequest != null && lstHoadonRequest.Count > 0)
                {
                    var webApiInfo = _drWinRepository.GetDrwWebApiInfo(job_name);
                    if (webApiInfo == null)
                    {
                        return;
                    }
                    var routerData = webApiInfo.WebRoute.Where(x => x.Name == "PostHoaDonThuoc").FirstOrDefault();
                    Dictionary<string, string> headers = new Dictionary<string, string>
                    {
                        { "Authorization", GetAuthorizationBasic(webApiInfo) }
                    };
                    var router = routerData.Route;
                    FileHelper.Write2Logs(job_name, webApiInfo.Host + router);
                    foreach (var item in lstHoadonRequest)
                    {
                        if (!item.Is_Return)
                        {
                            FileHelper.Write2Logs(job_name, "===> Request: " + JsonConvert.SerializeObject(JsonConvert.SerializeObject(item)));
                            var api = new RestApiHelper(
                                        job_name,
                                        webApiInfo.Host,
                                        router,
                                        "POST",
                                        headers,
                                        null,
                                        JsonConvert.SerializeObject(item)
                                   );
                            var strResponse = api.InteractWithApi();
                            var result = JsonConvert.DeserializeObject<HoaDonThuocResponse>(strResponse);
                            if (result != null && result.Code == 200)
                            {
                                _drWinRepository.UpdateStatusHoaDonThuoc(job_name, result, item);
                                FileHelper.Write2Logs(job_name, "===> DONE UpdateStatusHoaDonThuoc: " + item.Ma_hoa_don);
                            }
                            else
                            {
                                FileHelper.Write2Logs(job_name, "===> ERROR: " + JsonConvert.SerializeObject(strResponse));
                                _drWinRepository.InsertLoggingApi(job_name, item.Ma_hoa_don, item.Ma_sap, JsonConvert.SerializeObject(strResponse), router);
                                if(result.Code == 501)
                                {
                                    FileHelper.WriteLogs("===> ERROR: Lỗi đăng nhập api, thoát vòng lặp đẩy hóa đơn sang cục Dược");
                                    break;
                                }
                            }
                        }
                        else
                        {
                            var routerDataDelete = webApiInfo.WebRoute.Where(x => x.Name == "DeleteHoaDonThuoc").FirstOrDefault();
                            var routeDelete = routerDataDelete.Route + "/" + item.Ma_sap + "/" + item.Ma_hoa_don_goc;
                            FileHelper.Write2Logs(job_name, webApiInfo.Host + routeDelete);
                            var api = new RestApiHelper(
                                    job_name,
                                    webApiInfo.Host,
                                    routeDelete,
                                    "DELETE",
                                    headers,
                                    null,
                                    null
                               );
                            var strResponse = api.InteractWithApi();
                            var result = JsonConvert.DeserializeObject<HoaDonThuocResponse>(strResponse);
                            if (result != null && result.Code == 200)
                            {
                                _drWinRepository.UpdateStatusHoaDonThuoc(job_name, result, item);
                                FileHelper.Write2Logs(job_name, "===> DONE UpdateStatusHoaDonThuoc: " + item.Ma_hoa_don);
                            }
                            else
                            {
                                _drWinRepository.InsertLoggingApi(job_name, item.Ma_hoa_don_goc, item.Ma_sap, JsonConvert.SerializeObject(strResponse), routeDelete);
                                FileHelper.Write2Logs(job_name, "===> ERROR: " + JsonConvert.SerializeObject(strResponse));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> PostHoaDonThuoc_To_CucDuoc Exception: " + ex.Message.ToString());
            }
        }
        public void ProcessInboundStock(string job_name, string pathLocal, string pathArchived, int number_of_process, string procedure)
        {
            try
            {
                List<M_ton_kho> lstData = new List<M_ton_kho>();
                var lstFile = FileHelper.GetFileFromDir(pathLocal, "*.xml");
                var count = 0;
                var itemMaster = _drWinRepository.GetItemDrw(job_name);
                if(lstFile.Count > 0)
                {
                    if (_drWinRepository.RunFromSqlRaw(job_name, procedure))
                    {
                        FileHelper.Write2Logs(job_name, "===> RunFromSqlRaw: OK");
                        foreach (string file in lstFile)
                        {
                            InboundDrwService.ReadXmlStockData(job_name, itemMaster, pathLocal + file, ref lstData);
                            if (lstData.Count > 0)
                            {
                                if (_drWinRepository.SaveTonKhoDrw(job_name, lstData))
                                {
                                    FileHelper.MoveFileToDestination(pathLocal + file, pathArchived);
                                    lstData.Clear();
                                }
                            }
                            count += 1;
                            if (count == number_of_process)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Write2Logs(job_name, "===> ProcessInboundStock Exception: " + ex.Message.ToString());
                
            }
        }
    }
}
