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
        builder.Property(p => p.Id).UseSnowFlakeValueGenerator().HasComment("岗位标识");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("岗位名称");

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("岗位编码");

        builder.Property(p => p.Description)
            .HasMaxLength(500)
            .HasComment("岗位描述");

        builder.Property(p => p.SortOrder)
            .IsRequired()
            .HasComment("排序号");

        builder.Property(p => p.Status)
            .IsRequired()
            .HasComment("状态（0=禁用，1=启用）");

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasComment("创建时间");

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasComment("是否软删");

        builder.Property(p => p.DeletedAt).HasComment("删除时间");

        builder.Property(p => p.UpdateTime).HasComment("更新时间");

        // 索引
        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => p.DeptId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.IsDeleted);

        // 软删除过滤器
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
