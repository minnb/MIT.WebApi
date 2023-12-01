using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using VCM.Common.Helpers;
using VCM.Shared.Dtos.Invoice;
using VCM.Shared.Entity.Invoice;
using WebApi.Core.AppServices.Kafka;
using WebApi.InvoiceNumbering.Models;

namespace WebApi.InvoiceNumbering.Services
{
    public interface IInvoiceNumberingService
    {
        InvoiceNumberingDto GetNextInvoiceNumber(InvoiceNumberingRequest request);
    }
    public class InvoiceNumberingService : IInvoiceNumberingService
    {
        private readonly IDatabase _redis;
        private readonly ILogger<InvoiceNumberingService> _logger;
        private readonly IKafkaProducerService _kafkaProducerService;
        public InvoiceNumberingService
            (
            ILogger<InvoiceNumberingService> logger,
            IConnectionMultiplexer redis,
            IKafkaProducerService kafkaProducerService
            )
        {
            _logger = logger;
            _redis = redis.GetDatabase();
            _kafkaProducerService = kafkaProducerService;
        }
        public InvoiceNumberingDto GetNextInvoiceNumber(InvoiceNumberingRequest request)
        {
            long invoiceNumber = 1;
            string redisKey = request.TaxCode + "-" + request.TemplateNo + "-" + request.SerialNo;
            if (_redis.KeyExists(redisKey))
            {
                invoiceNumber = _redis.StringIncrement(redisKey);
            }
            else
            {
                _redis.StringSet(redisKey, 1);
            }
            var result = new InvoiceNumberingDto()
            {
                StoreNo = request.PosNo.Substring(0, 4),
                PosNo = request.PosNo,
                TaxCode = request.TaxCode,
                TemplateNo = request.TemplateNo,
                SerialNo = request.SerialNo,
                InvoiceNumber = StringHelper.NumberPadLeft(invoiceNumber, '0', 7)
            };

            _kafkaProducerService.SendMessageAsync("InvoiceNumbering", JsonConvert.SerializeObject(GetInvoiceNumberingDetail(result)));
            _logger.LogWarning("InvoiceNumbering: " + redisKey +"-"+ StringHelper.NumberPadLeft(invoiceNumber, '0', 7));

            return result;
        }

        private InvoiceNumberingDetail GetInvoiceNumberingDetail(InvoiceNumberingDto invoiceNumberingDto)
        {
            return new InvoiceNumberingDetail()
            {
                StoreNo = invoiceNumberingDto.StoreNo,
                PosNo = invoiceNumberingDto.PosNo,
                TaxCode = invoiceNumberingDto.TaxCode,
                TemplateNo = invoiceNumberingDto.TemplateNo,
                SerialNo = invoiceNumberingDto.SerialNo,
                InvoiceNumber = invoiceNumberingDto.InvoiceNumber,
                IssueDateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"),
                AddressIp = System.Environment.MachineName.ToString(),
            };
        }
    }
}
