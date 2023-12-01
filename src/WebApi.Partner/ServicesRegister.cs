using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Http;
using System;
using System.Net;
using System.Net.Http;
using VCM.Partner.API.Application.Implementation;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.Authentication;
using WebApi.Core.AppServices;
using WebApi.Core.AppServices.ShopeeService;
using WebApi.Partner.Application.Implementation;
using WebApi.Partner.Application.Interfaces;
using WebApi.Partner.Application.Repository;

namespace VCM.Partner.API
{
    public static class ServicesRegister
    {
        public static void RegisterCustomServices(this IServiceCollection services, string connectionString)
        {
            services.AddAuthentication("BasicAuthentication")
                    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
            services.AddAuthorization();

            //services.AddScoped<IRedisCacheService, RedisCacheService>();
            services.AddScoped<IMemoryCacheService, MemoryCacheService>();
            services.AddScoped<IAirPayService, AirPayService>();
            services.AddScoped<ITransService, TransService>();
            services.AddScoped<IPhanoService, PhanoService>();
            services.AddScoped<IMobiCastService, MobiCastService>();
            services.AddScoped<IShoppingCartService, ShoppingCartService>();
            services.AddTransient<IPhucLongService, PhucLongService>();
            services.AddTransient<IPhucLongV2Service, PhucLongV2Service>();
            services.AddTransient<IOrderRedisService, OrderRedisService>();
            services.AddTransient<IShopeeService, ShopeeService>();
            services.AddTransient<IDishShopeeService, DishShopeeService>();
            services.AddTransient<IToppingShopeeService, ToppingShopeeService>();
            services.AddTransient<IRestaurantShopeeService, RestaurantShopeeService>();
            services.AddTransient<IO2Service, O2Service>();
            services.AddTransient<IVoucherService, VoucherService>();
            services.AddTransient<IKibanaService, KibanaService>();

            services.AddTransient<IVoucherRepository, VoucherRepository>();

        }
    }
}
