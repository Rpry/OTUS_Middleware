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
        private readonly ILogger<CachingMiddleware> _logger;
        private readonly IMemoryCache _memoryCache;

        public CachingMiddleware(RequestDelegate next, ILogger<CachingMiddleware> logger, IMemoryCache memoryCache)
        {
            _next = next;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();

            var key = context.Request.Path.ToString();
            var cache = _memoryCache.Get<byte[]>(key);
            if (cache != null)
            {
                await GetResponseFromCache(context, cache);
            }
            else
            {
                await SetCacheAndInvokeNext(context, key);
            }
        }

        private async Task GetResponseFromCache(HttpContext context, byte[] cacheData)
        {
            _logger.LogInformation("taking data from cache");
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

    public static class CachingExtensions
    {
        public static IApplicationBuilder UseCaching(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CachingMiddleware>();
        }
    }
}