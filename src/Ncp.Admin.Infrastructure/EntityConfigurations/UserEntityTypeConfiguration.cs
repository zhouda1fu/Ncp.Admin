using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 用户聚合根及同聚合子实体（部门、岗位、角色、刷新令牌）的 EF 配置，集中于一文件便于维护。
/// </summary>
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
        builder.Property(b => b.CreatorId).HasComment("创建人用户ID");
        builder.Property(b => b.ModifierId).HasComment("修改人用户ID");
        builder.Property(b => b.DeleterId).HasComment("删除人用户ID");
        builder.Property(b => b.LastLoginTime).HasComment("最后登录时间");
        builder.Property(b => b.LastLoginIp).HasMaxLength(64).HasComment("最后登录IP");
        builder.Property(b => b.UpdateTime).HasComment("更新时间");
        builder.Property(b => b.IsDeleted).HasComment("是否已删除");
        builder.Property(b => b.DeletedAt).HasComment("删除时间");

        builder.Property(b => b.IdCardNumber).HasMaxLength(50).IsRequired().HasComment("身份证号");
        builder.Property(b => b.Address).HasMaxLength(500).IsRequired().HasComment("地址");
        builder.Property(b => b.Education).HasMaxLength(50).IsRequired().HasComment("学历");
        builder.Property(b => b.GraduateSchool).HasMaxLength(100).IsRequired().HasComment("毕业院校");
        builder.Property(b => b.AvatarUrl).HasMaxLength(500).IsRequired().HasComment("头像地址");
        builder.Property(b => b.NotOrderMeal).HasComment("是否不订餐");
        builder.Property(b => b.OrderMealSort).HasComment("订餐排序");
        builder.Property(b => b.WechatGuid).HasMaxLength(64).IsRequired().HasComment("唯一码");
        builder.Property(b => b.IsResigned).HasComment("是否离职");
        builder.Property(b => b.ResignedTime).HasComment("离职时间");

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

        builder.HasOne(au => au.Dept)
            .WithOne()
            .HasForeignKey<UserDept>(ud => ud.Id)
            .OnDelete(DeleteBehavior.ClientCascade);
        builder.Navigation(au => au.Dept).AutoInclude();

        builder.HasOne(au => au.Position)
            .WithOne()
            .HasForeignKey<UserPosition>(up => up.Id)
            .OnDelete(DeleteBehavior.ClientCascade);
        builder.Navigation(au => au.Position).AutoInclude();
    }
}

internal class UserDeptEntityTypeConfiguration : IEntityTypeConfiguration<UserDept>
{
    public void Configure(EntityTypeBuilder<UserDept> builder)
    {
        builder.ToTable("user_dept");

        builder.HasKey(ud => ud.Id);
        builder.Property(ud => ud.Id).HasColumnName("UserId");

        builder.Property(ud => ud.DeptId);
        builder.Property(ud => ud.DeptName).HasMaxLength(100);
        builder.Property(ud => ud.IsDeptManager).HasComment("是否为该部门主管");
        builder.Property(ud => ud.AssignedAt)
            .IsRequired();

        builder.HasIndex(ud => ud.Id);
        builder.HasIndex(ud => ud.DeptId);
    }
}

internal class UserPositionEntityTypeConfiguration : IEntityTypeConfiguration<UserPosition>
{
    public void Configure(EntityTypeBuilder<UserPosition> builder)
    {
        builder.ToTable("user_position");

        builder.HasKey(up => up.Id);
        builder.Property(up => up.Id).HasColumnName("UserId");

        builder.Property(up => up.PositionId);
        builder.Property(up => up.PositionName).HasMaxLength(100);
        builder.Property(up => up.AssignedAt).IsRequired();

        builder.HasIndex(up => up.Id);
        builder.HasIndex(up => up.PositionId);
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

internal class UserRefreshTokenEntityTypeConfiguration : IEntityTypeConfiguration<UserRefreshToken>
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
