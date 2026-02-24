using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 公告已读记录实体 EF Core 映射配置，表名 announcement_read_record
/// </summary>
internal class AnnouncementReadRecordEntityTypeConfiguration : IEntityTypeConfiguration<AnnouncementReadRecord>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AnnouncementReadRecord> builder)
    {
        builder.ToTable("announcement_read_record");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.AnnouncementId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.ReadAt).IsRequired();
        builder.HasIndex(x => new { x.AnnouncementId, x.UserId }).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}
