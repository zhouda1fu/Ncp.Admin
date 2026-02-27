using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class CustomerEntityTypeConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customer");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.OwnerId);
        builder.Property(x => x.CustomerSourceId).IsRequired();
        builder.Property(x => x.CustomerSourceName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.IsVoided).IsRequired();
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ShortName).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.Nature).IsRequired(false).HasMaxLength(50);
        builder.Property(x => x.ProvinceCode).IsRequired(false).HasMaxLength(20);
        builder.Property(x => x.CityCode).IsRequired(false).HasMaxLength(20);
        builder.Property(x => x.DistrictCode).IsRequired(false).HasMaxLength(20);
        builder.Property(x => x.ProvinceName).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.CityName).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.DistrictName).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.PhoneProvinceCode).IsRequired(false).HasMaxLength(20);
        builder.Property(x => x.PhoneCityCode).IsRequired(false).HasMaxLength(20);
        builder.Property(x => x.PhoneDistrictCode).IsRequired(false).HasMaxLength(20);
        builder.Property(x => x.PhoneProvinceName).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.PhoneCityName).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.PhoneDistrictName).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.ConsultationContent).IsRequired(false).HasMaxLength(2000);
        builder.Property(x => x.ContactQq).IsRequired(false).HasMaxLength(50);
        builder.Property(x => x.ContactWechat).IsRequired(false).HasMaxLength(50);
        builder.Property(x => x.CoverRegion).IsRequired(false).HasMaxLength(200);
        builder.Property(x => x.RegisterAddress).IsRequired(false).HasMaxLength(500);
        builder.Property(x => x.MainContactName).IsRequired(false).HasMaxLength(50);
        builder.Property(x => x.MainContactPhone).IsRequired(false).HasMaxLength(50);
        builder.Property(x => x.WechatStatus).IsRequired(false).HasMaxLength(50);
        builder.Property(x => x.Remark).IsRequired(false).HasMaxLength(1000);
        builder.Property(x => x.IsKeyAccount).IsRequired();
        builder.Property(x => x.IsHidden).IsRequired();
        builder.Property(x => x.CombineFlag).IsRequired();
        builder.Property(x => x.IsInSea).IsRequired();
        builder.Property(x => x.ReleasedToSeaAt);
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.CreatorName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.OwnerName).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.ClaimedAt);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.HasIndex(x => x.FullName);
        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.IsInSea);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasMany(c => c.Contacts)
            .WithOne()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Contacts).AutoInclude();

        builder.HasMany(c => c.Industries)
            .WithOne()
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Industries).AutoInclude();
    }
}

internal class CustomerContactEntityTypeConfiguration : IEntityTypeConfiguration<CustomerContact>
{
    public void Configure(EntityTypeBuilder<CustomerContact> builder)
    {
        builder.ToTable("customer_contact");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ContactType).IsRequired(false).HasMaxLength(20);
        builder.Property(x => x.Gender);
        builder.Property(x => x.Birthday);
        builder.Property(x => x.Position).IsRequired(false).HasMaxLength(50);
        builder.Property(x => x.Mobile).IsRequired(false).HasMaxLength(30);
        builder.Property(x => x.Phone).IsRequired(false).HasMaxLength(30);
        builder.Property(x => x.Email).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.IsPrimary).IsRequired();
    }
}

internal class CustomerIndustryEntityTypeConfiguration : IEntityTypeConfiguration<CustomerIndustry>
{
    public void Configure(EntityTypeBuilder<CustomerIndustry> builder)
    {
        builder.ToTable("customer_industry");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.IndustryId).IsRequired();
        builder.HasIndex(x => new { x.CustomerId, x.IndustryId }).IsUnique();
    }
}
