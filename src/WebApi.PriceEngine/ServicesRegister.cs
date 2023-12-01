using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Linq;
using WebApi.PriceEngine.Application.Implementation;
using WebApi.PriceEngine.Application.Interfaces;
using WebApiPriceEngine.Authentication;

namespace WebApiPriceEngine
{
    public static class ServicesRegister
    {
        public static void RegisterCustomServices(this IServiceCollection services, string connectionString)
        {
            services.AddAuthentication("BasicAuthentication")
                    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
            services.AddAuthorization();

            services.AddResponseCompression(options => {
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] {
                    "text/plain",
                    "text/css",
                    "application/javascript",
                    "text/html",
                    "application/xml",
                    "text/xml",
                    "application/json",
                    "text/json",
                });
            });

            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);

            services.AddScoped<IRedisCacheService, RedisCacheService>();
            services.AddScoped<IMasterDataService, MasterDataService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IDistributedCacheService, DistributedCacheService>();
            services.AddScoped<IBillPaymentService, BillPaymentService>();

            //HealthCheckResult
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddSqlServer(
                    connectionString,
                    name: "PriceEngine-check",
                    tags: new string[] { "PriceEngine" });
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
