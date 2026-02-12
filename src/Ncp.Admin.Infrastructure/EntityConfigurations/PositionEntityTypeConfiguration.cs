using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 岗位实体类型配置
/// </summary>
internal class PositionEntityTypeConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("position");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).UseSnowFlakeValueGenerator();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.SortOrder)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.IsDeleted)
            .IsRequired();

        builder.Property(p => p.DeletedAt);

        builder.Property(p => p.UpdateTime);

        // 索引
        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => p.DeptId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.IsDeleted);

        // 软删除过滤器
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
