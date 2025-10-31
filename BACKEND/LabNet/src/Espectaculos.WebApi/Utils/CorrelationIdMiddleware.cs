using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Espectaculos.WebApi.Utils;

public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var incoming = context.Request.Headers[HeaderName].ToString();
        var activityId = Activity.Current?.TraceId.ToString();
        var correlationId = !string.IsNullOrWhiteSpace(incoming)
            ? incoming
            : (activityId ?? Guid.NewGuid().ToString("N"));

    context.Response.Headers[HeaderName] = correlationId;
    // Guardar también en Items para que la instrumentación OTel pueda enriquecer la Activity
    context.Items["CorrelationId"] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
