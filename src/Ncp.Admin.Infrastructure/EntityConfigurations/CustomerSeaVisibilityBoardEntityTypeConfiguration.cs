using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSeaVisibilityAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class CustomerSeaVisibilityBoardEntityTypeConfiguration : IEntityTypeConfiguration<CustomerSeaVisibilityBoard>
{
    public void Configure(EntityTypeBuilder<CustomerSeaVisibilityBoard> builder)
    {
        builder.ToTable("customer_sea_visibility_board");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .HasComment("客户 ID（与客户主键一致）");

        builder.HasOne<Customer>()
            .WithOne()
            .HasForeignKey<CustomerSeaVisibilityBoard>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Entries).AutoInclude();

        builder.HasMany(x => x.Entries)
            .WithOne()
            .HasForeignKey(x => x.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
