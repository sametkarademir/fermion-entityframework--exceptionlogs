using Fermion.Domain.Extensions.HttpContexts;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Fermion.EntityFramework.ExceptionLogs.DependencyInjection.Logging;

public class HttpContextEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null) return;

        var correlationId = httpContext.GetCorrelationId();
        if (correlationId == null)
        {
            correlationId = Guid.NewGuid();
            httpContext.SetCorrelationId(correlationId.Value);
        }

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
    }
}