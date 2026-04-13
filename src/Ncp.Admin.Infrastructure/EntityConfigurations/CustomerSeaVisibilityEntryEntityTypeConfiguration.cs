using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.CustomerSeaVisibilityAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class CustomerSeaVisibilityEntryEntityTypeConfiguration : IEntityTypeConfiguration<CustomerSeaVisibilityEntry>
{
    public void Configure(EntityTypeBuilder<CustomerSeaVisibilityEntry> builder)
    {
        builder.ToTable("customer_sea_visibility_entry");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .UseGuidVersion7ValueGenerator()
            .HasComment("可见性条目 ID");

        builder.Property(x => x.BoardId).IsRequired().HasComment("客户 ID");
        builder.Property(x => x.UserId).IsRequired().HasComment("被授权用户 ID");
        builder.Property(x => x.GrantedAt).IsRequired().HasComment("授权时间（UTC）");
        builder.Property(x => x.RevokedAt).HasComment("撤回时间（UTC），空表示仍生效");

        builder.HasIndex(x => new { x.BoardId, x.UserId }).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}
