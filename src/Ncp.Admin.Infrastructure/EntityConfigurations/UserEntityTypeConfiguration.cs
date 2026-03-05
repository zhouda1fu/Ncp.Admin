using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseSnowFlakeValueGenerator().HasComment("用户标识");

        builder.Property(b => b.Name).HasMaxLength(50).IsRequired().HasComment("用户名");
        builder.Property(b => b.Email).HasMaxLength(100).IsRequired().HasComment("邮箱");
        builder.Property(b => b.PasswordHash).HasMaxLength(255).IsRequired().HasComment("密码哈希");
        builder.Property(b => b.Phone).HasMaxLength(20).HasComment("手机号");
        builder.Property(b => b.RealName).HasMaxLength(50).HasComment("真实姓名");
        builder.Property(b => b.Gender).HasMaxLength(10).HasComment("性别");
        builder.Property(b => b.Age).HasComment("年龄");
        builder.Property(b => b.BirthDate).HasComment("出生日期");
        builder.Property(b => b.IsActive).HasComment("是否启用");
        builder.Property(b => b.CreatedAt).HasComment("创建时间");
        builder.Property(b => b.LastLoginTime).HasComment("最后登录时间");
        builder.Property(b => b.UpdateTime).HasComment("更新时间");

        builder.HasIndex(b => b.Name);
        builder.HasIndex(b => b.Email);

        builder.HasMany(au => au.Roles)
            .WithOne()
            .HasForeignKey(aur => aur.UserId)
            .OnDelete(DeleteBehavior.ClientCascade);
        builder.Navigation(au => au.Roles).AutoInclude();

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        // 配置 User 与 UserDept 的一对一关系
        builder.HasOne(au => au.Dept)
            .WithOne()
            .HasForeignKey<UserDept>(ud => ud.UserId)
            .OnDelete(DeleteBehavior.ClientCascade);
        builder.Navigation(au => au.Dept).AutoInclude();
    }
}

internal class UserDeptEntityTypeConfiguration : IEntityTypeConfiguration<UserDept>
{
    public void Configure(EntityTypeBuilder<UserDept> builder)
    {
        builder.ToTable("user_dept");

        builder.HasKey(ud => ud.UserId);

        builder.Property(ud => ud.UserId);
        builder.Property(ud => ud.DeptId);
        builder.Property(ud => ud.DeptName).HasMaxLength(100);
        builder.Property(ud => ud.AssignedAt)
            .IsRequired();

        // 索引
        builder.HasIndex(ud => ud.UserId);
        builder.HasIndex(ud => ud.DeptId);
    }
}

internal class UserRoleEntityTypeConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_role");

        builder.HasKey(t => new { t.UserId, t.RoleId });

        builder.Property(b => b.UserId);
        builder.Property(b => b.RoleId);
        builder.Property(b => b.RoleName).HasMaxLength(50).IsRequired();

        builder.HasOne<User>()
            .WithMany(u => u.Roles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        builder.ToTable("user_refresh_token");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseSnowFlakeValueGenerator();
        builder.Property(x => x.Token).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CreatedTime).IsRequired();
        builder.Property(x => x.ExpiresTime).IsRequired();
    }
}

