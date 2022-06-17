using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Middleware.Middlewares
{
  public class RateLimiterMiddleware
  {
    private readonly RequestDelegate _next;

    public RateLimiterMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IMemoryCache memoryCache)
    {
      var now = DateTime.UtcNow;
      var minInterval = TimeSpan.FromSeconds(5);
      var key = context.Request.Path.ToString();
      var lastRequestDate = memoryCache.Get<DateTime?>(key);
      if (lastRequestDate != null && now - lastRequestDate < minInterval)
      {
        context.Response.StatusCode = (int) HttpStatusCode.TooManyRequests;
        context.Response.Headers["Retry-After"] = (lastRequestDate - now + minInterval).Value.TotalSeconds.ToString("#");
      }
      else
      {
        memoryCache.Set<DateTime>(key, now);
        await _next(context);
      }
    }
  }
  
  public static class RateLimiterExtensions
  {
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<RateLimiterMiddleware>();
    }
  }
}