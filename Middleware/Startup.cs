using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Middleware.Middlewares;

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
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      //app.UseHttpStatusCodeExceptionMiddleware();
      
      //регистрация делегатом
      /*
      app.Use(async (context, next) =>
      {
        //await context.Response.WriteAsync("Hello from middleware delegate.");
        await next.Invoke();
      });
      */
      //регистрация с помощью UseMiddleware
      // app.UseMiddleware<RequestCultureMiddleware>();

      //регистрация методом расширения
      //app.UseRequestCulture();
      //app.UseHttpRequestLogging();
      //app.UseCaching();
      //app.UseRateLimiting();
      
      app.UseRouting();
      
      app.UseAuthorization();

      app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
  }
}