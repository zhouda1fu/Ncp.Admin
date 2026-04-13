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
        builder.Property(x => x.Id)
            .UseGuidVersion7ValueGenerator()
            .HasComment("公告标识");
        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("公告标题");
        builder.Property(x => x.Content)
            .IsRequired()
            .HasComment("公告正文");
        builder.Property(x => x.PublisherId)
            .IsRequired()
            .HasComment("发布人用户ID");
        builder.Property(x => x.PublisherName)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("发布人姓名");
        builder.Property(x => x.Status)
            .IsRequired()
            .HasComment("公告状态");
        builder.Property(x => x.PublishAt)
            .HasComment("发布时间（草稿为 null）");
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasComment("创建时间");
        builder.Property(x => x.UpdateTime)
            .HasComment("更新时间");
        builder.HasIndex(x => x.PublisherId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.PublishAt);
        builder.HasIndex(x => x.CreatedAt);
    }
}
