using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ShareLinkAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 共享链接实体 EF Core 映射配置，表名 share_link
/// </summary>
internal class ShareLinkEntityTypeConfiguration : IEntityTypeConfiguration<ShareLink>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ShareLink> builder)
    {
        builder.ToTable("share_link");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.DocumentId).IsRequired();
        builder.Property(x => x.Token).IsRequired().HasMaxLength(64);
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.ExpiresAt);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.DocumentId);
        builder.HasIndex(x => x.Token).IsUnique();
    }
}
