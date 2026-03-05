using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class ProductEntityTypeConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("product");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("产品标识");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200).HasComment("产品名称");
        builder.Property(x => x.Code).IsRequired().HasMaxLength(100).HasComment("产品编码");
        builder.Property(x => x.Model).IsRequired().HasMaxLength(100).HasComment("型号");
        builder.Property(x => x.Unit).IsRequired().HasMaxLength(20).HasComment("单位");
        builder.HasIndex(x => x.Code);
    }
}
