using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Reflection;
using VCM.PhucLong.API.Config;
using VCM.PhucLong.API.Database;
using VCM.PhucLong.API.Services;
using WebApi.PhucLong.Services;

namespace VCM.PhucLong.API
{
    public class Startup
    {
        [Obsolete]
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Log.Logger = new LoggerConfiguration()
                //.Enrich.FromLogContext()
                .WriteTo.File("logs/warn/log-.txt", Serilog.Events.LogEventLevel.Warning, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824, null, false, false, null, RollingInterval.Hour, false, 200, null)
                .WriteTo.File("logs/error/log-.txt", Serilog.Events.LogEventLevel.Error, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824, null, false, false, null, RollingInterval.Hour, false, 200, null)
                .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            string connectionString = Configuration.GetConnectionString("Default");

            services.AddStackExchangeRedisCache(option =>
               option.Configuration = Configuration["RedisConfig:RedisConnectionString"]+":"+ Configuration["RedisConfig:Port"]
            );

            services.AddControllers();

            services.AddSingleton<DapperContext>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "VCM.PhucLong.API", Version = "v1" });
                c.SchemaFilter<SchemaFilter>();
            });


            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<IMasterService, MasterService>();
            services.AddTransient<IRedisService, RedisService>();
            services.AddTransient<ICrmService, CrmService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VCM.PhucLong.API v1"));

            loggerFactory.AddSerilog();

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
