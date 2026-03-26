using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Options;

namespace Middleware.Middlewares;

public sealed class SimpleCachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SimpleCachingMiddleware> _logger;
    private readonly SimpleCachingOptions _options;

    public SimpleCachingMiddleware(
        RequestDelegate next,
        ILogger<SimpleCachingMiddleware> logger,
        IOptions<SimpleCachingOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context, IMemoryCache memoryCache)
    {
        if (!ShouldCache(context))
        {
            await _next(context);
            return;
        }

        var cacheKey = GetCacheKey(context.Request.Path);

        if (memoryCache.TryGetValue<byte[]>(cacheKey, out var cachedResponse))
        {
            await WriteCachedResponse(context, cachedResponse);
            return;
        }

        await ExecuteAndCacheResponse(context, memoryCache, cacheKey);
    }

    private bool ShouldCache(HttpContext context)
    {
        return _options.CacheablePaths.Contains(context.Request.Path.Value ?? string.Empty);
    }

    private string GetCacheKey(PathString path)
    {
        return $"{_options.CacheKeyPrefix}{path}";
    }

    private async Task WriteCachedResponse(HttpContext context, byte[] cachedData)
    {
        _logger.LogInformation("Returning cached response for {Path}", context.Request.Path);
        
        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.Body.WriteAsync(cachedData);
    }

    private async Task ExecuteAndCacheResponse(HttpContext context, IMemoryCache memoryCache, string cacheKey)
    {
        _logger.LogInformation("Executing request and caching response for {Path}", context.Request.Path);

        var originalBodyStream = context.Response.Body;

        await using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context);

        var responseBytes = memoryStream.ToArray();
        memoryCache.Set(cacheKey, responseBytes, _options.CacheDuration);

        context.Response.Body = originalBodyStream;
        await originalBodyStream.WriteAsync(responseBytes);
    }
}

public static class CachingExtensions
{
    public static IApplicationBuilder UseSimpleCaching(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SimpleCachingMiddleware>();
    }

    public static IServiceCollection AddSimpleCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SimpleCachingOptions>(
            configuration.GetSection(SimpleCachingOptions.SectionName));
        return services;
    }
}