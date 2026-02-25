using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using TaskEntity = Ncp.Admin.Domain.AggregatesModel.TaskAggregate.Task;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 任务实体 EF Core 映射配置，表名 task，含与 task_comment 的一对多关系
/// </summary>
internal class TaskEntityTypeConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        builder.ToTable("task");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.ProjectId).IsRequired();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.AssigneeId);
        builder.Property(x => x.DueDate);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.AssigneeId);

        builder.HasMany(x => x.Comments)
            .WithOne()
            .HasForeignKey("TaskId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Comments).AutoInclude();
    }
}

/// <summary>
/// 任务评论实体 EF Core 映射配置，表名 task_comment
/// </summary>
internal class TaskCommentEntityTypeConfiguration : IEntityTypeConfiguration<TaskComment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TaskComment> builder)
    {
        builder.ToTable("task_comment");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property<TaskId>("TaskId").IsRequired();
        builder.Property(x => x.Content).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.AuthorId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex("TaskId");
    }
}
