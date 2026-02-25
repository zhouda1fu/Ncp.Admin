using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ContactAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 联系人实体 EF Core 映射配置，表名 contact
/// </summary>
internal class ContactEntityTypeConfiguration : IEntityTypeConfiguration<Contact>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("contact");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Company).HasMaxLength(200);
        builder.Property(x => x.GroupId);
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.HasIndex(x => x.CreatorId);
        builder.HasIndex(x => x.GroupId);
    }
}
