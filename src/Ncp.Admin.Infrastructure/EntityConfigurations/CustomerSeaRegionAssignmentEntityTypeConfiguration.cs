using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.CustomerSeaRegionAssignmentAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class CustomerSeaRegionAssignmentEntityTypeConfiguration : IEntityTypeConfiguration<CustomerSeaRegionAssignment>
{
    public void Configure(EntityTypeBuilder<CustomerSeaRegionAssignment> builder)
    {
        builder.ToTable("customer_sea_region_assignment");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("客户公海片区分配ID");
        builder.Property(x => x.TargetUserId).IsRequired().HasComment("目标用户ID");
        builder.Property(x => x.CreatedAt).IsRequired().HasComment("创建时间（UTC）");
        builder.Property(x => x.ModifiedAt).IsRequired().HasComment("更新时间（UTC）");

        // 每个用户仅维护一份绑定集合
        builder.HasIndex(x => x.TargetUserId).IsUnique();

        builder.Navigation(x => x.Regions).AutoInclude();

        builder.HasMany(x => x.Regions)
            .WithOne()
            .HasForeignKey(x => x.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

