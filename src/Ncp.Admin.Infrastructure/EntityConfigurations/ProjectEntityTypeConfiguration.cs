using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 项目实体 EF Core 映射配置，表名 project
/// </summary>
internal class ProjectEntityTypeConfiguration : IEntityTypeConfiguration<Project>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("project");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.CreatorName).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.CustomerName).IsRequired(false).HasMaxLength(200);
        builder.Property(x => x.ProjectTypeId).IsRequired();
        builder.Property(x => x.ProjectTypeName).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.ProjectStatusOptionId).IsRequired();
        builder.Property(x => x.ProjectStatusOptionName).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.ProjectNumber).HasMaxLength(50);
        builder.Property(x => x.ProjectIndustryId).IsRequired();
        builder.Property(x => x.ProjectIndustryName).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.ProvinceRegionId).IsRequired();
        builder.Property(x => x.ProvinceName).IsRequired(false).HasMaxLength(50);
        builder.Property(x => x.CityRegionId).IsRequired();
        builder.Property(x => x.CityName).IsRequired(false).HasMaxLength(50);
        builder.Property(x => x.DistrictRegionId).IsRequired();
        builder.Property(x => x.DistrictName).IsRequired(false).HasMaxLength(50);
        builder.Property(x => x.StartDate);
        builder.Property(x => x.ProjectEstimate).HasMaxLength(200);
        builder.Property(x => x.PurchaseAmount).HasPrecision(18, 2);
        builder.Property(x => x.ProjectContent).HasMaxLength(4000);
        builder.HasIndex(x => x.CreatorId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.ProjectIndustryId);

        builder.HasMany(p => p.Contacts)
            .WithOne()
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Contacts).AutoInclude();

        builder.HasMany(p => p.FollowUpRecords)
            .WithOne()
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.FollowUpRecords).AutoInclude();
    }
}

/// <summary>
/// 项目联系人实体 EF Core 映射配置，表名 project_contact
/// </summary>
internal class ProjectContactEntityTypeConfiguration : IEntityTypeConfiguration<ProjectContact>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProjectContact> builder)
    {
        builder.ToTable("project_contact");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.ProjectId).IsRequired();
        builder.Property(x => x.CustomerContactId);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Position).IsRequired(false).HasMaxLength(50);
        builder.Property(x => x.Mobile).IsRequired(false).HasMaxLength(30);
        builder.Property(x => x.OfficePhone).IsRequired(false).HasMaxLength(30);
        builder.Property(x => x.QQ).IsRequired(false).HasMaxLength(30);
        builder.Property(x => x.Wechat).IsRequired(false).HasMaxLength(50);
        builder.Property(x => x.Email).IsRequired(false).HasMaxLength(100);
        builder.Property(x => x.IsPrimary).IsRequired();
        builder.Property(x => x.Remark).IsRequired(false).HasMaxLength(500);
        builder.HasIndex(x => x.ProjectId);
    }
}

/// <summary>
/// 项目跟进记录实体 EF Core 映射配置，表名 project_follow_up_record
/// </summary>
internal class ProjectFollowUpRecordEntityTypeConfiguration : IEntityTypeConfiguration<ProjectFollowUpRecord>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProjectFollowUpRecord> builder)
    {
        builder.ToTable("project_follow_up_record");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.ProjectId).IsRequired();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.VisitDate);
        builder.Property(x => x.ReminderIntervalDays).IsRequired();
        builder.Property(x => x.Content).IsRequired(false).HasMaxLength(4000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatorId);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.VisitDate);
    }
}
