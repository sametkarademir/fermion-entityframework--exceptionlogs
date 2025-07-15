using Microsoft.AspNetCore.Builder;

namespace Fermion.EntityFramework.ExceptionLogs.DependencyInjection;

public static class ApplicationBuilderExceptionMiddlewareExtensions
{
    public static void FermionExceptionLogMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
    }
}