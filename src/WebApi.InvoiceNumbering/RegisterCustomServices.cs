using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Linq;
using WebApi.Core.AppServices.Kafka;
using WebApi.Core.AppServices.RedisService;
using WebApi.DrWin.Authentication;
using WebApi.InvoiceNumbering.Services;

namespace WebApi.InvoiceNumbering
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
           
            services.AddScoped<IStackExchangeRedisService, StackExchangeRedisService>();
            services.AddScoped<IInvoiceNumberingService, InvoiceNumberingService>();
            services.AddScoped<IKafkaProducerService, KafkaProducerService>();

        }
    }
}
