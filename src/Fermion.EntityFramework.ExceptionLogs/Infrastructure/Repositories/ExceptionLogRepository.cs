using Fermion.EntityFramework.ExceptionLogs.Domain.Entities;
using Fermion.EntityFramework.ExceptionLogs.Domain.Interfaces.Repositories;
using Fermion.EntityFramework.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fermion.EntityFramework.ExceptionLogs.Infrastructure.Repositories;

public class ExceptionLogRepository<TContext> : EfRepositoryBase<ExceptionLog, Guid, TContext>, IExceptionLogRepository where TContext : DbContext
{
    public ExceptionLogRepository(TContext context) : base(context)
    {
    }
}