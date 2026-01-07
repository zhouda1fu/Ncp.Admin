using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 组织架构实体类型配置
/// </summary>
internal class OrganizationUnitEntityTypeConfiguration : IEntityTypeConfiguration<OrganizationUnit>
{
    public void Configure(EntityTypeBuilder<OrganizationUnit> builder)
    {
        builder.ToTable("organization_unit");

        builder.HasKey(o => o.Id);
        builder.Property(t => t.Id).UseSnowFlakeValueGenerator();

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Description)
            .HasMaxLength(500);

        builder.Property(o => o.SortOrder)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.IsActive)
            .IsRequired();

        builder.Property(o => o.IsDeleted)
            .IsRequired();

        builder.Property(o => o.DeletedAt);

        builder.Property(o => o.UpdateTime);

        // 索引
        builder.HasIndex(o => o.ParentId);
        builder.HasIndex(o => o.SortOrder);
        builder.HasIndex(o => o.IsActive);
        builder.HasIndex(o => o.IsDeleted);

        // 软删除过滤器
        builder.HasQueryFilter(au => !au.IsDeleted);
    }
}

