using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using WCM.EntityFrameworkCore.EntityFrameworkCore.PriceEngine;
using WebApi.Core.Common.Extentions;

namespace WebApiPriceEngine
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
                            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(Configuration.GetConnectionString("ElasticConfiguration"))) // for the docker-compose implementation
                            {
                                AutoRegisterTemplate = true,
                                OverwriteTemplate = true,
                                DetectElasticsearchVersion = true,
                                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                                IndexFormat = Configuration["AppSetting:AppCode"] + $"_api_{DateTime.UtcNow:yyyy-MM-dd}",
                                ModifyConnectionSettings = x => x.BasicAuthentication("elastic", "elastic")
                            })
            .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            string connectionString = Configuration.GetConnectionString("Default");
            string connectionStringPLH = Configuration.GetConnectionString("PLH");
            string connectionStringPLF = Configuration.GetConnectionString("PLF");
            services.AddMemoryCache();

            services.AddDbContext<PriceEngineDbContext>(options =>
                options.UseSqlServer(connectionString,
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(migrationsAssembly);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(60);
                }));

            services.AddDbContext<PriceEnginePLHDbContext>(options =>
               options.UseSqlServer(connectionStringPLH,
               sqlServerOptionsAction: sqlOptions =>
               {
                   sqlOptions.MigrationsAssembly(migrationsAssembly);
                   sqlOptions.EnableRetryOnFailure(
                       maxRetryCount: 3,
                       maxRetryDelay: TimeSpan.FromSeconds(30),
                       errorNumbersToAdd: null);
                   sqlOptions.CommandTimeout(60);
               }));

            services.AddDbContext<PriceEnginePLFDbContext>(options =>
               options.UseSqlServer(connectionStringPLF,
               sqlServerOptionsAction: sqlOptions =>
               {
                   sqlOptions.MigrationsAssembly(migrationsAssembly);
                   sqlOptions.EnableRetryOnFailure(
                       maxRetryCount: 3,
                       maxRetryDelay: TimeSpan.FromSeconds(30),
                       errorNumbersToAdd: null);
                   sqlOptions.CommandTimeout(60);
               }));

            services.AddStackExchangeRedisCache(option =>
            {
                option.Configuration = Configuration["RedisCache:Server"] + "," + Configuration["RedisCache:Database"];
            }
            );

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WINMART", Version = "v1" });
                c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    In = ParameterLocation.Header,
                    Description = "Basic Authorization header using the Bearer scheme."
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                              new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "basic"
                                    }
                                },
                                new string[] {}
                        }
                    });

                c.SchemaFilter<SchemaFilter>();
            });

            //setup DI
            services.RegisterCustomServices(connectionString);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            //app.UseSwagger();
            //app.UseSwaggerUI(c => {
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WCM v1");
            //    c.DefaultModelsExpandDepth(-1);
            //});

            loggerFactory.AddSerilog();
            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecksUI(options => options.UIPath = "/hc-ui");
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
                endpoints.MapHealthChecks("/hc-details",
                            new HealthCheckOptions
                            {
                                ResponseWriter = async (context, report) =>
                                {
                                    var result = JsonSerializer.Serialize(
                                        new
                                        {
                                            status = report.Status.ToString(),
                                            monitors = report.Entries.Select(e => new { key = e.Key, value = Enum.GetName(typeof(HealthStatus), e.Value.Status) })
                                        });
                                    context.Response.ContentType = MediaTypeNames.Application.Json;
                                    await context.Response.WriteAsync(result);
                                }
                            }
                        );

                endpoints.MapControllers();
            });

            if (Configuration["AppSetting:Environment"] == "DEV")
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MASAN v1");
                    c.DefaultModelsExpandDepth(-1);
                });

            }
            else
            {
                app.Run(async (context) =>
                {
                    await context.Response.WriteAsync("<h1>Not Found</h1><p>The requested resource was not found on this server.</p>");
                });
            }
        }
    }
}
