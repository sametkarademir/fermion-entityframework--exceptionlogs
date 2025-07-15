using AutoMapper;
using Fermion.EntityFramework.ExceptionLogs.Application.DTOs.ExceptionLogs;
using Fermion.EntityFramework.ExceptionLogs.Domain.Interfaces.Repositories;
using Fermion.EntityFramework.ExceptionLogs.Domain.Interfaces.Services;
using Fermion.EntityFramework.Shared.DTOs.Pagination;
using Fermion.EntityFramework.Shared.Extensions;
using Fermion.Domain.Extensions.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fermion.EntityFramework.ExceptionLogs.Application.Services;

public class ExceptionLogAppService(
    IExceptionLogRepository exceptionLogRepository,
    IMapper mapper,
    ILogger<ExceptionLogAppService> logger)
    : IExceptionLogAppService
{
    public async Task<ExceptionLogResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedExceptionLog = await exceptionLogRepository.GetAsync(
            id: id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        return mapper.Map<ExceptionLogResponseDto>(matchedExceptionLog);
    }

    public async Task<PageableResponseDto<ExceptionLogResponseDto>> GetPageableAndFilterAsync(GetListExceptionLogRequestDto request, CancellationToken cancellationToken = default)
    {
        var queryable = exceptionLogRepository.GetQueryable();
        queryable = queryable.WhereIf(!string.IsNullOrEmpty(request.ExceptionType), item => item.ExceptionType != null && item.ExceptionType.Contains(request.ExceptionType!));
        queryable = queryable.WhereIf(!string.IsNullOrEmpty(request.Code), item => item.Code != null && item.Code.Contains(request.Code!));
        queryable = queryable.WhereIf(request.StatusCode.HasValue, item => item.StatusCode == request.StatusCode);
        queryable = queryable.WhereIf(request.StartDate.HasValue, item => item.Timestamp >= request.StartDate);
        queryable = queryable.WhereIf(request.EndDate.HasValue, item => item.Timestamp >= request.EndDate);
        queryable = queryable.WhereIf(request.SnapshotId.HasValue, item => item.SnapshotId == request.SnapshotId);
        queryable = queryable.WhereIf(request.SessionId.HasValue, item => item.SessionId == request.SessionId);
        queryable = queryable.WhereIf(request.CorrelationId.HasValue, item => item.CorrelationId == request.CorrelationId);

        queryable = queryable.AsNoTracking();
        queryable = queryable.ApplySort(request.Field, request.Order, cancellationToken);
        var result = await queryable.ToPageableAsync(request.Page, request.PerPage, cancellationToken);
        var mappedExceptionLogs = mapper.Map<List<ExceptionLogResponseDto>>(result.Data);

        return new PageableResponseDto<ExceptionLogResponseDto>(mappedExceptionLogs, result.Meta);
    }

    public async Task<int> CleanupOldExceptionLogsAsync(DateTime olderThan, bool isArchive = true, CancellationToken cancellationToken = default)
    {
        var queryable = exceptionLogRepository.GetQueryable();
        queryable = queryable.Where(x => x.CreationTime < olderThan);
        var countToDelete = await queryable.CountAsync(cancellationToken: cancellationToken);
        if (countToDelete == 0)
        {
            return 0;
        }

        const int batchSize = 200;
        var totalDeleted = 0;
        while (countToDelete > totalDeleted)
        {
            try
            {
                logger.LogInformation(
                    "[CleanupOldExceptionLogsAsync] [Action=DeleteRangeAsync()] [Count={Count}] [Start]",
                    countToDelete - totalDeleted
                );

                var exceptionLogsToDelete = await queryable
                    .OrderBy(x => x.CreationTime)
                    .Take(batchSize)
                    .ToListAsync(cancellationToken: cancellationToken);

                if (exceptionLogsToDelete.Count == 0)
                {
                    logger.LogInformation(
                        "[CleanupOldExceptionLogsAsync] [Action=DeleteRangeAsync()] [Count={Count}] [NoMoreLogsToDelete]",
                        totalDeleted
                    );

                    break;
                }

                await exceptionLogRepository.DeleteRangeAsync(exceptionLogsToDelete, permanent: !isArchive, cancellationToken: cancellationToken);
                await exceptionLogRepository.SaveChangesAsync(cancellationToken);

                logger.LogInformation(
                    "[CleanupOldExceptionLogsAsync] [Action=DeleteRangeAsync()] [Count={Count}] [End]",
                    exceptionLogsToDelete.Count
                );

                totalDeleted += exceptionLogsToDelete.Count;

                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation(
                        "[CleanupOldExceptionLogsAsync] [Action=DeleteRangeAsync()] [Cancelled] [TotalDeleted={TotalDeleted}]",
                        totalDeleted
                    );

                    break;
                }

                if (totalDeleted > 0 && totalDeleted % (batchSize * 5) == 0)
                {
                    await Task.Delay(500, cancellationToken);
                }
            }
            catch (Exception e)
            {
                logger.LogError(
                    e,
                    "[CleanupOldExceptionLogsAsync] [Action=DeleteRangeAsync()] [Error] [Exception={Exception}]",
                    e.Message
                );

                break;
            }
        }

        return totalDeleted;
    }
}