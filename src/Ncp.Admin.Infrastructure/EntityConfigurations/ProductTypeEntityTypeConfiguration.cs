using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class ProductTypeEntityTypeConfiguration : IEntityTypeConfiguration<ProductType>
{
    public void Configure(EntityTypeBuilder<ProductType> builder)
    {
        builder.ToTable("product_type");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("产品类型标识");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100).HasComment("类型名称");
        builder.Property(x => x.SortOrder).HasComment("排序");
        builder.Property(x => x.Visible).HasComment("是否启用");
        builder.HasIndex(x => x.SortOrder);
    }
}
