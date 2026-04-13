using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.CustomerSeaRegionAssignmentAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class CustomerSeaRegionAssignmentAuditEntityTypeConfiguration : IEntityTypeConfiguration<CustomerSeaRegionAssignmentAudit>
{
    public void Configure(EntityTypeBuilder<CustomerSeaRegionAssignmentAudit> builder)
    {
        builder.ToTable("customer_sea_region_assignment_audit");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("公海片区分配审计ID");
        builder.Property(x => x.TargetUserId).IsRequired().HasComment("被修改用户ID");
        builder.Property(x => x.OperatorUserId).IsRequired().HasComment("操作人用户ID");
        builder.Property(x => x.OperatorUserName).IsRequired().HasMaxLength(100).HasComment("操作人姓名");
        builder.Property(x => x.CreatedAt).IsRequired().HasComment("操作时间（UTC）");

        builder.HasIndex(x => x.TargetUserId);
        builder.HasIndex(x => x.CreatedAt);

        builder.Navigation(x => x.Details).AutoInclude();

        builder.HasMany(x => x.Details)
            .WithOne()
            .HasForeignKey(x => x.AuditId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

