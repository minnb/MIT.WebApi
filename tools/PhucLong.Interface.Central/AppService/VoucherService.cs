using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhucLong.Interface.Central.Database;
using PhucLong.Interface.Central.Models.ApiModel.CRX;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using VCM.Common.Helpers;
using VCM.Shared.Entity.Central;
using VCM.Shared.Entity.PhucLong;
using VCM.Shared.Entity.PhucLong.Dtos;

namespace PhucLong.Interface.Central.AppService
{
    public class VoucherService
    {
        private IConfiguration _configuration;
        private DapperCentral _dapperContext;
        public VoucherService
           (
               IConfiguration config
           )
        {
            _configuration = config;
            _dapperContext = new DapperCentral(_configuration);
        }
        public void UpdateStatusVoucherToCX(string procedure)
        {
            try
            {
                FileHelper.WriteLogs("===> Start update status voucher To CX");
                using IDbConnection conn = _dapperContext.ConnCentralPhucLong;
                conn.Open();
                WebApiService webApiService = new WebApiService(_configuration);
                var lstWebApi = webApiService.GetWebApiInfo(conn, "CRX");
                var apiVoucher = lstWebApi.WebRoute.Where(x => x.Name == "UpdateStatusVoucher").FirstOrDefault();
                if (lstWebApi != null && apiVoucher != null)
                {
                    var lstVoucher = conn.Query<VoucherInfoDto>(@"EXEC " + procedure).ToList();
                    List<VoucherCRX> voucherList = new List<VoucherCRX>();

                    FileHelper.WriteLogs("Voucher: " + lstVoucher.Count.ToString());
                    FileHelper.WriteLogs("Url: " + lstWebApi.Host + apiVoucher.Route);
                    if (lstVoucher.Count > 0)
                    {
                        Dictionary<string, string> Headers = new Dictionary<string, string>
                        {
                            { "Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(lstWebApi.UserName + ":" + lstWebApi.Password))}
                        };

                        foreach (var item in lstVoucher)
                        {
                            voucherList.Add(new VoucherCRX()
                            {
                                VoucherSerial = item.serial_number,
                                Status = "USED",
                                UpdatedDate = (long)DateTimeOffset.Now.ToUnixTimeMilliseconds()
                            });

                            //FileHelper.WriteLogs("Request: " + JsonConvert.SerializeObject(bodyData));
                            VoucherStock bodyData = new VoucherStock()
                            {
                                MerchantId = "PLH",
                                ChannelId = "POS",
                                VoucherList = voucherList
                            };

                            RestShapHelper apiHelper = new RestShapHelper(
                                       lstWebApi.Host,
                                       apiVoucher.Route,
                                       "PUT",
                                       Headers,
                                       null,
                                       bodyData
                                   );
                            string mess_error = "";
                            int statusRsp = 0;
                            var result = apiHelper.InteractWithApi(ref statusRsp, ref mess_error);

                            //FileHelper.WriteLogs("Response: " + result);

                            RspVoucherCRX rsp = JsonConvert.DeserializeObject<RspVoucherCRX>(result);

                            if (rsp != null)
                            {
                                if (statusRsp == 200 && (rsp.Data.ErrorCode == 0 || rsp.Data.ErrorCode == 1))
                                {
                                    UpdateStatusVoucher(conn, item, "Y", rsp.Data.Message.ToString());
                                    FileHelper.WriteLogs("DONE: " + JsonConvert.SerializeObject(rsp));
                                }
                                else 
                                {
                                    UpdateStatusVoucher(conn, item, "E", JsonConvert.SerializeObject(rsp.Data));
                                }
                            }
                            else
                            {
                                FileHelper.WriteLogs("Failed");
                                UpdateStatusVoucher(conn, item, "E", mess_error);
                            }
                            Thread.Sleep(100);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("UpdateStatusVoucherToCX Exception" + ex.Message.ToString());
            }
        }
        public void SaveVoucherInfo(string pathLocal, string pathArchive, string pathError, int maxFile, bool isMoveFile)
        {
            var lstFile = FileHelper.GetFileFromDir(pathLocal, "*.txt");
            FileHelper.WriteLogs("Scan: " + pathLocal + "===> " + lstFile.Count.ToString());
            DateTime initialDate = new DateTime(1990, 1, 1);
            int count = 0;
            if (lstFile.Count > 0)
            {
                foreach (string file in lstFile)
                {
                    if (file.ToString().Substring(0, 7).ToUpper() == "VOUCHER")
                    {
                        using IDbConnection conn = _dapperContext.ConnCentralPhucLong;
                        try
                        {
                            conn.Open();
                            var dataRaw = JsonConvert.DeserializeObject<Crm_Voucher_Info>(System.IO.File.ReadAllText(pathLocal + file));
                            count++;
                            if (dataRaw != null)
                            {
                                VoucherInfoDto voucher = new VoucherInfoDto()
                                {
                                    id = dataRaw.id,
                                    serial_number = dataRaw.ean,
                                    type = dataRaw.type,
                                    publish_date = dataRaw.publish_date,
                                    publish_id = dataRaw.publish_id,
                                    state = dataRaw.state,
                                    effective_date_from = dataRaw.effective_date_from?? initialDate,
                                    effective_date_to = dataRaw.effective_date_to?? initialDate,
                                    date_used = dataRaw.date_used?? initialDate,
                                    voucher_amount = dataRaw.voucher_amount??0,
                                    order_reference = dataRaw.order_reference??"",
                                    used_on = "",
                                    update_status = "",
                                };

                                var check = conn.Query<VoucherInfoDto>(@"
                                                        SELECT [id],[serial_number],[type],[publish_date],[publish_id],[state],[effective_date_from],[effective_date_to]
                                                        ,[date_used],[voucher_amount],[order_reference],[used_on],[update_status],[update_date] FROM [dbo].[Odoo_VoucherInfo] WHERE serial_number = '" + voucher.serial_number + @"'").ToList().FirstOrDefault();

                                using var transaction = conn.BeginTransaction();
                                try
                                {
                                    if(check == null)
                                    {
                                        var queryIns = @"INSERT INTO [dbo].[Odoo_VoucherInfo]
                                                            ([id],[serial_number],[type],[publish_date],[publish_id],[state],[effective_date_from]
                                                            ,[effective_date_to],[date_used],[voucher_amount],[order_reference],[used_on],[update_status],[update_date])
                                                        VALUES (@id, @serial_number, @type, @publish_date, @publish_id, @state, @effective_date_from
                                                            , @effective_date_to, @date_used, @voucher_amount, @order_reference, @used_on, @update_status, getdate())";

                                        var affecaffectedRows = conn.Execute(queryIns, voucher, transaction);
                                    }
                                    else
                                    {
                                        if(voucher.state == "Close" && check.state == "Create")
                                        {
                                            string queryUpdate = @"UPDATE [dbo].[Odoo_VoucherInfo] SET state = @state, date_used = @date_used, effective_date_to=@effective_date_to, effective_date_from = @effective_date_from, order_reference = @order_reference,
                                                                            [update_date] = getdate(), update_status= 'N' WHERE id = @id AND serial_number = @serial_number;";
                                            conn.Execute(queryUpdate, voucher, transaction);
                                        }
                                    }
                                    
                                    transaction.Commit();
                                    Console.WriteLine("Done: " + dataRaw.ean);
                                    if (isMoveFile) FileHelper.MoveFileToFolder(pathArchive, pathLocal + file);
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    FileHelper.WriteLogs("ExceptionHelper" + file);
                                    ExceptionHelper.WriteExptionError("INB-VOUCHER: ", ex);
                                }
                            }
                            if(count == maxFile)
                            {
                                break;
                            }
                        }
                        catch(Exception ex)
                        {
                            FileHelper.WriteLogs("SaveVoucherInfo Exception:" + ex.Message.ToString());
                            if (isMoveFile) FileHelper.MoveFileToFolder(pathError, pathLocal + file);
                            FileHelper.WriteLogs("ExceptionHelper" + file);
                            ExceptionHelper.WriteExptionError("INB-VOUCHER: ", ex);
                        }
                    }
                }
            }
        }
        public void UpdateStatusVoucherToSAP(string procedure)
        {
            try
            {
                FileHelper.WriteLogs("===> Start update status voucher To SAP");
                using IDbConnection conn = _dapperContext.ConnCentralPhucLong;
                conn.Open();
                WebApiService webApiService = new WebApiService(_configuration);
                var lstWebApi = webApiService.GetWebApiInfo(conn, "SAP");
                var apiVoucher = lstWebApi.WebRoute.Where(x => x.Name == "UpdateStatusVoucher").FirstOrDefault();
                if (lstWebApi != null && apiVoucher != null)
                {
                    var lstVoucher = conn.Query<VoucherInfoDto>(@"EXEC " + procedure).ToList();
                    List<ListSeriNoSAP> voucherList = new List<ListSeriNoSAP>();

                    FileHelper.WriteLogs("Voucher: " + lstVoucher.Count.ToString());
                    FileHelper.WriteLogs("Url: " + lstWebApi.Host + apiVoucher.Route);
                    if (lstVoucher.Count > 0)
                    {
                        Dictionary<string, string> Headers = new Dictionary<string, string>
                        {
                            { "Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(lstWebApi.UserName + ":" + lstWebApi.Password))}
                        };

                        foreach (var item in lstVoucher)
                        {
                            voucherList.Add(new ListSeriNoSAP()
                            {
                                SeriNo = item.serial_number,
                                IsVoucher = true,
                                Value = item.voucher_amount
                            });
                            //FileHelper.WriteLogs("Request: " + JsonConvert.SerializeObject(bodyData));
                            RedeemVoucherSAP bodyData = new RedeemVoucherSAP()
                            {
                                Partner = "PLG",
                                OrderNo = item.order_reference,
                                StoreNo = "2001",
                                PosID = "200101",
                                StaffCode = "System",
                                TotalBill = item.voucher_amount,
                                ListSeriNo = voucherList
                            };
                            RestShapHelper apiHelper = new RestShapHelper(
                                       lstWebApi.Host,
                                       apiVoucher.Route,
                                       "POST",
                                       Headers,
                                       null,
                                       bodyData
                                   );
                            string mess_error = "";
                            int statusRsp = 0;
                            var result = apiHelper.InteractWithApi(ref statusRsp, ref mess_error);

                            //FileHelper.WriteLogs("Response: " + result);

                            DataRspVoucherSAP rsp = JsonConvert.DeserializeObject<DataRspVoucherSAP>(result);

                            if (rsp != null)
                            {
                                if (statusRsp == 200 && rsp.Status == 200)
                                {
                                    UpdateStatusVoucherSAP(conn, item, "Y", rsp.Message.ToString());
                                    FileHelper.WriteLogs("DONE: " + JsonConvert.SerializeObject(rsp));
                                }
                                else
                                {
                                    UpdateStatusVoucherSAP(conn, item, "Z", JsonConvert.SerializeObject(rsp.Data));
                                }
                            }
                            else
                            {
                                FileHelper.WriteLogs("Failed");
                                UpdateStatusVoucherSAP(conn, item, "E", mess_error);
                            }
                            Thread.Sleep(100);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.WriteLogs("UpdateStatusVoucherToSAP Exception" + ex.Message.ToString());
            }
        }
        private void UpdateStatusVoucher(IDbConnection conn, VoucherInfoDto voucherInfoDto, string update_flg, string message)
        {
            using var transaction = conn.BeginTransaction();
            string queryUpdate = @"UPDATE [dbo].[Odoo_VoucherInfo] SET update_date = getdate(), update_status='" + update_flg + "', message_api = '"+ message + "'  WHERE serial_number = @serial_number;";
            conn.Execute(queryUpdate, voucherInfoDto, transaction);
            transaction.Commit();
        }
        private void UpdateStatusVoucherSAP(IDbConnection conn, VoucherInfoDto voucherInfoDto, string update_flg, string message)
        {
            using var transaction = conn.BeginTransaction();
            string queryUpdate = @"UPDATE [dbo].[Odoo_VoucherInfo] SET update_date = getdate(), update_flag='" + update_flg + "', update_sap = '" + message + "'  WHERE serial_number = @serial_number;";
            conn.Execute(queryUpdate, voucherInfoDto, transaction);
            transaction.Commit();
        }
    }
}
