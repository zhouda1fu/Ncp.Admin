using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class ProductParameterEntityTypeConfiguration : IEntityTypeConfiguration<ProductParameter>
{
    public void Configure(EntityTypeBuilder<ProductParameter> builder)
    {
        builder.ToTable("product_parameter");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("产品参数标识");
        builder.Property(x => x.ProductId).HasComment("所属产品ID");
        builder.Property(x => x.Year).IsRequired().HasMaxLength(20).HasComment("参数年份");
        builder.Property(x => x.Description).IsRequired().HasMaxLength(4000).HasComment("参数内容");
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => new { x.ProductId, x.Year }).IsUnique();
    }
}
