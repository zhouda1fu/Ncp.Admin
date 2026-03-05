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
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("行业标识");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100).HasComment("名称");
        builder.Property(x => x.ParentId).HasComment("父级行业ID");
        builder.Property(x => x.SortOrder).IsRequired().HasComment("排序");
        builder.Property(x => x.Remark).HasMaxLength(500).HasComment("备注");
        builder.HasIndex(x => x.ParentId);
    }
}
