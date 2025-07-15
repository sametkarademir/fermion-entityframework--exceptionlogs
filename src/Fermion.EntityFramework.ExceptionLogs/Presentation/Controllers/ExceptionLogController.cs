using Fermion.EntityFramework.ExceptionLogs.Application.DTOs.ExceptionLogs;
using Fermion.EntityFramework.ExceptionLogs.Domain.Interfaces.Services;
using Fermion.EntityFramework.Shared.DTOs.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fermion.EntityFramework.ExceptionLogs.Presentation.Controllers;

[ApiController]
[Route("api/exception-logs")]
public class ExceptionLogController(
    IExceptionLogAppService exceptionLogAppService)
    : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExceptionLogResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await exceptionLogAppService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("pageable")]
    [ProducesResponseType(typeof(PageableResponseDto<ExceptionLogResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetPageableAndFilterAsync([FromQuery] GetListExceptionLogRequestDto request, CancellationToken cancellationToken = default)
    {
        var result = await exceptionLogAppService.GetPageableAndFilterAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("cleanup")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CleanupOldExceptionLogsAsync([FromQuery] DateTime olderThan, [FromQuery] bool isArchive = true, CancellationToken cancellationToken = default)
    {
        var result = await exceptionLogAppService.CleanupOldExceptionLogsAsync(olderThan, isArchive, cancellationToken);
        return Ok(result);
    }
}