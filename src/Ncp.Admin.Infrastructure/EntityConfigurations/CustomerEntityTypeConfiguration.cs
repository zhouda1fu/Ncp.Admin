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
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("客户标识");
        builder.Property(x => x.OwnerId).IsRequired().HasComment("负责人用户ID（公海为 0）");
        builder.Property(x => x.OwnerDeptId).IsRequired().HasComment("负责人部门ID（冗余，无部门为 0）");
        builder.Property(x => x.OwnerDeptName).IsRequired(false).HasMaxLength(100).HasComment("负责人部门名称（冗余）");
        builder.Property(x => x.CustomerSourceId).IsRequired().HasComment("客户来源ID");
        builder.Property(x => x.CustomerSourceName).IsRequired().HasMaxLength(100).HasComment("客户来源名称");
        builder.Property(x => x.IsVoided).IsRequired().HasComment("是否作废");
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(200).HasComment("客户全称");
        builder.Property(x => x.ShortName).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.Status).IsRequired(false);
        builder.Property(x => x.Nature).IsRequired(false);
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
        builder.Property(x => x.EmployeeCount).IsRequired();
        builder.Property(x => x.BusinessLicense).IsRequired(false).HasMaxLength(500);
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

        builder.HasMany(c => c.ContactRecords)
            .WithOne()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.ContactRecords).AutoInclude();

        builder.HasMany(c => c.Industries)
            .WithOne()
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Industries).AutoInclude();

        builder.HasMany(c => c.Shares)
            .WithOne()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal class CustomerShareEntityTypeConfiguration : IEntityTypeConfiguration<CustomerShare>
{
    public void Configure(EntityTypeBuilder<CustomerShare> builder)
    {
        builder.ToTable("customer_share");
        builder.HasKey(x => new { x.CustomerId, x.SharedToUserId });
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.SharedToUserId).IsRequired();
        builder.Property(x => x.SharedByUserId).IsRequired();
        builder.Property(x => x.SharedAt).IsRequired();
        builder.HasIndex(x => x.SharedToUserId);
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

internal class CustomerContactRecordEntityTypeConfiguration : IEntityTypeConfiguration<CustomerContactRecord>
{
    public void Configure(EntityTypeBuilder<CustomerContactRecord> builder)
    {
        builder.ToTable("customer_contact_record");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.RecordAt).IsRequired();
        builder.Property(x => x.RecordType).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Content).IsRequired(false).HasMaxLength(2000);
        builder.Property(x => x.RecorderId).IsRequired().HasComment("记录人用户ID（无则为 0）");
        builder.Property(x => x.RecorderName).IsRequired(false).HasMaxLength(100);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.RecordAt);
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
