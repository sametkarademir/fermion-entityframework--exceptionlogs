using System.Net.Mime;
using Fermion.EntityFramework.ExceptionLogs.DependencyInjection.ExceptionHandling.Handlers;
using Fermion.EntityFramework.ExceptionLogs.Domain.Entities;
using Fermion.EntityFramework.ExceptionLogs.Domain.Interfaces.Repositories;
using Fermion.Domain.Exceptions.Types;
using Fermion.Domain.Extensions.Exceptions;
using Fermion.Domain.Extensions.HttpContexts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fermion.EntityFramework.ExceptionLogs.DependencyInjection;

public class ExceptionMiddleware(RequestDelegate next)
{
    private readonly HttpExceptionHandler _httpExceptionHandler = new();

    public async Task Invoke(HttpContext context, ILogger<ExceptionMiddleware> logger, IExceptionLogRepository exceptionLogRepository)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context.Response, exception, context);

            logger.LogError(exception, exception.Message);

            try
            {
                await exceptionLogRepository.AddAsync(new ExceptionLog
                {
                    Fingerprint = exception.GenerateFingerprint(),
                    Timestamp = DateTime.UtcNow,
                    ExceptionType = exception.GetExceptionType(),
                    Message = exception.Message,
                    Source = exception.Source,
                    StackTrace = exception.StackTrace,
                    InnerExceptions = exception.ConvertInnerExceptionsToJson(),
                    ExceptionData = exception.ConvertExceptionDataToJson(),
                    Code = exception is AppException appExceptionCode ? appExceptionCode.Code : "APP:UNKNOWN:1000",
                    Details = exception is AppException appExceptionDetails
                        ? appExceptionDetails.Details
                        : "An unknown error occurred.",
                    StatusCode = exception is AppException appExceptionStatusCode
                        ? appExceptionStatusCode.StatusCode
                        : 500,
                    SnapshotId = context.GetSnapshotId(),
                    SessionId = context.GetSessionId(),
                    CorrelationId = context.GetCorrelationId(),
                });
                await exceptionLogRepository.SaveChangesAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to log exception to the database.");
            }
        }
    }
    protected virtual Task HandleExceptionAsync(HttpResponse response, dynamic exception, HttpContext context)
    {
        response.ContentType = MediaTypeNames.Application.Json;
        _httpExceptionHandler.Response = response;

        return exception switch
        {
            AppAuthenticationException authEx => _httpExceptionHandler.HandleException(authEx),
            AppAuthorizationException authEx => _httpExceptionHandler.HandleException(authEx),
            AppBusinessException businessEx => _httpExceptionHandler.HandleException(businessEx),
            AppEntityNotFoundException notFoundEx => _httpExceptionHandler.HandleException(notFoundEx),
            AppUserFriendlyException userFriendlyEx => _httpExceptionHandler.HandleException(userFriendlyEx),
            AppValidationException validationEx => _httpExceptionHandler.HandleException(validationEx),

            _ => HandleGenericExceptionAsync(exception, context)
        };
    }

    private async Task HandleGenericExceptionAsync(Exception exception, HttpContext context)
    {
        await _httpExceptionHandler.HandleException(exception, context.GetCorrelationId().ToString());
    }
}