using Fermion.Domain.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace Fermion.EntityFramework.ExceptionLogs.Application.DTOs.ExceptionLogs;

public class ExceptionLogResponseDto : CreationAuditedEntityDto<Guid>
{
    public DateTime Timestamp { get; set; }
    public string? ExceptionType { get; set; }
    public string? Message { get; set; }
    public string? Source { get; set; }
    public string? StackTrace { get; set; }
    public string? InnerExceptions { get; set; }
    public string? ExceptionData { get; set; }

    public string? Code { get; set; }
    public string? Details { get; set; }
    public int StatusCode { get; set; }

    public Guid? SnapshotId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid? CorrelationId { get; set; }
}