using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace FunWithMiddleware.Middlewares
{
  public class RequestCultureMiddleware
  {
    private readonly RequestDelegate _next;

    public RequestCultureMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      var cultureQuery = context.Request.Query["culture"];
      if (!string.IsNullOrWhiteSpace(cultureQuery))
      {
        var culture = new CultureInfo(cultureQuery);

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
      }
      
      // Вызов следующего мидлваре в конвеере
      await _next(context);

    }
  }

  public static class Extensions
  {
    public static IApplicationBuilder UseRequestCulture(this IApplicationBuilder builder)
    {
      if (builder == null)
      {
        throw new ArgumentNullException(nameof(builder));
      }
      
      return builder.UseMiddleware<RequestCultureMiddleware>();
    }
    
    public static Stream GenerateStreamFromString(string s)
    {
      var stream = new MemoryStream();
      var writer = new StreamWriter(stream);
      writer.Write(s);
      writer.Flush();
      stream.Position = 0;
      return stream;
    }
    
  }
  

}