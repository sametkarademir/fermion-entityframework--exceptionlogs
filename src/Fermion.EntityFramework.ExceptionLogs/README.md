# Fermion.EntityFramework.ExceptionLogs

Fermion.EntityFramework.ExceptionLogs is a robust exception logging library for .NET applications, providing detailed tracking, filtering, and management of application exceptions. Built on top of Entity Framework Core and following clean architecture principles, it enables advanced exception analysis and operational insights.

---

## Features

- Centralized exception logging with rich context (fingerprint, stack trace, correlation/session IDs, etc.)
- RESTful API for querying, filtering, and cleaning up exception logs
- Advanced filtering by exception type, code, log level, status code, date range, and more
- Custom middleware for global exception handling and logging
- Extensible exception handling for authentication, authorization, business, validation, and user-friendly errors
- Custom Serilog sink for forwarding logs to external systems (e.g., webhooks)
- Auditing support for creation time and user
- Automatic cleanup of old logs

---

## Installation

```bash
  dotnet add package Fermion.EntityFramework.ExceptionLogs
```

---

## Project Structure

The library follows Clean Architecture principles with the following layers:

### Core
- Base entities and interfaces
- Domain models (`ExceptionLog`)
- Configuration options

### Infrastructure
- Entity Framework Core configurations
- Database context implementations
- Repository implementations

### Application
- DTOs
- Interfaces
- Services
- Mappings

### Presentation
- Controllers
- API endpoints
- Request/Response models

### DependencyInjection
- Service registration extensions
- Configuration options
- Middleware implementation
- Exception handling and logging

---

## Configuration

```csharp
// Register exception log services in Program.cs or Startup.cs
builder.AddFermionExceptionLogServices<ApplicationDbContext>(opt =>
{
    opt.SerilogEnabled = true;
    opt.ConsoleEnabled = true;
    opt.FileEnabled = true;
    opt.WebhookOptions.WebhookEnabled = false;
    opt.WebhookOptions.WebhookUrl = "https://example.com/webhook";
    opt.WebhookOptions.Method = "POST";
    opt.WebhookOptions.BatchSizeLimit = 50;
    opt.WebhookOptions.Period = TimeSpan.FromSeconds(5);
    opt.WebhookOptions.QueueLimit = 500;
    opt.WebhookOptions.RestrictedToMinimumLevel = Serilog.Events.LogEventLevel.Error;
});

// Configure DbContext
public class YourDbContext : DbContext 
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Apply ExceptionLog configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExceptionLogConfiguration).Assembly);
    }
}

// Configure middleware in Program.cs
var app = builder.Build();

// Add exception logging middleware (should be one of the first middleware)
app.UseFermionExceptionMiddleware();

// Add other middleware
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
```

---

## API Endpoints

The library provides the following RESTful API endpoints when `EnableApiEndpoints` is set to true:

- `GET /api/exception-logs/{id}` - Get specific exception log details
- `GET /api/exception-logs/pageable` - Get exception logs with filtering and pagination
- `DELETE /api/exception-logs/cleanup` - Clean up old exception logs

---