using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class ProjectStatusOptionEntityTypeConfiguration : IEntityTypeConfiguration<ProjectStatusOption>
{
    public void Configure(EntityTypeBuilder<ProjectStatusOption> builder)
    {
        builder.ToTable("project_status_option");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("项目状态选项标识");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100).HasComment("名称");
        builder.Property(x => x.Code).HasMaxLength(50).HasComment("编码");
        builder.Property(x => x.SortOrder).IsRequired().HasComment("排序");
        builder.HasIndex(x => x.SortOrder);
    }
}
