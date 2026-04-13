using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.CustomerSeaRegionAssignmentAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class CustomerSeaRegionAssignmentAuditDetailEntityTypeConfiguration : IEntityTypeConfiguration<CustomerSeaRegionAssignmentAuditDetail>
{
    public void Configure(EntityTypeBuilder<CustomerSeaRegionAssignmentAuditDetail> builder)
    {
        builder.ToTable("customer_sea_region_assignment_audit_detail");

        builder.HasKey(x => new { x.AuditId, x.ChangeType, x.RegionId });

        builder.Property(x => x.AuditId).IsRequired();
        builder.Property(x => x.RegionId).IsRequired();

        builder.Property(x => x.RegionNameSnapshot).IsRequired(false).HasMaxLength(200);

        builder.Property(x => x.ChangeType).IsRequired();

        builder.HasIndex(x => x.RegionId);
    }
}

