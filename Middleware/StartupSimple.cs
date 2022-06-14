using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FunWithMiddleware
{
  public class StartupSimple
  {
    public void Configure(IApplicationBuilder app)
    {
      app.Use(async (context, next) =>
      {
        if (context.Request.Path == "/")
        {
          await context
            .Response
            .WriteAsync("Use GRPC ");
        }
        else
        {
          await next();
        }
      });
    }
  }
}