using System;

namespace Middleware.Options;

public sealed class SimpleRateLimiterOptions
{
    public const string SectionName = "SimpleRateLimiter";
    
    public int IntervalSeconds { get; set; } = 5;
    public string ClientIdentifierHeader { get; set; } = "IP";
    public string CacheKeyPrefix { get; set; } = "rateLimiting_";

    public TimeSpan Interval => TimeSpan.FromSeconds(IntervalSeconds);
}

