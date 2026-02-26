using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class VehicleBookingEntityTypeConfiguration : IEntityTypeConfiguration<VehicleBooking>
{
    public void Configure(EntityTypeBuilder<VehicleBooking> builder)
    {
        builder.ToTable("vehicle_booking");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.VehicleId).IsRequired();
        builder.Property(x => x.BookerId).IsRequired();
        builder.Property(x => x.Purpose).IsRequired().HasMaxLength(200);
        builder.Property(x => x.StartAt).IsRequired();
        builder.Property(x => x.EndAt).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.HasIndex(x => x.VehicleId);
        builder.HasIndex(x => x.BookerId);
        builder.HasIndex(x => x.StartAt);
    }
}
