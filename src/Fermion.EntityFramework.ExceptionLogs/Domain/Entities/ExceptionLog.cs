using Fermion.Domain.Shared.Auditing;
using Fermion.Domain.Shared.Filters;
using Fermion.Domain.Shared.Interfaces;

namespace Fermion.EntityFramework.ExceptionLogs.Domain.Entities;

[ExcludeFromProcessing]
public class ExceptionLog : CreationAuditedEntity<Guid>, IEntitySnapshotId, IEntitySessionId, IEntityCorrelationId
{
    public string Fingerprint { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }
    public string? ExceptionType { get; set; }
    public string? Message { get; set; }
    public string? Source { get; set; }
    public string? StackTrace { get; set; }
    public string? InnerExceptions { get; set; }
    public string? ExceptionData { get; set; }

    public string? Code { get; set; }
    public string? Details { get; set; }
    public int StatusCode { get; set; } = 500;

    public Guid? SnapshotId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid? CorrelationId { get; set; }
}