using Fermion.EntityFramework.ExceptionLogs.Domain.Entities;
using Fermion.EntityFramework.Shared.Interfaces;

namespace Fermion.EntityFramework.ExceptionLogs.Domain.Interfaces.Repositories;

public interface IExceptionLogRepository : IRepository<ExceptionLog, Guid>
{

}