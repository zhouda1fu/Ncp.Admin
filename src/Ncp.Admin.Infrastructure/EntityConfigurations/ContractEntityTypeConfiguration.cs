using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 合同实体 EF Core 映射配置，表名 contract
/// </summary>
internal class ContractEntityTypeConfiguration : IEntityTypeConfiguration<Contract>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("contract");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.PartyA).IsRequired().HasMaxLength(200);
        builder.Property(x => x.PartyB).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.FileStorageKey).HasMaxLength(500);
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.EndDate);
    }
}
