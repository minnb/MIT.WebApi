using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VCM.Partner.API.Application.Interfaces;
using VCM.Shared.API;
using VCM.Shared.API.MakeOrder;
using VCM.Shared.Enums;

namespace VCM.Partner.API.Application.Implementation
{
    public class MakeOrderService : IMakeOrderService
    {
        private readonly ITransService _transService;
        private readonly ILogger<MakeOrderService> _logger;
        public MakeOrderService(
             ILogger<MakeOrderService> logger,
            ITransService transService
            )
        {
            _transService = transService;
            _logger = logger;
        }

        public async Task<ResponseClient> CreateOrderAsync(OrderRequestBody orderRequestBody)
        {
            try
            {
                var result = await _transService.AddRawDataAsync(orderRequestBody.AppCode, orderRequestBody.StoreNo, orderRequestBody.OrderNo, orderRequestBody.OrderNo, JsonConvert.SerializeObject(orderRequestBody), "N");
                if (result != null)
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 200,
                            Message = "Successfully"
                        },
                        Data = null
                    };
                }
                else
                {
                    return new ResponseClient()
                    {
                        Meta = new Meta()
                        {
                            Code = 201,
                            Message = ""
                        },
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());

                return new ResponseClient()
                {
                    Meta = new Meta()
                    {
                        Code = (int) ApiStatusEnum.Failed,
                        Message = ApiStatusEnum.Failed.ToString()
                    },
                    Data = null
                };
            }
        }
    }
}
