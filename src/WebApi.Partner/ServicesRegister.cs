using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VCM.Partner.API.Application.Implementation;
using VCM.Partner.API.Application.Interfaces;
using VCM.Partner.API.Authentication;

namespace VCM.Partner.API
{
    public static class ServicesRegister
    {
        public static void RegisterCustomServices(this IServiceCollection services, string connectionString)
        {
            services.AddAuthentication("BasicAuthentication")
                    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

            services.AddScoped<IMemoryCacheService, MemoryCacheService>();
            services.AddScoped<IAirPayService, AirPayService>();
            services.AddScoped<ITransService, TransService>();
            services.AddScoped<IPhanoService, PhanoService>();
            services.AddScoped<IMobiCastService, MobiCastService>();
            services.AddScoped<IShoppingCartService, ShoppingCartService>();
            services.AddTransient<IPhucLongService, PhucLongService>();
            services.AddTransient<IMakeOrderService, MakeOrderService>();

            //HealthCheckResult
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddSqlServer(
                    connectionString,
                    name: "PartnerDB-check",
                    tags: new string[] { "PartnerDb" });
                    services.AddHealthChecksUI(opt =>
                    {
                        opt.SetEvaluationTimeInSeconds(15); //time in seconds between check
                        opt.MaximumHistoryEntriesPerEndpoint(60); //maximum history of checks
                        //opt.SetApiMaxActiveRequests(1); //api requests concurrency
                        //opt.AddHealthCheckEndpoint("Partner API", "/hc"); //map health check api
                    }).AddInMemoryStorage();
         }
    }
}
