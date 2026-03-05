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
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("车辆标识");
        builder.Property(x => x.PlateNumber).IsRequired().HasMaxLength(20).HasComment("车牌号");
        builder.Property(x => x.Model).IsRequired().HasMaxLength(100).HasComment("型号");
        builder.Property(x => x.Status).IsRequired().HasComment("状态");
        builder.Property(x => x.Remark).HasMaxLength(500).HasComment("备注");
        builder.Property(x => x.CreatedAt).IsRequired().HasComment("创建时间");
        builder.Property(x => x.UpdateTime).HasComment("更新时间");
        builder.HasIndex(x => x.PlateNumber);
        builder.HasIndex(x => x.Status);
    }
}
