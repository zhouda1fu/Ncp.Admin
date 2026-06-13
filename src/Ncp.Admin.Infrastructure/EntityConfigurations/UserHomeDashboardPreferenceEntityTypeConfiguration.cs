using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.DashboardAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class UserHomeDashboardPreferenceEntityTypeConfiguration : IEntityTypeConfiguration<UserHomeDashboardPreference>
{
    public void Configure(EntityTypeBuilder<UserHomeDashboardPreference> builder)
    {
        builder.ToTable("user_home_dashboard_preference");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasComment("用户标识");
        builder.Property(x => x.CardOrderJson).IsRequired().HasMaxLength(4000).HasComment("首页卡片排序 JSON");
        builder.Property(x => x.UpdatedAt).HasComment("更新时间");
    }
}
