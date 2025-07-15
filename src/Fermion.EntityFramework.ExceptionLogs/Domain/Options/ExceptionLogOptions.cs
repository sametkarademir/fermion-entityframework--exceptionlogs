using Fermion.Domain.Shared.Conventions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Fermion.EntityFramework.ExceptionLogs.Domain.Options;

public class ExceptionLogOptions
{
    public SerilogOptions Serilog { get; set; } = new SerilogOptions();
    public ExceptionLogControllerOptions ExceptionLogController { get; set; } = new();
}

public class ExceptionLogControllerOptions
{
    /// <summary>
    /// If true, the AuditLogController will be enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Route for the AuditLogController
    /// </summary>
    public string Route { get; set; } = "api/exception-logs";
    
    /// <summary>
    /// Authorization settings for AuditLog Controller
    /// </summary>
    public AuthorizationOptions GlobalAuthorization { get; set; } = new()
    {
        RequireAuthentication = true,
        Policy = null,
        Roles = null
    };
    
    /// <summary>
    /// Endpoint-specific authorization settings for AuditLog Controller
    /// </summary>
    public List<EndpointOptions>? Endpoints { get; set; }
}

public class SerilogOptions
{
    public bool Enabled { get; set; } = true;
    public ConsoleOptions Console { get; set; } = new();
    public FileOptions File { get; set; } = new();
    public WebhookOptions WebhookOptions { get; set; } = new();
}

public class ConsoleOptions
{
    public bool Enabled { get; set; } = true;
    public string OutputTemplate { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
    public ConsoleTheme Theme { get; set; } = AnsiConsoleTheme.Code;
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Verbose;
}

public class FileOptions
{
    public bool Enabled { get; set; } = false;
    public string PathToTxt { get; set; } = "logs/txt-.txt";
    public string PathToJson { get; set; } = "logs/json-.json";
    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;
    public string OutputTemplate { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{Properties:j}{NewLine}{Exception}";
}

public class WebhookOptions
{
    public bool Enabled { get; set; } = true;
    public string? Url { get; set; }
    public string Method { get; set; } = "POST";
    public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>()
    {
        { "Content-Type", "application/json" }
    };
    public int BatchSizeLimit { get; set; } = 50;
    public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(5);
    public int QueueLimit { get; set; } = 500;
    public LogEventLevel RestrictedToMinimumLevel { get; set; } = LogEventLevel.Error;
}