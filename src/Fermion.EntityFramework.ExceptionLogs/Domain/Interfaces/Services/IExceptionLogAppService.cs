using Fermion.EntityFramework.ExceptionLogs.Application.DTOs.ExceptionLogs;
using Fermion.EntityFramework.Shared.DTOs.Pagination;

namespace Fermion.EntityFramework.ExceptionLogs.Domain.Interfaces.Services;

public interface IExceptionLogAppService
{
    Task<ExceptionLogResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PageableResponseDto<ExceptionLogResponseDto>> GetPageableAndFilterAsync(GetListExceptionLogRequestDto request, CancellationToken cancellationToken = default);
    Task<int> CleanupOldExceptionLogsAsync(DateTime olderThan, bool isArchive = true, CancellationToken cancellationToken = default);

}