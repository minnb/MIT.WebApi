using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VCM.Shared.API.Shopee.Restaurants;
using VCM.Shared.Entity.SalesPartner;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Partner;

namespace WebApi.PhucLong.Services
{
    public interface IPhucLongShopeeService
    {
        Task InsertShopeeRestaurant(List<RestaurantHeader> shopeeRestaurant);
    }

    public class PhucLongShopeeService: IPhucLongShopeeService
    {
        private readonly ILogger<PhucLongShopeeService> _logger;
        private readonly IRedisService _redisService;
        private readonly PartnerMDDbContext _dbContext;
        public PhucLongShopeeService
            (
                ILogger<PhucLongShopeeService> logger,
                IRedisService redisService,
                PartnerMDDbContext dbContext
            )
        {
            _logger = logger;
            _redisService = redisService;
            _dbContext = dbContext;
        }

        public async Task InsertShopeeRestaurant(List<RestaurantHeader> shopeeRestaurant)
        {
            try
            {
                if (shopeeRestaurant.Count > 0)
                {
                    foreach(var rs in shopeeRestaurant)
                    {
                        var checkData = _dbContext.ShopeeRestaurant.Where(x => x.partner_restaurant_id == rs.partner_restaurant_id && x.restaurant_id == rs.restaurant_id).FirstOrDefault();
                        if (checkData == null)
                        {
                            var dataIns = new ShopeeRestaurant()
                            {
                                restaurant_id = rs.restaurant_id,
                                partner_restaurant_id = rs.partner_restaurant_id,
                                name = rs.name,
                                address = rs.address,
                                city = rs.city,
                                foody_service = rs.foody_service
                            };
                            _dbContext.Add(dataIns);
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("IPhucLongShopeeService.InsertShopeeRestaurant.Exception " + ex.Message.ToString());
            }
        }
    }
}
