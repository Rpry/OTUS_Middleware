using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Middleware.Middlewares
{
    public class RequestCultureMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestCultureMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,
            ILogger<RequestCultureMiddleware> logger)
        {
            var cultureQuery = context.Request.Query["culture"];
            if (!string.IsNullOrWhiteSpace(cultureQuery))
            {
                var culture = new CultureInfo(cultureQuery);

                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }

            // Вызов следующего мидлваре в конвейере
            await _next(context);
        }
    }

    public static class Extensions
    {
        public static IApplicationBuilder UseRequestCulture(this IApplicationBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.UseMiddleware<RequestCultureMiddleware>();
        }
    }
}