using Fermion.EntityFramework.ExceptionLogs.DependencyInjection.ExceptionHandling.Abstracts;
using Fermion.Domain.Exceptions.Models;

namespace Fermion.EntityFramework.ExceptionLogs.DependencyInjection.ExceptionHandling.HttpProblemDetails;

public class AppValidationProblemDetails : AppProblemDetails
{
    public AppValidationProblemDetails(
        string? code,
        string? message,
        string? details,
        int? statusCode,
        string? correlationId,
        List<ValidationExceptionModel>? validationErrors)
        : base(code, message, details, statusCode, correlationId, validationErrors)
    {
    }
}