using Fermion.Domain.Exceptions.Types;

namespace Fermion.EntityFramework.ExceptionLogs.DependencyInjection.ExceptionHandling.Handlers;

public abstract class ExceptionHandler
{
    public abstract Task HandleException(AppAuthenticationException exception);
    public abstract Task HandleException(AppAuthorizationException exception);
    public abstract Task HandleException(AppBusinessException exception);
    public abstract Task HandleException(AppEntityNotFoundException exception);
    public abstract Task HandleException(AppUserFriendlyException exception);
    public abstract Task HandleException(AppValidationException exception);
    public abstract Task HandleException(Exception exception, string? correlationId = null);
}