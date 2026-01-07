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
        builder.Property(t => t.Id).UseSnowFlakeValueGenerator();

        builder.Property(b => b.Name).HasMaxLength(50).IsRequired();
        builder.Property(b => b.Email).HasMaxLength(100).IsRequired();
        builder.Property(b => b.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(b => b.Phone).HasMaxLength(20);
        builder.Property(b => b.RealName).HasMaxLength(50);
        builder.Property(b => b.Gender).HasMaxLength(10);
        builder.Property(b => b.Age);
        builder.Property(b => b.BirthDate);
        builder.Property(b => b.IsActive);
        builder.Property(b => b.CreatedAt);
        builder.Property(b => b.LastLoginTime);
        builder.Property(b => b.UpdateTime);

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

        // 配置 User 与 UserOrganizationUnit 的一对一关系
        builder.HasOne(au => au.OrganizationUnit)
            .WithOne()
            .HasForeignKey<UserOrganizationUnit>(uou => uou.UserId)
            .OnDelete(DeleteBehavior.ClientCascade);
        builder.Navigation(au => au.OrganizationUnit).AutoInclude();
    }
}

internal class UserOrganizationUnitEntityTypeConfiguration : IEntityTypeConfiguration<UserOrganizationUnit>
{
    public void Configure(EntityTypeBuilder<UserOrganizationUnit> builder)
    {
        builder.ToTable("user_organization_unit");

        builder.HasKey(uo => uo.UserId);

        builder.Property(uo => uo.UserId);
        builder.Property(uo => uo.OrganizationUnitId);
        builder.Property(uo => uo.OrganizationUnitName).HasMaxLength(100);
        builder.Property(uo => uo.AssignedAt)
            .IsRequired();

        // 索引
        builder.HasIndex(uo => uo.UserId);
        builder.HasIndex(uo => uo.OrganizationUnitId);
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

