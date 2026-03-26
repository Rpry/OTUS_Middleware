using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Middleware.Options;

namespace Middleware.Middlewares;

public sealed class SimpleRateLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SimpleRateLimiterOptions _options;

    public SimpleRateLimiterMiddleware(
        RequestDelegate next,
        IOptions<SimpleRateLimiterOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context, IMemoryCache memoryCache)
    {
        var clientId = GetClientIdentifier(context);
        var cacheKey = $"{_options.CacheKeyPrefix}{clientId}";
        var now = DateTime.UtcNow;

        if (IsRateLimited(memoryCache, cacheKey, now, out var retryAfter))
        {
            await WriteRateLimitedResponse(context, retryAfter);
            return;
        }

        memoryCache.Set(cacheKey, now, _options.Interval);
        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        return context.Request.Headers[_options.ClientIdentifierHeader].ToString();
    }

    private bool IsRateLimited(IMemoryCache cache, string key, DateTime now, out TimeSpan retryAfter)
    {
        retryAfter = TimeSpan.Zero;

        if (!cache.TryGetValue<DateTime>(key, out var lastRequestTime))
            return false;

        var elapsed = now - lastRequestTime;
        if (elapsed >= _options.Interval)
            return false;

        retryAfter = _options.Interval - elapsed;
        return true;
    }

    private static async Task WriteRateLimitedResponse(HttpContext context, TimeSpan retryAfter)
    {
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.Headers["Retry-After"] = Math.Ceiling(retryAfter.TotalSeconds).ToString();
        await context.Response.WriteAsync("Too many requests. Please try again later.");
    }
}

public static class RateLimiterExtensions
{
    public static IApplicationBuilder UseSimpleRateLimiter(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SimpleRateLimiterMiddleware>();
    }

    public static IServiceCollection AddSimpleRateLimiter(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SimpleRateLimiterOptions>(
            configuration.GetSection(SimpleRateLimiterOptions.SectionName));
        return services;
    }
}