using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class AssetEntityTypeConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("asset");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Category).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.PurchaseDate).IsRequired();
        builder.Property(x => x.Value).HasPrecision(18, 2);
        builder.Property(x => x.Remark).HasMaxLength(500);
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.Status);
    }
}
