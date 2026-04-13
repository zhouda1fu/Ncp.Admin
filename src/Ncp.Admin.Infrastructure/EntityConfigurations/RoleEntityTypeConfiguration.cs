using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class RoleEntityTypeConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("role");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseGuidVersion7ValueGenerator().HasComment("角色标识");

        builder.Property(b => b.Name).HasMaxLength(50).IsRequired().HasComment("角色名称");
        builder.Property(b => b.Description).HasMaxLength(200).HasComment("角色描述");
        builder.Property(b => b.DataScope).HasComment("数据权限范围");
        builder.Property(b => b.CreatedAt).HasComment("创建时间");
        builder.Property(b => b.IsActive).HasComment("是否启用");
        builder.Property(b => b.IsDeleted).HasComment("是否软删");

        builder.HasIndex(b => b.Name).IsUnique();

        builder.HasMany(r => r.Permissions).WithOne().HasForeignKey(rp => rp.RoleId);
        builder.Navigation(e => e.Permissions).AutoInclude();

        builder.HasMany(r => r.DataDepts).WithOne().HasForeignKey(rd => rd.RoleId);
        builder.Navigation(e => e.DataDepts).AutoInclude();

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}

internal class RolePermissionEntityTypeConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permission");

        builder.HasKey(t => new { t.RoleId, t.PermissionCode });

        builder.Property(b => b.RoleId).HasComment("角色ID");
        builder.Property(b => b.PermissionCode).HasMaxLength(100).IsRequired().HasComment("权限编码");
        builder.Property(b => b.PermissionName).HasMaxLength(100).HasComment("权限名称");
        builder.Property(b => b.PermissionDescription).HasMaxLength(200).HasComment("权限描述");

        builder.HasOne<Role>()
            .WithMany(r => r.Permissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal class RoleDataDeptEntityTypeConfiguration : IEntityTypeConfiguration<RoleDataDept>
{
    public void Configure(EntityTypeBuilder<RoleDataDept> builder)
    {
        builder.ToTable("role_data_dept");

        builder.HasKey(t => new { t.RoleId, t.DeptId });

        builder.Property(b => b.RoleId).HasComment("角色ID");
        builder.Property(b => b.DeptId).HasComment("部门ID");

        builder.HasOne<Role>()
            .WithMany(r => r.DataDepts)
            .HasForeignKey(rd => rd.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

