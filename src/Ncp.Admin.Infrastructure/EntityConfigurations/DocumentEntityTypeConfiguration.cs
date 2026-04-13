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
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("文档标识");
        builder.Property(x => x.Title).IsRequired().HasMaxLength(500).HasComment("标题");
        builder.Property(x => x.CreatorId).IsRequired().HasComment("创建人用户ID");
        builder.Property(x => x.CreatedAt).IsRequired().HasComment("创建时间");
        builder.Property(x => x.UpdateTime).HasComment("更新时间");
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
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("文档版本标识");
        builder.Property<DocumentId>("DocumentId").IsRequired().HasComment("文档ID");
        builder.Property(x => x.VersionNumber).IsRequired().HasComment("版本号（从 1 递增）");
        builder.Property(x => x.FileStorageKey).IsRequired().HasMaxLength(500).HasComment("文件存储 Key");
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(500).HasComment("原始文件名");
        builder.Property(x => x.FileSize).IsRequired().HasComment("文件大小（字节）");
        builder.Property(x => x.CreatedAt).IsRequired().HasComment("创建时间");
        builder.HasIndex("DocumentId");
    }
}
