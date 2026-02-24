using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 排班实体 EF Core 映射配置，表名 schedule
/// </summary>
internal class ScheduleEntityTypeConfiguration : IEntityTypeConfiguration<Schedule>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("schedule");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.WorkDate).IsRequired();
        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.EndTime).IsRequired();
        builder.Property(x => x.ShiftName).HasMaxLength(50);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.WorkDate);
        builder.HasIndex(x => new { x.UserId, x.WorkDate }).IsUnique();
    }
}
