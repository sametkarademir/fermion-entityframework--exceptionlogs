using Fermion.EntityFramework.ExceptionLogs.Domain.Entities;
using Fermion.EntityFramework.ExceptionLogs.Infrastructure.EntityConfigurations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fermion.EntityFramework.ExceptionLogs.Infrastructure.Contexts;

public class ExceptionLogDbContext : DbContext
{
    public DbSet<ExceptionLog> ExceptionLogs { get; set; }

    public ExceptionLogDbContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ExceptionLogConfiguration).Assembly);
    }
}