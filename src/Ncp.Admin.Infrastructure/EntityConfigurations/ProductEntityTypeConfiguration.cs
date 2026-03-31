using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class ProductEntityTypeConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("product");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("产品标识");
        builder.Property(x => x.ProductTypeId)
            .HasConversion(
                new ValueConverter<ProductTypeId, Guid>(
                    v => Guid.Parse(v.ToString()!),
                    v => new ProductTypeId(v)))
            .HasComment("产品类型ID");
        builder.Property(x => x.Status).HasComment("状态（是否有效）");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200).HasComment("产品名称");
        builder.Property(x => x.Code).IsRequired().HasMaxLength(100).HasComment("产品编码");
        builder.Property(x => x.Model).IsRequired().HasMaxLength(100).HasComment("型号");
        builder.Property(x => x.Unit).IsRequired().HasMaxLength(20).HasComment("单位");
        builder.Property(x => x.Barcode).IsRequired().HasMaxLength(50).HasComment("条码");
        builder.Property(x => x.ActivationCode).IsRequired().HasMaxLength(200).HasComment("激活码");
        builder.Property(x => x.PriceStandard).IsRequired().HasMaxLength(200).HasComment("价格标准");
        builder.Property(x => x.MarketSales).IsRequired().HasMaxLength(500).HasComment("市场销售");
        builder.Property(x => x.Description).IsRequired().HasMaxLength(4000).HasComment("描述");
        builder.Property(x => x.CostPrice).HasPrecision(18, 4).HasComment("成本价");
        builder.Property(x => x.CustomerPrice).HasPrecision(18, 4).HasComment("客户价");
        builder.Property(x => x.Qty).HasComment("库存数量");
        builder.Property(x => x.Tags).IsRequired().HasMaxLength(200).HasComment("标签");
        builder.Property(x => x.Feature).IsRequired().HasMaxLength(4000).HasComment("功能特点");
        builder.Property(x => x.Configuration).IsRequired().HasMaxLength(4000).HasComment("硬件配置");
        builder.Property(x => x.Instructions).IsRequired().HasMaxLength(4000).HasComment("使用说明");
        builder.Property(x => x.InstallProcess).IsRequired().HasMaxLength(4000).HasComment("操作流程");
        builder.Property(x => x.OperationProcessResources).IsRequired().HasMaxLength(2000).HasComment("操作流程资源JSON");
        builder.Property(x => x.Introduction).IsRequired().HasMaxLength(4000).HasComment("产品介绍");
        builder.Property(x => x.IntroductionResources).IsRequired().HasMaxLength(2000).HasComment("产品介绍资源JSON");
        builder.Property(x => x.ImagePath).IsRequired().HasMaxLength(500).HasComment("图片路径");
        builder.Property(x => x.CategoryId)
            .HasConversion(
                new ValueConverter<ProductCategoryId, Guid?>(
                    v => v == new ProductCategoryId(Guid.Empty) ? null : Guid.Parse(v.ToString()!),
                    v => v == null || v == Guid.Empty ? new ProductCategoryId(Guid.Empty) : new ProductCategoryId(v.Value)))
            .HasComment("产品分类ID");
        builder.Property(x => x.SupplierId)
            .HasConversion(
                new ValueConverter<SupplierId, Guid?>(
                    v => v == new SupplierId(Guid.Empty) ? null : Guid.Parse(v.ToString()!),
                    v => v == null || v == Guid.Empty ? new SupplierId(Guid.Empty) : new SupplierId(v.Value)))
            .HasComment("供应商ID");
        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.Barcode);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.ProductTypeId);
        builder.HasIndex(x => x.SupplierId);
    }
}
