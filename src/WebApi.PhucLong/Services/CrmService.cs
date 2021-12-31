using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VCM.PhucLong.API.Database;
using VCM.Shared.API.PLG;
using VCM.Shared.Entity.PhucLong.Dtos;
using VCM.Shared.Enums;

namespace WebApi.PhucLong.Services
{
    public class CrmService : ICrmService
    {
        private readonly ILogger<CrmService> _logger;
        private readonly DapperContext _context;
        private readonly IRedisService _redisService;
        private readonly IConfiguration _configuration;
        public CrmService
            (
                ILogger<CrmService> logger,
                DapperContext context,
                IRedisService redisService,
                IConfiguration configuration
            )
        {
            _logger = logger;
            _context = context;
            _redisService = redisService;
            _configuration = configuration;
        }

        public Task<VoucherInfoDto> CheckVoucher(string serial_number)
        {
            throw new NotImplementedException();
        }

        public async Task<VoucherInfoDto> UpdateStatusVoucher(RequestVoucherPLG request)
        {
            try
            {
                using var conn = _context.CreateConnection(1);
                conn.Open();
                string queryVoucher = @"SELECT  id, ean as serial_number, publish_date, publish_id, state, 
		                                        (case when effective_date_from is null then '9999-01-01' else effective_date_from end) as effective_date_from,
		                                        (case when effective_date_to is null then '9999-01-01' else effective_date_to end) as effective_date_to,
		                                        voucher_amount, order_reference, 'api' used_on, type,
		                                        (case when date_used is null then '1990-01-01' else date_used end) as date_used
                                        FROM public.crm_voucher_info
                                        where ean = '" + request.serial_number + "';";

                var data = await conn.QueryAsync<VoucherInfoDto>(queryVoucher).ConfigureAwait(false);
                var voucherData = data.FirstOrDefault();
                if (voucherData != null)
                {
                    using var transaction = conn.BeginTransaction();
                    try
                    {
                        if (voucherData.state == VoucherStateEnumPLG.Create.ToString())
                        {
                            DateTime from_date = DateTime.ParseExact(request.effective_date_from, "yyyyMMdd", CultureInfo.InvariantCulture);
                            DateTime to_date = DateTime.ParseExact(request.effective_date_to, "yyyyMMdd", CultureInfo.InvariantCulture);
                            DateTime today = DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture);

                            if (request.status.ToUpper() == VoucherStatusEnum.SOLD.ToString())
                            {
                                if (voucherData.effective_date_from.Year == 2050)
                                {
                                    
                                    voucherData.effective_date_from = from_date;
                                    voucherData.effective_date_to = to_date;
                                    voucherData.serial_number = request.serial_number;
                                    voucherData.state = VoucherStateEnumPLG.Create.ToString();
                                    voucherData.update_status = VoucherUpdateStatusEnum.Success.ToString();

                                    var querySold = @"UPDATE public.crm_voucher_info SET effective_date_from = @effective_date_from, effective_date_to = @effective_date_to, write_date = now()
                                                 WHERE id = @id and ean = @serial_number and state = 'Create';";

                                    await conn.ExecuteAsync(querySold, voucherData, transaction);

                                    _logger.LogWarning("Successfully: " + JsonConvert.SerializeObject(request));
                                }
                                else
                                {
                                    voucherData.update_status = VoucherUpdateStatusEnum.Activated.ToString();
                                }
                            }
                            //else if (request.status.ToUpper() == VoucherStatusEnum.REDE.ToString() && voucherData.effective_date_from != null && voucherData.effective_date_to != null)
                            //{
                            //    if(today <= voucherData.effective_date_to && today >= voucherData.effective_date_from)
                            //    {
                            //        voucherData.state = VoucherStateEnum.Close.ToString();
                            //        voucherData.update_status = VoucherUpdateStatusEnum.Success.ToString();
                            //        var queryRedeem = @"UPDATE public.crm_voucher_info SET state = @state WHERE id = @id and ean = @serial_number;";
                            //        await conn.ExecuteAsync(queryRedeem, voucherData, transaction);
                            //    }
                            //    else
                            //    {
                            //        voucherData.update_status = VoucherUpdateStatusEnum.OutOfDate.ToString();
                            //    }
                            //}
                        }
                        else
                        {
                            voucherData.update_status = VoucherUpdateStatusEnum.UsedOrExpired.ToString();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        voucherData.update_status = VoucherUpdateStatusEnum.Errors.ToString();
                        _logger.LogError(ex.Message.ToString());
                    }

                }
                else
                {
                    voucherData.update_status = VoucherUpdateStatusEnum.NotExist.ToString();
                }
                return voucherData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
                return null;
            }
        }
    }
}
