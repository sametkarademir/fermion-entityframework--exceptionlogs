using System.Reflection;
using Fermion.Domain.Shared.Conventions;
using Fermion.EntityFramework.ExceptionLogs.Application.Services;
using Fermion.EntityFramework.ExceptionLogs.DependencyInjection.Logging;
using Fermion.EntityFramework.ExceptionLogs.Domain.Interfaces.Repositories;
using Fermion.EntityFramework.ExceptionLogs.Domain.Interfaces.Services;
using Fermion.EntityFramework.ExceptionLogs.Domain.Options;
using Fermion.EntityFramework.ExceptionLogs.Infrastructure.Repositories;
using Fermion.EntityFramework.ExceptionLogs.Presentation.Controllers;
using Fermion.EntityFramework.ExceptionLogs.Presentation.Filters;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;

namespace Fermion.EntityFramework.ExceptionLogs.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFermionExceptionLogServices<TContext>(this WebApplicationBuilder builder, Action<ExceptionLogOptions> configureOptions) where TContext : DbContext
    {
        var options = new ExceptionLogOptions();
        configureOptions.Invoke(options);
        builder.Services.Configure<ExceptionLogOptions>(configureOptions.Invoke);

        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddScoped<IExceptionLogRepository, ExceptionLogRepository<TContext>>();
        builder.Services.AddScoped<IExceptionLogAppService, ExceptionLogAppService>();
        builder.Services.Configure<ApiBehaviorOptions>(apiBehaviorOptions => { apiBehaviorOptions.SuppressModelStateInvalidFilter = true; });
        
        builder.Services.AddControllers(mvcOptions =>
            {
                mvcOptions.Filters.Add<ValidationActionFilter>();

                if (options.ExceptionLogController.Enabled)
                {
                    mvcOptions.Conventions.Add(new ControllerAuthorizationConvention(
                        typeof(ExceptionLogController),
                        options.ExceptionLogController.Route,
                        options.ExceptionLogController.GlobalAuthorization,
                        options.ExceptionLogController.Endpoints
                    ));
                }
                else
                {
                    mvcOptions.Conventions.Add(new ControllerDisablingConvention(typeof(ExceptionLogController)));
                    mvcOptions.Conventions.Add(new ControllerRemovalConvention(typeof(ExceptionLogController)));
                }
            })
            .ConfigureApplicationPartManager(manager =>
            {
                if (options.ExceptionLogController.Enabled)
                {
                    manager.ApplicationParts.Add(new AssemblyPart(typeof(ExceptionLogController).Assembly));
                }
            });

        if (options.Serilog.Enabled)
        {
            builder.Logging.ClearProviders();
            builder.Host.UseSerilog((context, services, configuration) =>
            {
                var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
                var httpContextEnricher = new HttpContextEnricher(httpContextAccessor);

                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId()
                    .Enrich.WithProcessId()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithExceptionDetails()
                    .Enrich.With(httpContextEnricher);

                if (options.Serilog.WebhookOptions.Enabled && !string.IsNullOrEmpty(options.Serilog.WebhookOptions.Url))
                {
                    configuration.WriteTo.CustomWebhook(
                        webhookUrl: options.Serilog.WebhookOptions.Url,
                        method: options.Serilog.WebhookOptions.Method,
                        headers: options.Serilog.WebhookOptions.Headers,
                        batchSizeLimit: options.Serilog.WebhookOptions.BatchSizeLimit,
                        period: options.Serilog.WebhookOptions.Period,
                        queueLimit: options.Serilog.WebhookOptions.QueueLimit,
                        restrictedToMinimumLevel: options.Serilog.WebhookOptions.RestrictedToMinimumLevel
                    );
                }

                if (options.Serilog.Console.Enabled)
                {
                    configuration.WriteTo.Console(
                        outputTemplate: options.Serilog.Console.OutputTemplate,
                        theme: options.Serilog.Console.Theme,
                        restrictedToMinimumLevel: options.Serilog.Console.MinimumLevel
                    );
                }

                if (options.Serilog.File.Enabled)
                {
                    configuration.WriteTo.File(
                            path: options.Serilog.File.PathToTxt,
                            rollingInterval: options.Serilog.File.RollingInterval,
                            outputTemplate: options.Serilog.File.OutputTemplate
                        )
                        .WriteTo.File(
                            new CompactJsonFormatter(),
                            options.Serilog.File.PathToJson,
                            rollingInterval: options.Serilog.File.RollingInterval
                        );
                }
            });
        }

        return builder.Services;
    }
}