using Fermion.Domain.Exceptions.Models;

namespace Fermion.EntityFramework.ExceptionLogs.DependencyInjection.ExceptionHandling.Abstracts;

public abstract class AppProblemDetails
{
    protected AppProblemDetails(string? code, string? message, string? details, int? statusCode, string? correlationId, List<ValidationExceptionModel>? validationErrors)
    {
        Code = code;
        Message = message;
        Details = details;
        StatusCode = statusCode;
        CorrelationId = correlationId;
        ValidationErrors = validationErrors;
    }

    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public int? StatusCode { get; set; }
    public string? CorrelationId { get; set; }
    public List<ValidationExceptionModel>? ValidationErrors { get; set; }

}