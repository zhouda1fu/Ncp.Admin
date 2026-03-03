using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class ProjectIndustryEntityTypeConfiguration : IEntityTypeConfiguration<ProjectIndustry>
{
    public void Configure(EntityTypeBuilder<ProjectIndustry> builder)
    {
        builder.ToTable("project_industry");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.SortOrder).IsRequired();
        builder.HasIndex(x => x.SortOrder);
    }
}
