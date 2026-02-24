using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 会议室预订实体 EF Core 映射配置，表名 meeting_booking
/// </summary>
internal class MeetingBookingEntityTypeConfiguration : IEntityTypeConfiguration<MeetingBooking>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MeetingBooking> builder)
    {
        builder.ToTable("meeting_booking");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.MeetingRoomId).IsRequired();
        builder.Property(x => x.BookerId).IsRequired();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.StartAt).IsRequired();
        builder.Property(x => x.EndAt).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.HasIndex(x => x.MeetingRoomId);
        builder.HasIndex(x => x.BookerId);
        builder.HasIndex(x => x.StartAt);
    }
}
