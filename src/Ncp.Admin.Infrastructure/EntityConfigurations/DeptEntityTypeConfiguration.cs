using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 部门实体类型配置
/// </summary>
internal class DeptEntityTypeConfiguration : IEntityTypeConfiguration<Dept>
{
    public void Configure(EntityTypeBuilder<Dept> builder)
    {
        builder.ToTable("dept");

        builder.HasKey(d => d.Id);
        builder.Property(t => t.Id).UseSnowFlakeValueGenerator().HasComment("部门标识");

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("部门名称");

        builder.Property(d => d.Remark)
            .HasMaxLength(500)
            .HasComment("备注");

        builder.Property(d => d.ManagerId)
            .IsRequired()
            .HasComment("部门主管用户ID");

        builder.Property(d => d.Status)
            .IsRequired()
            .HasComment("状态（0=禁用，1=启用）");

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasComment("创建时间");

        builder.Property(d => d.IsDeleted)
            .IsRequired()
            .HasComment("是否软删");

        builder.Property(d => d.DeletedAt).HasComment("删除时间");

        builder.Property(d => d.UpdateTime).HasComment("更新时间");

        // 索引
        builder.HasIndex(d => d.ParentId);
        builder.HasIndex(d => d.ManagerId);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.IsDeleted);

        // 软删除过滤器
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
