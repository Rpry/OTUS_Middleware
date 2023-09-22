using System;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Middleware.Middlewares;
using Middleware.Utilities;
using Prometheus;

namespace Middleware
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<IMemoryCache, MemoryCache>();
            services.AddSingleton<MetricReporter>();

            services.AddHealthChecks()
              //.AddCheck<SampleHealthCheck>("SampleHealthCheck");
              .AddCheck<SampleHealthCheck>(
                "SampleHealthCheck",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[]
                {
                    "SampleHealthCheck"
                });
              
            
            services.AddHttpLogging(httpLoggingOptions =>
            {
                httpLoggingOptions.LoggingFields =
                    HttpLoggingFields.All;
            });
            services.AddResponseCaching((opt) =>
            {
            });
            
            services.AddRateLimiter(options => {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                    httpContext => RateLimitPartition.GetFixedWindowLimiter(partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(), factory: partition => new FixedWindowRateLimiterOptions {
                    AutoReplenishment = true,
                    PermitLimit = 5,
                    QueueLimit = 0,
                    Window = TimeSpan.FromMinutes(1)
                }));
                options.RejectionStatusCode = 429;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.UseExceptionHandlingMiddleware();

            #region Виды регистраций

            //регистрация делегатом
            
            /*
            app.Use(async (context, next) =>
            {
              //await context.Response.WriteAsync("Hello from middleware delegate.");
              await next.Invoke();
            });
                  
            app.Run(async (context) =>
            {
              //await context.Response.WriteAsync("Hello from middleware delegate.");
            });
            */

            //регистрация с помощью UseMiddleware
            app.UseMiddleware<RequestCultureMiddleware>();

            //регистрация методом расширения
            app.UseRequestCulture();

            #endregion

            #region Conditional
            /*
            
            app.Map("/test/error" , appBuilder =>
            {
              appBuilder.UseMiddleware<RequestCultureMiddleware>();
            });
            
            
            app.MapWhen(context => context.Request.Path.StartsWithSegments("/test/error"), (appBuilder) =>
            {
                appBuilder.UseMiddleware<RequestCultureMiddleware>();
            });
            
            */
            #endregion

            #region Виды пользовательских миддлваре

            //app.UseRequestCulture();
            
            //Логирование запроса
            //app.UseSimpleHttpLogging();
            //app.UseHttpLogging();
            
            //Обработка исключений
            //app.UseSimpleExceptionHandling();

            //Кеширование запроса
            //app.UseSimpleCaching();
            //app.UseResponseCaching();

            //Антитроттлинг
            //app.UseSimpleRateLimiter();
            //app.UseRateLimiter();  //.Net7

            //Хелсчек
            app.UseHealthChecks("/health");
            app.UseHealthChecks("/samplehealth", new HealthCheckOptions()
            {
                Predicate = healthCheck => healthCheck.Tags.Contains("SampleHealthCheck")
            });

            //Метрики для прометеуса
            app.UseMetricServer();
            app.UseMiddleware<ResponseMetricMiddleware>();

            #endregion

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}