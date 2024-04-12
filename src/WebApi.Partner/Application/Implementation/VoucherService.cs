using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.Logging;
using MIT.Utils.Utils;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Partner.API.Application.Implementation;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.ViewModels.AirPay;
using VCM.Partner.API.ViewModels.MBC;
using VCM.Shared.API.BLUEPOS;
using VCM.Shared.API.Voucher;
using VCM.Shared.Entity.Partner;
using VCM.Shared.Enums;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Partner;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebApi.Partner.Application.Implementation
{
    public interface IVoucherService
    {
        Task<Tuple<bool, string, VoucherIssueDetail>> CheckVouchersync(WebApiViewModel webApiInfo, CheckVoucherSAP request, string proxyHttp, string[] byPass);
        Task<Tuple<bool, string>> UpdateStatusVouchersync(WebApiViewModel webApiInfo, UpdateStatusVoucherSAP request, string proxyHttp, string[] byPass);
        Task<Tuple<bool, string, VoucherIssueInfo>> RegisterPhoneNumberAsync(IssueVoucher issueVoucher);
        Task<Tuple<bool, string>> CreateVoucherToSAPAsync(WebApiViewModel webApiInfo,Item itemDto, VoucherIssueInfo issueVoucherInfo, string proxyHttp, string[] byPass);
    }

    public class VoucherService: IVoucherService
    {
        private readonly ILogger<VoucherService> _logger;
        private readonly PartnerDbContext _dbConext;
        public VoucherService(
          ILogger<VoucherService> logger,
        PartnerDbContext dbConext
          )
        {
            _logger = logger;
            _dbConext = dbConext;
        }
        private string CallApiBLUEPOS(WebApiViewModel webApiInfo, string function, string bodyJson, string proxyHttp, string[] byPass, string requestId)
        {
            string result = string.Empty;
            if (webApiInfo == null)
            {
                return null;
            }

            var routeApi = webApiInfo.WebRoute.Where(x => x.Name.ToLower() == function.ToLower()).FirstOrDefault();
            var url_request = webApiInfo.Host + routeApi.Route.ToString();
            string errMsg = "";
            try
            {
                ApiHelper api = new ApiHelper(
                    url_request,
                    "",
                    null,
                    "POST",
                    bodyJson,
                    false,
                    proxyHttp,
                    byPass
                    );

                _logger.LogWarning(String.Format("{0} Request {1}\t\n{2}", requestId, url_request, bodyJson));

                var rs = api.InteractWithApiResponse(ref errMsg);

                using Stream stream = rs.GetResponseStream();
                StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
                result = streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                result = ex.Message;
                _logger.LogWarning(requestId + " CallApiBLUEPOS Exception: " + ex.Message.ToString());
            }
            _logger.LogWarning(String.Format("{0} Request {1}\t\n{2}", requestId, url_request, result));
            return result;
        }
        public async Task<Tuple<bool, string>> CreateVoucherToSAPAsync(WebApiViewModel webApiInfo, Item itemDto, VoucherIssueInfo issueVoucherInfo, string proxyHttp, string[] byPass)
        {
            try 
            {
                string requestId = StringHelper.InitRequestId();
                var lstVoucherToSAP = new List<CreateNewVoucherSAP>();
                string ExpDate = DateTime.Now.AddDays(itemDto.VatPercent).ToString("dd/MM/yyyy");
                if (itemDto.VatPercent == 0)
                {
                    ExpDate = itemDto.ItemName.ToString();
                }

                for (int i = 1; i<= issueVoucherInfo.NumberOfVouchers; i++)
                {
                    lstVoucherToSAP.Add(new CreateNewVoucherSAP()
                    {
                        VoucherNumber = "PAR" + StringHelper.GetTimeStampString(true) + i.ToString().PadRight(2, '0'),
                        Value = decimal.Parse(itemDto.VatGroup),
                        From_Date = DateTime.Now.ToString("dd/MM/yyyy"),
                        Expiry_Date= ExpDate,
                        SiteCode = itemDto.RefNo[..4],
                        POSTerminal = itemDto.RefNo,
                        BonusBuy= itemDto.Barcode,
                        Article_No= itemDto.ItemNo,
                        Id = issueVoucherInfo.Id,
                        PhoneNumber = issueVoucherInfo.PhoneNumber.ToString(),
                        RequestId = requestId
                    });
                }

                var response = CallApiBLUEPOS(webApiInfo, "CreateNewVoucher", JsonConvert.SerializeObject(lstVoucherToSAP), proxyHttp, byPass, requestId);
                await Task.Delay(1);
                
                if (!string.IsNullOrEmpty(response))
                {
                    var rspData = JsonConvert.DeserializeObject<RspWebApiBluePOS>(response);
                    if(rspData != null && rspData.Status == 200)
                    {
                        var dataVoucher = JsonConvert.DeserializeObject<List<RspCreateNewVoucherSAP>>(JsonConvert.SerializeObject(rspData.Data));
                        if(dataVoucher != null && dataVoucher.FirstOrDefault().Return == "0")
                        {
                            await SaveVoucherInfoDetail(lstVoucherToSAP);
                            return new Tuple<bool, string>(true, "Tạo voucher/coupon thành công");
                        }
                        else
                        {
                            return new Tuple<bool, string>(false, "Voucher/coupon không hợp lệ " + dataVoucher.FirstOrDefault().Status??"");
                        }
                        
                    }
                    else
                    {
                        return new Tuple<bool, string>(false, "Lỗi tạo voucher/coupon lên SAP");
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, "Lỗi tạo voucher/coupon lên SAP");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("CreateVoucherToSAPAsync Exception: " + ex.Message.ToString());
                return new Tuple<bool, string>(false, "Lỗi tạo voucher lên SAP");
            }
        }
        public async Task<Tuple<bool, string, VoucherIssueInfo>> RegisterPhoneNumberAsync(IssueVoucher issueVoucher)
        {
            try
            {
                var dataIns = new VoucherIssueInfo()
                {
                    PhoneNumber = issueVoucher.PhoneNumber,
                    NumberOfVouchers = issueVoucher.NumberOfVouchers,
                    CrtDate = DateTime.Now,
                    ChgDate = DateTime.Now,
                    UpdateFlg = "N",
                    Id = Guid.NewGuid()
                };
                _dbConext.VoucherIssueInfo.Add(dataIns );
                await _dbConext.SaveChangesAsync();
                return new Tuple<bool, string, VoucherIssueInfo>(true, "Đăng ký thành công", dataIns);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("RegisterPhoneNumberAsync Exception: " + ex.Message.ToString());
                return new Tuple<bool, string, VoucherIssueInfo>(false, "Lỗi cập nhật dữ liệu", null);
            }
        }
        public async Task<Tuple<bool, string, VoucherIssueDetail>> CheckVouchersync(WebApiViewModel webApiInfo, CheckVoucherSAP request, string proxyHttp, string[] byPass)
        {
            await Task.Delay(1);
            try
            {
                var lstVoucher =  GetVoucherByPhoneNumber(request.PhoneNumber);
                if(!lstVoucher.Item1 || lstVoucher.Item3 == null) 
                {
                    return new Tuple<bool, string, VoucherIssueDetail>(true, string.Format("Số điện thoại {0} không tìm thấy voucher/coupon", request.PhoneNumber), null);
                }
                else
                {
                    var firstVoucher = lstVoucher.Item3.FirstOrDefault();





                    //var resultVoucher = new RspCheckVoucherSAP()
                    //{
                    //    VoucherNumber = firstVoucher.VoucherNumber,
                    //    Validity_From_Date = firstVoucher.FromDate,
                    //    Expiry_Date = firstVoucher.ExpiryDate,
                    //    Value = firstVoucher.Value,
                    //    Status = firstVoucher.Status,
                    //    ActicleNo = firstVoucher.ActicleNo,
                    //    ActicleType = firstVoucher.ArticleType,
                    //    CompanyCode = "WCM",
                    //    PhoneNumber = firstVoucher.PhoneNumber
                    //};
                    return new Tuple<bool, string, VoucherIssueDetail>(true, "Success", firstVoucher);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("CreateVoucherToSAPAsync Exception: " + ex.Message.ToString());
                return new Tuple<bool, string, VoucherIssueDetail>(false, "Lỗi Exception", null);
            }
        }
        public async Task<Tuple<bool, string>> UpdateStatusVouchersync(WebApiViewModel webApiInfo, UpdateStatusVoucherSAP request, string proxyHttp, string[] byPass)
        {
            try
            {
                string requestId = StringHelper.InitRequestId();
                var lstVoucherToSAP = new List<ListSeriNoUpdate>();
                var checkVoucher = _dbConext.VoucherIssueDetail.Where(x => request.Vouchers.Contains(x.VoucherNumber)).ToList();
                
                if(checkVoucher == null || checkVoucher.Count == 0 || checkVoucher.FirstOrDefault(x=>x.Status != SAPVoucherStatusEnum.SOLD.ToString()) != null)
                {
                    return new Tuple<bool, string>(false, @"Request có voucher/coupon không hợp lệ");
                }

                foreach( var item in checkVoucher)
                {
                    lstVoucherToSAP.Add(new ListSeriNoUpdate()
                    {
                        Status = SAPVoucherStatusEnum.RDM.ToString(),
                        VoucherNumber = item.VoucherNumber,
                        ArticleNo = item.ActicleNo,
                        ArticleType = item.ArticleType,
                        Value = item.Value,
                        CompanyCode = "WCM",
                        Partner = "",
                        IsVoucher = false
                    });
                    item.Status = request.Status;
                    item.PosUsed = request.PosNo;
                    item.UsedTime = DateTime.Now;
                    item.OrderNo = request.OrderNo;
                }

                var bodyJson = new RequestUpdateStatusVoucherSAP()
                {
                    OrderNo = request.OrderNo,
                    TotalBill = 0,
                    POSTerminal = request.PosNo,
                    SiteCode = request.PosNo.Substring(0, 4),
                    ListSeriNo = lstVoucherToSAP
                };

                var response = CallApiBLUEPOS(webApiInfo, "RedeemCpnVch", JsonConvert.SerializeObject(bodyJson), proxyHttp, byPass, requestId);
                await Task.Delay(1);

                if (!string.IsNullOrEmpty(response))
                {
                    var rspData = JsonConvert.DeserializeObject<RspWebApiBluePOS>(response);
                    if (rspData != null && rspData.Status == 200)
                    {
                        var dataVoucher = JsonConvert.DeserializeObject<List<RspCreateNewVoucherSAP>>(JsonConvert.SerializeObject(rspData.Data));
                        if (dataVoucher != null && dataVoucher.FirstOrDefault().Return == "0")
                        {
                            if(! await UpdateVoucherInfoDetail(checkVoucher))
                            {

                            }
                            return new Tuple<bool, string>(true, "Sử dụng voucher thành công");
                        }
                        else
                        {
                            return new Tuple<bool, string>(false, "Request có voucher/coupon không hợp lệ " + dataVoucher.FirstOrDefault().Status ?? "");
                        }

                    }
                    else
                    {
                        return new Tuple<bool, string>(false, "Lỗi sử dụng voucher/coupon lên SAP " + rspData.Status.ToString());
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, "Lỗi sử dụng voucher/coupon lên SAP ");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("CreateVoucherToSAPAsync Exception: " + ex.Message.ToString());
                return new Tuple<bool, string>(false, "Lỗi Exception sử dụng voucher/coupon lên SAP");
            }
        }
        private async Task<bool> SaveVoucherInfoDetail(List<CreateNewVoucherSAP> data)
        {
            bool flg = false;
            var strategy = _dbConext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = _dbConext.Database.BeginTransaction();
                try
                {
                    var lstVoucherDetail = new List<VoucherIssueDetail>();
                    foreach (var item in data)
                    {
                        lstVoucherDetail.Add(new VoucherIssueDetail()
                        {
                            VoucherNumber = item.VoucherNumber,
                            Value = item.Value,
                            Status = SAPVoucherStatusEnum.SOLD.ToString(),
                            CrtDate = DateTime.Now,
                            ArticleType = "ZCPN",
                            BonusBuy = item.BonusBuy,
                            PhoneNumber = item.PhoneNumber,
                            Id = Guid.NewGuid(),
                            RefId = item.Id,
                            RequestId = item.RequestId,
                            FromDate = item.From_Date,
                            ExpiryDate = item.Expiry_Date,
                            ActicleNo = item.Article_No
                        });
                    }

                    lstVoucherDetail.ForEach(x => _dbConext.VoucherIssueDetail.Add(x));
                    await _dbConext.SaveChangesAsync();
                    transaction.Commit();
                    flg = true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("SaveVoucherInfoDetail Exception: " + ex.Message.ToString());
                    transaction.Rollback();
                    flg = false;
                }
            });

            return flg;
        }
        private async Task<bool> UpdateVoucherInfoDetail(List<VoucherIssueDetail> data)
        {
            bool flg = false;
            var strategy = _dbConext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = _dbConext.Database.BeginTransaction();
                try
                {
                    data.ForEach(x => _dbConext.VoucherIssueDetail.Update(x));
                    await _dbConext.SaveChangesAsync();
                    transaction.Commit();
                    flg = true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("SaveVoucherInfoDetail Exception: " + ex.Message.ToString());
                    transaction.Rollback();
                    flg = false;
                }
            });

            return flg;
        }
        private Tuple<bool, string, List<VoucherIssueDetail>> GetVoucherByPhoneNumber(string phoneNumber)
        {
            try
            {
                var lstVoucher = _dbConext.VoucherIssueDetail.Where(x => x.PhoneNumber.ToUpper() == phoneNumber.ToUpper()
                                                                && x.Status == SAPVoucherStatusEnum.SOLD.ToString()).OrderBy(x => x.CrtDate).ToList();

                return new Tuple<bool, string, List<VoucherIssueDetail>>(true, "Success", lstVoucher);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("GetVoucherByPhoneNumber Exception: " + ex.Message.ToString());
                return new Tuple<bool, string, List<VoucherIssueDetail>>(false, ex.Message.ToString(), null);
            }
        }
    }
}
