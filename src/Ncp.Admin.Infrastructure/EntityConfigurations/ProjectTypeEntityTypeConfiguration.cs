using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class ProjectTypeEntityTypeConfiguration : IEntityTypeConfiguration<ProjectType>
{
    public void Configure(EntityTypeBuilder<ProjectType> builder)
    {
        builder.ToTable("project_type");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("项目类型标识");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100).HasComment("名称");
        builder.Property(x => x.SortOrder).IsRequired().HasComment("排序");
        builder.HasIndex(x => x.SortOrder);
    }
}
