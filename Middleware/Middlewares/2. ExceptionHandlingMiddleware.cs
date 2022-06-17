using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Middleware.Middlewares
{
  public class HttpStatusCodeExceptionMiddleware
  {
    private readonly RequestDelegate _next;

    public HttpStatusCodeExceptionMiddleware(RequestDelegate next)
    {
      _next = next;
    }
    
    public async Task Invoke(HttpContext context, 
      ILogger<HttpStatusCodeExceptionMiddleware> logger,
      IWebHostEnvironment webHostEnvironment)
    {
      context.Request.EnableBuffering();
      try
      {
        await _next(context);
      }
      catch (Exception ex)
      {
        await HandleExceptionAsync(context, ex, logger, webHostEnvironment);
      }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex,
      ILogger<HttpStatusCodeExceptionMiddleware> logger, IWebHostEnvironment webHostEnvironment)
    {
      logger.LogError(ex,  ex.Message);
      string errorMessageDetails = string.Empty;
      if (!webHostEnvironment.IsProduction())
      {
        errorMessageDetails = ex.Message;
      }
      else
      {
        errorMessageDetails = "ошибка. Пожалуйста, обратитесь к администратору.";  
      }
      var result = JsonConvert.SerializeObject(new { error = errorMessageDetails });
      context.Response.ContentType = "application/json";
      context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return context.Response.WriteAsync(result);
    }
  }

  // Extension method used to add the middleware to the HTTP request pipeline.
  public static class HttpStatusCodeExceptionMiddlewareExtensions
  {
    public static IApplicationBuilder UseHttpStatusCodeExceptionMiddleware(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<HttpStatusCodeExceptionMiddleware>();
    }
  }
}