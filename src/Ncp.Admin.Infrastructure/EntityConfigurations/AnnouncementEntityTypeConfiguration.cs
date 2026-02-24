using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 公告实体 EF Core 映射配置，表名 announcement
/// </summary>
internal class AnnouncementEntityTypeConfiguration : IEntityTypeConfiguration<Announcement>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("announcement");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.PublisherId).IsRequired();
        builder.Property(x => x.PublisherName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.PublishAt);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.HasIndex(x => x.PublisherId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.PublishAt);
        builder.HasIndex(x => x.CreatedAt);
    }
}
