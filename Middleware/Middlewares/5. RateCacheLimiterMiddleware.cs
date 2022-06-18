using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Middleware.Middlewares
{
    public class RateCacheLimiterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CachingMiddleware> _logger;
        private readonly IMemoryCache _memoryCache;

        public RateCacheLimiterMiddleware(RequestDelegate next, ILogger<CachingMiddleware> logger, IMemoryCache memoryCache)
        {
            _next = next;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task InvokeAsync(HttpContext context, IMemoryCache memoryCache)
        {
            var now = DateTime.UtcNow;
            var minInterval = TimeSpan.FromSeconds(5);
            var keyDate = $"date_{context.Request.Path}";
            var keyData = $"data_{context.Request.Path}";


            var lastRequestDate = memoryCache.Get<DateTime?>(keyDate);
            if (lastRequestDate != null && now - lastRequestDate < minInterval)
            {
                _logger.LogInformation("Too frequent request. Getting response from cache");
                var cache = _memoryCache.Get<byte[]>(keyData);
                if (cache != null)
                {
                    await GetResponseFromCache(context, cache);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.Headers["Retry-After"] = (lastRequestDate - now + minInterval).Value.TotalSeconds.ToString("#");
                }
            }
            else
            {
                _logger.LogInformation("Normal request. Getting response from action method");
                memoryCache.Set<DateTime>(keyDate, now);
                await SetCacheAndInvokeNext(context, keyData);
            }
        }

        private async Task GetResponseFromCache(HttpContext context, byte[] cacheData)
        {
            _logger.LogInformation("taking response from cache");
            var responseStream = context.Response.Body;
            context.Response.Body = responseStream;
            context.Response.Headers.Append("Content-Type", "text/plain; charset=utf-8");
            await context.Response.Body.WriteAsync(cacheData);
        }

        private async Task SetCacheAndInvokeNext(HttpContext context, string key)
        {
            _logger.LogInformation("taking data from action method");
            var responseStream = context.Response.Body;
            await using var ms = new MemoryStream();
            context.Response.Body = ms;
            await _next(context);
            _memoryCache.Set(key, ms.ToArray(), TimeSpan.FromSeconds(5));
            context.Response.Body = responseStream;
            await context.Response.Body.WriteAsync(ms.ToArray());
        }
    }

    public static class RateCacheLimiterExtensions
    {
        public static IApplicationBuilder RateCacheLimiterMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateCacheLimiterMiddleware>();
        }
    }
}