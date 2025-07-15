using System.Text.Json;
using Fermion.EntityFramework.ExceptionLogs.DependencyInjection.ExceptionHandling.Abstracts;
using Fermion.EntityFramework.ExceptionLogs.DependencyInjection.ExceptionHandling.HttpProblemDetails;
using Fermion.Domain.Exceptions.Types;
using Microsoft.AspNetCore.Http;

namespace Fermion.EntityFramework.ExceptionLogs.DependencyInjection.ExceptionHandling.Handlers;

public class HttpExceptionHandler : ExceptionHandler
{
    public HttpResponse Response
    {
#pragma warning disable S112 // General or reserved exceptions should never be thrown
        get => _response ?? throw new NullReferenceException(nameof(_response));
#pragma warning restore S112 // General or reserved exceptions should never be thrown
        set => _response = value;
    }
    private HttpResponse? _response;

    public override Task HandleException(AppAuthenticationException exception)
    {
        var details = new AppAuthenticationProblemDetails(
            exception.Code,
            exception.Message,
            exception.Details,
            exception.StatusCode,
            exception.CorrelationId,
            null).ToJson();

        Response.StatusCode = exception.StatusCode;
        return Response.WriteAsync(details);
    }

    public override Task HandleException(AppAuthorizationException exception)
    {
        var details = new AppAuthorizationProblemDetails(
            exception.Code,
            exception.Message,
            exception.Details,
            exception.StatusCode,
            exception.CorrelationId,
            null).ToJson();

        Response.StatusCode = exception.StatusCode;
        return Response.WriteAsync(details);
    }

    public override Task HandleException(AppBusinessException exception)
    {
        var details = new AppBusinessProblemDetails(
            exception.Code,
            exception.Message,
            exception.Details,
            exception.StatusCode,
            exception.CorrelationId,
            null).ToJson();

        Response.StatusCode = exception.StatusCode;
        return Response.WriteAsync(details);
    }

    public override Task HandleException(AppEntityNotFoundException exception)
    {
        var details = new AppEntityNotFoundProblemDetails(
            exception.Code,
            exception.Message,
            exception.Details,
            exception.StatusCode,
            exception.CorrelationId,
            null).ToJson();

        Response.StatusCode = exception.StatusCode;
        return Response.WriteAsync(details);
    }

    public override Task HandleException(AppUserFriendlyException exception)
    {
        var details = new AppUserFriendlyProblemDetails(
            exception.Code,
            exception.Message,
            exception.Details,
            exception.StatusCode,
            exception.CorrelationId,
            null).ToJson();

        Response.StatusCode = exception.StatusCode;
        return Response.WriteAsync(details);
    }

    public override Task HandleException(AppValidationException exception)
    {
        var details = new AppValidationProblemDetails(
            exception.Code,
            exception.Message,
            exception.Details,
            exception.StatusCode,
            exception.CorrelationId,
            exception.Errors.ToList()).ToJson();

        Response.StatusCode = exception.StatusCode;
        return Response.WriteAsync(details);
    }

    public override Task HandleException(Exception exception, string? correlationId = null)
    {
        var details = new AppBusinessProblemDetails(
            "APP:EXCEPTION:1000",
            "An unexpected error occurred.",
            exception.Message,
            StatusCodes.Status500InternalServerError,
            correlationId,
            null).ToJson();

        Response.StatusCode = StatusCodes.Status500InternalServerError;
        return Response.WriteAsync(details);
    }
}

public static class ProblemDetailsExtensions
{
    public static string ToJson<TProblemDetail>(this TProblemDetail details)
        where TProblemDetail : AppProblemDetails
    {
        return JsonSerializer.Serialize(details);
    }
}
