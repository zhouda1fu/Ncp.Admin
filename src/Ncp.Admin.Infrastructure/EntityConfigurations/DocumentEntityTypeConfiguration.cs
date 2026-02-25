using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 文档实体 EF Core 映射配置，表名 document，含与 document_version 的一对多关系
/// </summary>
internal class DocumentEntityTypeConfiguration : IEntityTypeConfiguration<Document>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("document");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(500);
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.HasIndex(x => x.CreatorId);

        builder.HasMany(x => x.Versions)
            .WithOne()
            .HasForeignKey("DocumentId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Versions).HasField("_versions").AutoInclude();
    }
}

/// <summary>
/// 文档版本实体 EF Core 映射配置，表名 document_version
/// </summary>
internal class DocumentVersionEntityTypeConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("document_version");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property<DocumentId>("DocumentId").IsRequired();
        builder.Property(x => x.VersionNumber).IsRequired();
        builder.Property(x => x.FileStorageKey).IsRequired().HasMaxLength(500);
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(500);
        builder.Property(x => x.FileSize).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex("DocumentId");
    }
}
