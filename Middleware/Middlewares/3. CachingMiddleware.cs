using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Middleware.Middlewares
{
  public class CachingMiddleware
  {
    private readonly RequestDelegate _next;

    public CachingMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    public async Task InvokeAsync(HttpContext context,
      IMemoryCache memoryCache,
      ILogger<CachingMiddleware> logger)
    {
      context.Request.EnableBuffering();
      
      var responseStream = context.Response.Body;
      var key = context.Request.Path.ToString();
      var cache = memoryCache.Get<byte[]>(key);
      if (cache != null)
      {
        logger.LogInformation("taking data from cache");
        context.Response.Body = responseStream;
        context.Response.Headers.Append("Content-Type", "text/plain; charset=utf-8");
        await context.Response.Body.WriteAsync(cache);
      }
      else
      {
        logger.LogInformation("taking data from action method");
        await using var ms = new MemoryStream();
        context.Response.Body = ms;
        await _next(context);
        memoryCache.Set(key, ms.ToArray(), TimeSpan.FromSeconds(5));
        context.Response.Body = responseStream;
        await context.Response.Body.WriteAsync(ms.ToArray());
      }
    }
  }
  
  public static class CachingExtensions
  {
    public static IApplicationBuilder UseCaching(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<CachingMiddleware>();
    }
  }
}