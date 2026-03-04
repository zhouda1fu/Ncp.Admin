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
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Model).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Unit).IsRequired().HasMaxLength(20);
        builder.HasIndex(x => x.Code);
    }
}
