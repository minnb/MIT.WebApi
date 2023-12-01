using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using VCM.Shared.API.BLUEPOS;
using VCM.Shared.Entity.Partner;
using Microsoft.Extensions.Logging;
using VCM.Partner.API.Application.Implementation;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Partner;
using VCM.Shared.Enums;
using System.Linq;

namespace WebApi.Partner.Application.Repository
{
    public interface IVoucherRepository
    {

    }
    public class VoucherRepository: IVoucherRepository
    {
        private readonly ILogger<VoucherRepository> _logger;
        private readonly PartnerDbContext _dbConext;
        public VoucherRepository(
            ILogger<VoucherRepository> logger,
            PartnerDbContext dbConext
          )
        {
            _logger = logger;
            _dbConext = dbConext;
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
                            ArticleType = item.Article_No,
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
