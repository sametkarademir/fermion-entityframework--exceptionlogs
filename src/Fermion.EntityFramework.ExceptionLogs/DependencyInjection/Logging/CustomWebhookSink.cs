using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Fermion.EntityFramework.ExceptionLogs.DependencyInjection.Logging;

public class CustomWebhookSink : IBatchedLogEventSink
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;
    private readonly IFormatProvider? _formatProvider;
    private readonly string _method;
    private readonly Dictionary<string, string> _headers;
    private readonly LogEventLevel _restrictedToMinimumLevel;

    public CustomWebhookSink(
        string webhookUrl,
        string method = "POST",
        Dictionary<string, string>? headers = null,
        IFormatProvider? formatProvider = null,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information)
    {
        _webhookUrl = webhookUrl;
        _method = method;
        _formatProvider = formatProvider;
        _headers = headers ?? new Dictionary<string, string>();
        _restrictedToMinimumLevel = restrictedToMinimumLevel;

        _httpClient = new HttpClient();

        // Default headers
        if (!_headers.ContainsKey("Content-Type"))
        {
            _headers["Content-Type"] = "application/json";
        }

        // Custom headers
        foreach (var header in _headers)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    public async Task EmitBatchAsync(IReadOnlyCollection<LogEvent> batch)
    {
        if (batch.Count == 0) return;

        try
        {
            var payload = CreatePayload(batch);
            var json = System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod(_method), _webhookUrl)
            {
                Content = content
            };

            if (_headers.ContainsKey("Content-Type"))
            {
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(_headers["Content-Type"]);
            }

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Webhook sink error: HTTP {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CustomWebhookSink: {ex.Message}");
        }
    }

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }

    private object CreatePayload(IEnumerable<LogEvent> logEvents)
    {
        var logs = logEvents.Where(item => item.Level >= _restrictedToMinimumLevel).Select(logEvent => new
        {
            timestamp = logEvent.Timestamp,
            level = logEvent.Level.ToString(),
            message = logEvent.RenderMessage(_formatProvider),
            exception = logEvent.Exception?.ToString(),
            exceptionType = logEvent.Exception?.GetType().Name,
            sourceContext = GetSourceContext(logEvent),
            properties = logEvent.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString())
        }).ToList();

        return new
        {
            batchId = Guid.NewGuid(),
            timestamp = DateTime.UtcNow,
            batchSize = logs.Count,
            logs
        };
    }

    private string? GetSourceContext(LogEvent logEvent)
    {
        return logEvent.Properties.TryGetValue("SourceContext", out var sourceContext)
            ? sourceContext.ToString().Trim('"')
            : null;
    }
}

public static class CustomWebhookSinkExtensions
{
    public static LoggerConfiguration CustomWebhook(
        this LoggerSinkConfiguration loggerConfiguration,
        string webhookUrl,
        string method = "POST",
        Dictionary<string, string>? headers = null,
        int batchSizeLimit = 100,
        TimeSpan? period = null,
        int queueLimit = 1000,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information,
        IFormatProvider? formatProvider = null)
    {
        var sink = new CustomWebhookSink(webhookUrl, method, headers, formatProvider, restrictedToMinimumLevel);

        return loggerConfiguration.Sink(sink, new BatchingOptions
        {
            EagerlyEmitFirstEvent = true,
            BatchSizeLimit = batchSizeLimit,
            BufferingTimeLimit = period ?? TimeSpan.FromSeconds(10),
            QueueLimit = queueLimit
        });
    }
}