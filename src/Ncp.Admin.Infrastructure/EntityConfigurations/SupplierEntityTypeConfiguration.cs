using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class SupplierEntityTypeConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("supplier");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("供应商标识");
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(200).HasComment("供应商全称");
        builder.Property(x => x.ShortName).IsRequired().HasMaxLength(100).HasComment("简称");
        builder.Property(x => x.Contact).IsRequired().HasMaxLength(50).HasComment("联系人");
        builder.Property(x => x.Phone).IsRequired().HasMaxLength(50).HasComment("电话");
        builder.Property(x => x.Email).IsRequired().HasMaxLength(100).HasComment("邮箱");
        builder.Property(x => x.Address).IsRequired().HasMaxLength(500).HasComment("地址");
        builder.Property(x => x.Remark).IsRequired().HasMaxLength(500).HasComment("备注");
        builder.HasIndex(x => x.FullName);
    }
}
