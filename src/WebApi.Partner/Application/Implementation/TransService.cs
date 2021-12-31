using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using VCM.Common.Helpers;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.Database;
using VCM.Partner.API.ViewModels.Partner;
using VCM.Shared.API;
using VCM.Shared.Entity.Partner;
using VCM.Shared.SeedWork;

namespace VCM.Partner.API.Application.Implementation
{
    public class TransService : ITransService
    {
        private readonly PartnerDbContext _dbConext;
        private readonly ILogger<TransService> _logger;
        public TransService(
                PartnerDbContext dbConext,
                 ILogger<TransService> logger
            )
        {
            _dbConext = dbConext;
            _logger = logger;
        }
        public TransRaw AddTransRaw(TransRaw transRaw)
        {
            try
            {
                _dbConext.TransRaw.Add(transRaw);
                _dbConext.SaveChanges();
                return transRaw;
            }
            catch
            {
                return null;
            }
            
        }

        public string AddStoreAndKios(StoreAndKios data)
        {
            try
            {
                _dbConext.StoreAndKios.Add(data);
                _dbConext.SaveChanges();
                return "OK";
            }
            catch (Exception ex)
            {
                string messageError = ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    messageError = ex.InnerException.Message.ToString();
                }
                return messageError;
            }
        }

        public async Task<ResponseClient> GetStoreAndKiosAsync(GetStoreKiosPaging query)
        {
            var filter = await _dbConext.StoreAndKios.ToListAsync();

            if (!string.IsNullOrEmpty(query.SearchKeyword))
                filter = filter.Where(s => s.LocationName.Contains(query.SearchKeyword)).ToList();

            if (!string.IsNullOrEmpty(query.PosOdoo))
                filter = filter.Where(s => s.PosOdoo.Contains(query.PosOdoo)).ToList();

            if (!string.IsNullOrEmpty(query.StoreNo))
                filter = filter.Where(s => s.StoreNo == query.StoreNo).ToList();

            var totalRow = filter.Count();

            var items =  filter
                .OrderByDescending(x => x.CrtDate)
                .Skip((query.PageIndex - 1) * query.PageSize)
                .Take(query.PageSize).ToList();

            var result = new PagedList<StoreAndKios>(items, totalRow, query.PageIndex, query.PageSize);

            if(result != null)
            {
                return ResponseHelper.RspOK(result);
            }
            else
            {
                return ResponseHelper.RspNotFoundData("Không tìm thấy dữ liệu");
            }
        }

        public void LoggingApi(string partner, string posNo, string serviceType, string orderNo, string reference, string rawData, string status)
        {
            this.AddTransRaw(new TransRaw()
            {
                Id = Guid.NewGuid(),
                AppCode = partner,
                StoreNo = posNo.Substring(0, 4),
                OrderNo = orderNo,
                ServiceType = serviceType,
                ReferenceNo = reference,
                RawData = rawData,
                UpdateFlg = status,
                CrtDate = DateTime.Now,
                CrtUser = posNo,
                IPAddress = System.Environment.MachineName.ToString()
            });
        }
        public async Task<RawData> AddRawDataAsync(string appCode, string storeNo, string orderNo, string requestId, string jsonData, string status)
        {
            try
            {
                var rawData = new RawData()
                            {
                                Id = Guid.NewGuid(),
                                AppCode = appCode,
                                StoreNo = storeNo,
                                OrderNo = orderNo,
                                RequestId = requestId,
                                JsonData = jsonData,
                                UpdateFlg = status,
                                CrtDate = DateTime.Now,
                                CrtUser = appCode,
                                HostName = System.Environment.MachineName.ToString()
                            };
                _dbConext.RawData.Add(rawData);
                await _dbConext.SaveChangesAsync();
                return rawData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
                return null;
            }
        }
    }
}
