using System;
using System.Collections.Generic;

namespace Middleware.Options;

public sealed class SimpleCachingOptions
{
    public const string SectionName = "SimpleCaching";
    
    public HashSet<string> CacheablePaths { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public int CacheDurationSeconds { get; set; } = 5;
    public string CacheKeyPrefix { get; set; } = "caching_";

    public TimeSpan CacheDuration => TimeSpan.FromSeconds(CacheDurationSeconds);
}

