using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 会议室实体 EF Core 映射配置，表名 meeting_room
/// </summary>
internal class MeetingRoomEntityTypeConfiguration : IEntityTypeConfiguration<MeetingRoom>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MeetingRoom> builder)
    {
        builder.ToTable("meeting_room");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Location).HasMaxLength(200);
        builder.Property(x => x.Capacity).IsRequired();
        builder.Property(x => x.Equipment).HasMaxLength(500);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.HasIndex(x => x.Status);
    }
}
