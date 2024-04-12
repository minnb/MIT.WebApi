using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using VCM.Partner.API.Common.Extentions;
using VCM.Partner.API;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Partner;
using WebApi.Core.Middleware;
using Serilog.Sinks.Elasticsearch;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MIT.WebApi
{
    public class Startup
    {
        [Obsolete]
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Log.Logger = new LoggerConfiguration()
                //.Enrich.FromLogContext()
                .WriteTo.File("logs/warn/log-.txt", Serilog.Events.LogEventLevel.Warning, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824, null, false, false, null, RollingInterval.Hour, false, 300, null)
                .WriteTo.File("logs/error/log-.txt", Serilog.Events.LogEventLevel.Error, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824, null, false, false, null, RollingInterval.Hour, false, 300, null)
                .Enrich.WithMachineName()
                .Enrich.WithProperty("ResponseTime", 0)
                .Enrich.WithProperty("PosNo", "")
                .Enrich.WithProperty("HttpContext", "")
                .Enrich.WithProperty("Message", "")
                .Enrich.WithProperty("WebApi", "")
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(Configuration.GetConnectionString("ElasticConfiguration"))) // for the docker-compose implementation
                {
                    AutoRegisterTemplate = true,
                    OverwriteTemplate = true,
                    DetectElasticsearchVersion = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    IndexFormat = Configuration["AppSetting:AppCode"] + $"_partner_api_{DateTime.UtcNow:yyyy-MM-dd}",
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
           
            services.AddMemoryCache();

            services.AddDbContext<PartnerDbContext>(options =>
                options.UseSqlServer(connectionString,
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(migrationsAssembly);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

            services.AddStackExchangeRedisCache(option =>
                {
                    option.Configuration = Configuration["RedisCache:Server"];
                }
            );
           
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            // JWT Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                    };
                });

            services.AddHealthChecks();
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
            services.AddSingleton<IConfiguration>(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseMyCustomMiddleware();
            app.UseForwardedHeaders();
            loggerFactory.AddSerilog();
            app.UseDefaultFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
   
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/hc");
                endpoints.MapControllers();
            });

            if(Configuration["AppSetting:Environment"] == "DEV")
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WCM v1");
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

