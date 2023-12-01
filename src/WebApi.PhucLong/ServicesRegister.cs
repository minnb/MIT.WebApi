using Microsoft.Extensions.DependencyInjection;
using VCM.PhucLong.API.Services;
using WebApi.Core.AppServices.ShopeeService;
using WebApi.PhucLong.Services;

namespace WebApi.PhucLong
{
    public static class ServicesRegister
    {
        public static void RegisterCustomServices(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<IMasterService, MasterService>();
            services.AddTransient<IRedisService, RedisService>();
            services.AddTransient<ICrmService, CrmService>();
            services.AddTransient<IDishShopeeService, DishShopeeService>();
            services.AddTransient<IToppingShopeeService, ToppingShopeeService>();
            services.AddTransient<IRestaurantShopeeService, RestaurantShopeeService>();
            services.AddTransient<IPhucLongShopeeService, PhucLongShopeeService>();
        }
    }
}
