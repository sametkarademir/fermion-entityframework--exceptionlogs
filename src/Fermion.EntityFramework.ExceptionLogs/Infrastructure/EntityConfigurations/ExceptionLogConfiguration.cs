using Fermion.EntityFramework.ExceptionLogs.Domain.Entities;
using Fermion.EntityFramework.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fermion.EntityFramework.ExceptionLogs.Infrastructure.EntityConfigurations;

public class ExceptionLogConfiguration : IEntityTypeConfiguration<ExceptionLog>
{
    public void Configure(EntityTypeBuilder<ExceptionLog> builder)
    {
        builder.ApplyGlobalEntityConfigurations();

        builder.ToTable("ExceptionLogs");
        builder.HasIndex(item => item.Fingerprint);
        builder.HasIndex(item => item.Timestamp);
        builder.HasIndex(item => item.ExceptionType);
        builder.HasIndex(item => item.Code);
        builder.HasIndex(item => item.StatusCode);

        builder.Property(item => item.Fingerprint).HasMaxLength(100).IsRequired();
        builder.Property(item => item.Timestamp).IsRequired();
        builder.Property(item => item.ExceptionType).HasMaxLength(500).IsRequired(false);
        builder.Property(item => item.Message).IsRequired(false);
        builder.Property(item => item.Source).IsRequired(false);
        builder.Property(item => item.StackTrace).IsRequired(false);
        builder.Property(item => item.InnerExceptions).IsRequired(false);
        builder.Property(item => item.ExceptionData).IsRequired(false);

        builder.Property(item => item.Code).HasMaxLength(500).IsRequired(false);
        builder.Property(item => item.Details).HasMaxLength(2000).IsRequired(false);
        builder.Property(item => item.StatusCode).IsRequired();
    }
}