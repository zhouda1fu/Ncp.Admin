using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 联系组实体 EF Core 映射配置，表名 contact_group
/// </summary>
internal class ContactGroupEntityTypeConfiguration : IEntityTypeConfiguration<ContactGroup>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ContactGroup> builder)
    {
        builder.ToTable("contact_group");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.HasIndex(x => x.CreatorId);
    }
}
