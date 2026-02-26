using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class AssetAllocationEntityTypeConfiguration : IEntityTypeConfiguration<AssetAllocation>
{
    public void Configure(EntityTypeBuilder<AssetAllocation> builder)
    {
        builder.ToTable("asset_allocation");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.AssetId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.AllocatedAt).IsRequired();
        builder.Property(x => x.ReturnedAt);
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.HasIndex(x => x.AssetId);
        builder.HasIndex(x => x.UserId);
    }
}
