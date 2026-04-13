using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerContactRecordAggregate;

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
        builder.Property(x => x.ShortName).IsRequired(false).HasMaxLength(100).HasComment("客户简称");
        builder.Property(x => x.Status).IsRequired(false).HasComment("客户状态");
        builder.Property(x => x.Nature).IsRequired(false).HasComment("公司性质");
        builder.Property(x => x.ProvinceCode).IsRequired(false).HasMaxLength(20).HasComment("省区域码");
        builder.Property(x => x.CityCode).IsRequired(false).HasMaxLength(20).HasComment("市区域码");
        builder.Property(x => x.DistrictCode).IsRequired(false).HasMaxLength(20).HasComment("区/县区域码");
        builder.Property(x => x.ProvinceName).IsRequired(false).HasMaxLength(100).HasComment("省名称");
        builder.Property(x => x.CityName).IsRequired(false).HasMaxLength(100).HasComment("市名称");
        builder.Property(x => x.DistrictName).IsRequired(false).HasMaxLength(100).HasComment("区/县名称");
        builder.Property(x => x.PhoneProvinceCode).IsRequired(false).HasMaxLength(20).HasComment("电话省区域码");
        builder.Property(x => x.PhoneCityCode).IsRequired(false).HasMaxLength(20).HasComment("电话市区域码");
        builder.Property(x => x.PhoneDistrictCode).IsRequired(false).HasMaxLength(20).HasComment("电话区/县区域码");
        builder.Property(x => x.PhoneProvinceName).IsRequired(false).HasMaxLength(100).HasComment("电话省名称");
        builder.Property(x => x.PhoneCityName).IsRequired(false).HasMaxLength(100).HasComment("电话市名称");
        builder.Property(x => x.PhoneDistrictName).IsRequired(false).HasMaxLength(100).HasComment("电话区/县名称");
        builder.Property(x => x.ConsultationContent).IsRequired(false).HasMaxLength(2000).HasComment("咨询内容");
        builder.Property(x => x.ContactQq).IsRequired(false).HasMaxLength(50).HasComment("QQ");
        builder.Property(x => x.ContactWechat).IsRequired(false).HasMaxLength(50).HasComment("微信");
        builder.Property(x => x.CoverRegion).IsRequired(false).HasMaxLength(200).HasComment("覆盖区域");
        builder.Property(x => x.RegisterAddress).IsRequired(false).HasMaxLength(500).HasComment("注册地址");
        builder.Property(x => x.EmployeeCount).IsRequired().HasComment("员工数量");
        builder.Property(x => x.BusinessLicense).IsRequired(false).HasMaxLength(500).HasComment("营业执照（路径或 URL）");
        builder.Property(x => x.MainContactName).IsRequired(false).HasMaxLength(50).HasComment("主联系人姓名");
        builder.Property(x => x.MainContactPhone).IsRequired(false).HasMaxLength(50).HasComment("主联系人电话");
        builder.Property(x => x.WechatStatus).IsRequired(false).HasMaxLength(50).HasComment("微信添加情况");
        builder.Property(x => x.Remark).IsRequired(false).HasMaxLength(1000).HasComment("备注");
        builder.Property(x => x.IsKeyAccount).IsRequired().HasComment("是否重点客户");
        builder.Property(x => x.IsHidden).IsRequired().HasComment("是否隐藏");
        builder.Property(x => x.CombineFlag).IsRequired().HasComment("合并标记");
        builder.Property(x => x.IsInSea).IsRequired().HasComment("是否在公海");
        builder.Property(x => x.ReleasedToSeaAt).HasComment("释放到公海时间");
        builder.Property(x => x.CreatorId).IsRequired().HasComment("创建人用户ID");
        builder.Property(x => x.CreatorName).IsRequired().HasMaxLength(100).HasComment("创建人姓名");
        builder.Property(x => x.OwnerName).IsRequired(false).HasMaxLength(100).HasComment("负责人姓名（冗余）");
        builder.Property(x => x.ClaimedAt).HasComment("领用时间");
        builder.Property(x => x.CreatedAt).IsRequired().HasComment("创建时间");
        builder.Property(x => x.UpdateTime).HasComment("更新时间");
        builder.HasIndex(x => x.FullName);
        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.IsInSea);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasMany(c => c.Contacts)
            .WithOne()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Industries)
            .WithOne()
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

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
        builder.Property(x => x.CustomerId).IsRequired().HasComment("客户ID");
        builder.Property(x => x.SharedToUserId).IsRequired().HasComment("共享给用户ID");
        builder.Property(x => x.SharedByUserId).IsRequired().HasComment("共享人用户ID");
        builder.Property(x => x.SharedAt).IsRequired().HasComment("共享时间");
        builder.HasIndex(x => x.SharedToUserId);
    }
}

internal class CustomerContactEntityTypeConfiguration : IEntityTypeConfiguration<CustomerContact>
{
    public void Configure(EntityTypeBuilder<CustomerContact> builder)
    {
        builder.ToTable("customer_contact");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("联系人标识");
        builder.Property(x => x.CustomerId).IsRequired().HasComment("客户ID");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50).HasComment("联系人姓名");
        builder.Property(x => x.ContactType).IsRequired(false).HasMaxLength(20).HasComment("联系类型");
        builder.Property(x => x.Gender);
        builder.Property(x => x.Birthday);
        builder.Property(x => x.Position).IsRequired(false).HasMaxLength(50).HasComment("职位");
        builder.Property(x => x.Mobile).IsRequired(false).HasMaxLength(30).HasComment("手机");
        builder.Property(x => x.Phone).IsRequired(false).HasMaxLength(30).HasComment("电话");
        builder.Property(x => x.Email).IsRequired(false).HasMaxLength(100).HasComment("邮箱");
        builder.Property(x => x.Qq).IsRequired(false).HasMaxLength(50).HasComment("QQ");
        builder.Property(x => x.Wechat).IsRequired(false).HasMaxLength(50).HasComment("微信");
        builder.Property(x => x.IsWechatAdded).IsRequired().HasComment("微信是否已添加");
        builder.Property(x => x.IsPrimary).IsRequired().HasComment("是否主联系人");
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
        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.RecordAt).IsRequired();
        builder.Property(x => x.RecordType).IsRequired().HasComment("联系类型：1电话 2出差 3微信 4其他");
        builder.Property(x => x.Title).IsRequired(false).HasMaxLength(200);
        builder.Property(x => x.Content).IsRequired(false).HasMaxLength(4000);
        builder.Property(x => x.NextVisitAt);
        builder.Property(x => x.Status).IsRequired().HasComment("0待选择 1有效联系 2无效联系");
        builder.Property(x => x.OwnerId).IsRequired().HasComment("负责人用户ID");
        builder.Property(x => x.OwnerName).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.OwnerDeptId).IsRequired().HasComment("负责人部门ID");
        builder.Property(x => x.OwnerDeptName).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ModifierId).IsRequired();
        builder.Property(x => x.ModifiedAt).IsRequired();
        builder.Property(x => x.Remark).IsRequired(false).HasMaxLength(2000);
        builder.Property(x => x.ReminderIntervalDays).IsRequired().HasComment("提醒间隔天：1,2,3,10,15,20,30,50,80,100");
        builder.Property(x => x.ReminderCount).IsRequired();
        builder.Property(x => x.FilePath).IsRequired(false).HasMaxLength(1000);
        builder.Property(x => x.CustomerAddress).IsRequired(false).HasMaxLength(500);
        builder.Property(x => x.VisitAddress).IsRequired(false).HasMaxLength(500);
        builder.Property(x => x.IsDeleted).IsRequired().HasComment("软删");
        builder.Property(x => x.DeletedAt).IsRequired().HasComment("删除时间");
        builder.Property(x => x.DeleterId).IsRequired().HasComment("删除人");
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.RecordAt);
        builder.HasIndex(x => x.IsDeleted);
        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.Navigation(x => x.Contacts).AutoInclude();
    }
}

internal class CustomerContactRecordContactEntityTypeConfiguration : IEntityTypeConfiguration<CustomerContactRecordContact>
{
    public void Configure(EntityTypeBuilder<CustomerContactRecordContact> builder)
    {
        builder.ToTable("customer_contact_record_contact");
        builder.HasKey(x => new { x.RecordId, x.ContactId });
        builder.Property(x => x.RecordId).IsRequired();
        builder.Property(x => x.ContactId).IsRequired();
        builder.HasIndex(x => x.ContactId);

        builder.HasOne<CustomerContactRecord>()
            .WithMany(r => r.Contacts)
            .HasForeignKey(x => x.RecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal class CustomerIndustryEntityTypeConfiguration : IEntityTypeConfiguration<CustomerIndustry>
{
    public void Configure(EntityTypeBuilder<CustomerIndustry> builder)
    {
        builder.ToTable("customer_industry");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("客户行业关联标识");
        builder.Property(x => x.CustomerId).IsRequired().HasComment("客户ID");
        builder.Property(x => x.IndustryId).IsRequired().HasComment("行业ID");
        builder.HasIndex(x => new { x.CustomerId, x.IndustryId }).IsUnique();
    }
}
