using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 订单分类实体 EF Core 映射配置，表名 order_category
/// </summary>
internal class OrderCategoryEntityTypeConfiguration : IEntityTypeConfiguration<OrderCategory>
{
    public void Configure(EntityTypeBuilder<OrderCategory> builder)
    {
        builder.ToTable("order_category");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("订单分类标识");
        builder.Property(x => x.OrderId).IsRequired().HasComment("订单ID");
        builder.Property(x => x.ProductCategoryId).IsRequired().HasComment("产品分类ID");
        builder.Property(x => x.CategoryName).IsRequired().HasMaxLength(100).HasComment("产品分类名称");
        builder.Property(x => x.DiscountPoints).IsRequired().HasPrecision(18, 4).HasComment("优惠点数");
        builder.Property(x => x.Remark).IsRequired().HasMaxLength(500).HasComment("备注");

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.ProductCategoryId);
        builder.HasIndex(x => new { x.OrderId, x.ProductCategoryId });
    }
}
