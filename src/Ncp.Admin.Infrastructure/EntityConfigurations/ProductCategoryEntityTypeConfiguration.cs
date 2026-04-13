using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class ProductCategoryEntityTypeConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("product_category");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("产品分类标识");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100).HasComment("分类名称");
        builder.Property(x => x.Remark).IsRequired().HasMaxLength(500).HasComment("备注");
        builder.Property(x => x.ParentId)
            .IsRequired()
            .HasComment("上级分类ID（Guid.Empty 表示根节点）");
        builder.Property(x => x.SortOrder).HasComment("排序");
        builder.Property(x => x.Visible).HasComment("是否可见");
        builder.Property(x => x.IsDiscount).IsRequired().HasComment("是否优惠");
        builder.HasIndex(x => x.ParentId);
        builder.HasIndex(x => x.SortOrder);
    }
}
