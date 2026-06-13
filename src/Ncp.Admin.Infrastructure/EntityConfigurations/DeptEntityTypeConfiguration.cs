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

        builder.Property(d => d.Status)
            .IsRequired()
            .HasComment("状态（0=禁用，1=启用）");

        builder.Property(d => d.SortOrder)
            .IsRequired()
            .HasComment("排序号");

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
        builder.HasIndex(d => d.SortOrder);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.IsDeleted);

        builder.HasMany(d => d.ResponsibleUsers)
            .WithOne()
            .HasForeignKey(x => x.DeptId)
            .OnDelete(DeleteBehavior.Cascade);

        // 软删除过滤器
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}

/// <summary>
/// 部门负责人关系实体类型配置。
/// </summary>
internal class DeptResponsibleUserEntityTypeConfiguration : IEntityTypeConfiguration<DeptResponsibleUser>
{
    public void Configure(EntityTypeBuilder<DeptResponsibleUser> builder)
    {
        builder.ToTable("dept_responsible_user");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseSnowFlakeValueGenerator().HasComment("部门负责人关系标识");

        builder.Property(x => x.DeptId).IsRequired().HasComment("部门ID");
        builder.Property(x => x.UserId).IsRequired().HasComment("负责人用户ID");
        builder.Property(x => x.IsDefault).IsRequired().HasComment("是否默认负责人");
        builder.Property(x => x.SortOrder).IsRequired().HasComment("排序号");
        builder.Property(x => x.CreatedAt).IsRequired().HasComment("创建时间");

        builder.HasIndex(x => x.DeptId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.DeptId, x.UserId }).IsUnique();
    }
}
