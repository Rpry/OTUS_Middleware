using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
            //app.UseMiddleware<RequestCultureMiddleware>();

            //регистрация методом расширения
            //app.UseRequestCulture();

            #endregion

            #region Conditional
          
            /*
            app.Map("/test/error" , appBuilder =>
            {
              appBuilder.UseMiddleware<RequestCultureMiddleware>();
            });
            */
            /*
            app.MapWhen(context => context.Request.Path.StartsWithSegments("/test/error"), (appBuilder) =>
            {
                appBuilder.UseMiddleware<RequestCultureMiddleware>();
            });
            */
            
            #endregion

            #region Виды пользовательских миддлваре

            //Логирование запроса
            //app.UseHttpRequestLogging();

            //Обработка исключений
            //app.UseExceptionHandlingMiddleware();

            //Кеширование запроса
            ///app.UseCaching();

            //Антитроттлинг
            //app.UseRateLimiting();
            //app.UseRatiLimiterWithCaching();

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