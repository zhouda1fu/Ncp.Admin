using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.LeaveBalanceAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 请假余额实体类型配置
/// </summary>
internal class LeaveBalanceEntityTypeConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("leave_balance");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).UseGuidVersion7ValueGenerator();

        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.Year).IsRequired();
        builder.Property(b => b.LeaveType).IsRequired();
        builder.Property(b => b.TotalDays).IsRequired();
        builder.Property(b => b.UsedDays).IsRequired();
        builder.Ignore(b => b.RemainingDays);
        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdateTime);

        builder.HasIndex(b => new { b.UserId, b.Year, b.LeaveType }).IsUnique();
    }
}
