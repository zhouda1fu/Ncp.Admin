using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 项目任务实体 EF Core 映射配置，表名 project_task，含与 project_task_comment 的一对多关系
/// </summary>
internal class ProjectTaskEntityTypeConfiguration : IEntityTypeConfiguration<ProjectTask>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        builder.ToTable("project_task");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("任务标识");
        builder.Property(x => x.ProjectId).IsRequired().HasComment("项目ID");
        builder.Property(x => x.Title).IsRequired().HasMaxLength(500).HasComment("任务标题");
        builder.Property(x => x.Description).HasMaxLength(2000).HasComment("任务描述");
        builder.Property(x => x.AssigneeId).HasComment("负责人用户ID");
        builder.Property(x => x.DueDate).HasComment("截止日期");
        builder.Property(x => x.Status).IsRequired().HasComment("任务状态");
        builder.Property(x => x.SortOrder).IsRequired().HasComment("排序");
        builder.Property(x => x.CreatedAt).IsRequired().HasComment("创建时间");
        builder.Property(x => x.UpdateTime).HasComment("更新时间");
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.AssigneeId);

        builder.HasMany(x => x.Comments)
            .WithOne()
            .HasForeignKey("ProjectTaskId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Comments).AutoInclude();
    }
}

/// <summary>
/// 项目任务评论实体 EF Core 映射配置，表名 project_task_comment
/// </summary>
internal class ProjectTaskCommentEntityTypeConfiguration : IEntityTypeConfiguration<ProjectTaskComment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProjectTaskComment> builder)
    {
        builder.ToTable("project_task_comment");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property<ProjectTaskId>("ProjectTaskId").IsRequired();
        builder.Property(x => x.Content).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.AuthorId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex("ProjectTaskId");
    }
}
