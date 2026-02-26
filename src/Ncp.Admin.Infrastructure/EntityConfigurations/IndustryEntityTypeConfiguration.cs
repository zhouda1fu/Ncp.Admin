using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class IndustryEntityTypeConfiguration : IEntityTypeConfiguration<Industry>
{
    public void Configure(EntityTypeBuilder<Industry> builder)
    {
        builder.ToTable("industry");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ParentId);
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.Remark).HasMaxLength(500);
        builder.HasIndex(x => x.ParentId);
    }
}
