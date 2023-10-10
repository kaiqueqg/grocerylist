using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class JwtTokenLoggingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<JwtTokenLoggingMiddleware> _logger;

  public JwtTokenLoggingMiddleware(RequestDelegate next, ILogger<JwtTokenLoggingMiddleware> logger)
  {
    _next = next ?? throw new ArgumentNullException(nameof(next));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public async Task InvokeAsync(HttpContext context)
  {
    _logger.LogInformation("JwtTokenLoggingMiddleware");
    if(context.Request.Headers.ContainsKey("Authorization"))
    {
      var jwtToken = context.Request.Headers["Authorization"].ToString();
      _logger.LogInformation("JWT Token received: " + jwtToken);
    }

    // Call the next middleware in the pipeline
    await _next(context);
  }
}
