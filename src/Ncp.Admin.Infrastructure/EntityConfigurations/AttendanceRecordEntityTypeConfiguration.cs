using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 考勤记录实体 EF Core 映射配置，表名 attendance_record
/// </summary>
internal class AttendanceRecordEntityTypeConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("attendance_record");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.CheckInAt).IsRequired();
        builder.Property(x => x.CheckOutAt);
        builder.Property(x => x.Source).IsRequired();
        builder.Property(x => x.Location).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.CheckInAt);
        builder.HasIndex(x => new { x.UserId, x.CheckInAt });
    }
}
