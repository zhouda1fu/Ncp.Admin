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
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("资产标识");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100).HasComment("名称");
        builder.Property(x => x.Category).IsRequired().HasMaxLength(50).HasComment("分类");
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50).HasComment("编码");
        builder.Property(x => x.Status).IsRequired().HasComment("状态");
        builder.Property(x => x.PurchaseDate).IsRequired().HasComment("购置日期");
        builder.Property(x => x.Value).HasPrecision(18, 2).HasComment("价值");
        builder.Property(x => x.Remark).HasMaxLength(500).HasComment("备注");
        builder.Property(x => x.CreatorId).IsRequired().HasComment("创建人用户ID");
        builder.Property(x => x.CreatedAt).IsRequired().HasComment("创建时间");
        builder.Property(x => x.UpdateTime).HasComment("更新时间");
        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.Status);
    }
}
