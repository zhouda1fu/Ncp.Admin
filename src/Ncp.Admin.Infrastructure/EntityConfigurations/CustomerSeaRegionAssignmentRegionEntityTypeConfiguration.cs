using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.CustomerSeaRegionAssignmentAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class CustomerSeaRegionAssignmentRegionEntityTypeConfiguration : IEntityTypeConfiguration<CustomerSeaRegionAssignmentRegion>
{
    public void Configure(EntityTypeBuilder<CustomerSeaRegionAssignmentRegion> builder)
    {
        builder.ToTable("customer_sea_region_assignment_region");

        builder.HasKey(x => new { x.AssignmentId, x.RegionId });

        builder.Property(x => x.AssignmentId).IsRequired();
        builder.Property(x => x.RegionId).IsRequired();

        builder.HasIndex(x => x.RegionId);
    }
}

