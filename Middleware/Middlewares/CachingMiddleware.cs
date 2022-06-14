using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FunWithMiddleware.Middlewares
{
  public class CachingMiddleware
  {
    private readonly RequestDelegate _next;
    private int _counter;

    public CachingMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IMemoryCache memoryCache)
    {
      context.Request.EnableBuffering();
      var key = context.Request.Path.ToString();
      var cache = memoryCache.Get(key) as byte[];
      if (cache != null)
      {
        await context.Response.WriteAsync(Encoding.GetEncoding("windows-1251").GetString(cache));
      }
      else
      {
        var responseStream = context.Response.Body;
        await using var ms = new MemoryStream();

        context.Response.Body = ms;
        await _next(context);

        memoryCache.Set(key, ms.ToArray(), TimeSpan.FromSeconds(30));
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