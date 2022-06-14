using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FunWithMiddleware.Middlewares
{
  public class LoggingMiddleware
  {
    private readonly RequestDelegate _next;

    private const string MessageTemplate = "HTTP {RequestMethod} {RequestPath} {Query} {QueryString} " +
                                           "responded {StatusCode} in {Elapsed}";

    public LoggingMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, ILogger<LoggingMiddleware> logger)
    {
      Stopwatch timer = new Stopwatch();
      timer.Start();
      await _next(httpContext);
      timer.Stop();
      var elapsedMs = timer.Elapsed;

      logger.Log(LogLevel.Information, MessageTemplate,
        httpContext.Request.Method, httpContext.Request.Path,
        httpContext.Request.Query, httpContext.Request.QueryString,
        httpContext.Response?.StatusCode, elapsedMs.TotalMilliseconds
      );
    }
  }

  public static class LoggingMiddlewareAppExtensions
  {
    public static IApplicationBuilder UseHttpRequestLogging(this IApplicationBuilder applicationBuilder)
    {
      return applicationBuilder.UseMiddleware<LoggingMiddleware>();
    }
  }
}