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

        public CachingMiddleware(RequestDelegate next, ILogger<CachingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context,
            IMemoryCache memoryCache)
        {
            context.Request.EnableBuffering();
            var key = $"caching_{context.Request.Path.ToString()}";
            var cache = memoryCache.Get<byte[]>(key);
            if (cache != null)
                await GetDataFromCache(context, memoryCache, key);
            else
                await SaveDataToCacheEndExecuteNext(context, memoryCache, _next);
        }

        private async Task GetDataFromCache(HttpContext context,
            IMemoryCache memoryCache, string key)
        {
            var cache = memoryCache.Get<byte[]>(key);
            var responseStream = context.Response.Body;
            _logger.LogInformation("taking data from cache");
            context.Response.Body = responseStream;
            context.Response.Headers.Append("Content-Type", "text/plain; charset=utf-8");
            await context.Response.Body.WriteAsync(cache);
        }

        private async Task SaveDataToCacheEndExecuteNext(HttpContext context,
            IMemoryCache memoryCache, RequestDelegate requestDelegate)
        {
            var responseStream = context.Response.Body;
            var key = $"caching_{context.Request.Path.ToString()}";
            _logger.LogInformation("taking data from action method");
            await using var ms = new MemoryStream();
            context.Response.Body = ms;
            await requestDelegate(context);
            memoryCache.Set(key, ms.ToArray(),
                TimeSpan.FromSeconds(5));
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