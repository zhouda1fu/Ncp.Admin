using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class VehicleEntityTypeConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicle");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.PlateNumber).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Model).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Remark).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.HasIndex(x => x.PlateNumber);
        builder.HasIndex(x => x.Status);
    }
}
