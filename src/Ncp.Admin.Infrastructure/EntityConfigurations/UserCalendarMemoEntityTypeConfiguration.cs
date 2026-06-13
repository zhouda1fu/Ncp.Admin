using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.DashboardAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class UserCalendarMemoEntityTypeConfiguration : IEntityTypeConfiguration<UserCalendarMemo>
{
    public void Configure(EntityTypeBuilder<UserCalendarMemo> builder)
    {
        builder.ToTable("user_calendar_memo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseSnowFlakeValueGenerator().HasComment("便签标识");
        builder.Property(x => x.UserId).HasComment("用户标识");
        builder.Property(x => x.MemoDate).HasComment("便签日期");
        builder.Property(x => x.Content).IsRequired().HasMaxLength(4000).HasComment("便签内容");
        builder.Property(x => x.CreatedAt).HasComment("创建时间");
        builder.Property(x => x.UpdatedAt).HasComment("更新时间");
        builder.HasIndex(x => new { x.UserId, x.MemoDate }).IsUnique();
    }
}
