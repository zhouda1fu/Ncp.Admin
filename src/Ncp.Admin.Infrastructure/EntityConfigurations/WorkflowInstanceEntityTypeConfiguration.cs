using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 流程实例实体类型配置
/// </summary>
internal class WorkflowInstanceEntityTypeConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("workflow_instance");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).UseGuidVersion7ValueGenerator();

        builder.Property(i => i.WorkflowDefinitionId)
            .IsRequired();

        builder.Property(i => i.WorkflowDefinitionName)
            .HasMaxLength(200);

        builder.Property(i => i.BusinessKey)
            .HasMaxLength(200);

        builder.Property(i => i.BusinessType)
            .HasMaxLength(100);

        builder.Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.InitiatorId)
            .IsRequired();

        builder.Property(i => i.InitiatorName)
            .HasMaxLength(100);

        builder.Property(i => i.InitiatorDeptId)
            .IsRequired();

        builder.Property(i => i.Status)
            .IsRequired();

        builder.Property(i => i.CurrentNodeName)
            .HasMaxLength(200);

        builder.Property(i => i.StartedAt)
            .IsRequired();

        builder.Property(i => i.CompletedAt);

        builder.Property(i => i.Variables)
            .HasColumnType("longtext");

        builder.Property(i => i.Remark)
            .HasMaxLength(1000);

        builder.Property(i => i.FailureReason)
            .HasMaxLength(2000);

        // 索引
        builder.HasIndex(i => i.WorkflowDefinitionId);
        builder.HasIndex(i => i.BusinessKey);
        builder.HasIndex(i => i.BusinessType);
        builder.HasIndex(i => i.InitiatorId);
        builder.HasIndex(i => i.InitiatorDeptId);
        builder.HasIndex(i => i.Status);

        // 任务关系
        builder.HasMany(i => i.Tasks)
            .WithOne()
            .HasForeignKey(t => t.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(i => i.Tasks).AutoInclude();
    }
}

/// <summary>
/// 工作流任务实体类型配置
/// </summary>
internal class WorkflowTaskEntityTypeConfiguration : IEntityTypeConfiguration<WorkflowTask>
{
    public void Configure(EntityTypeBuilder<WorkflowTask> builder)
    {
        builder.ToTable("workflow_task");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseGuidVersion7ValueGenerator();

        builder.Property(t => t.WorkflowInstanceId)
            .IsRequired();

        builder.Property(t => t.NodeName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.TaskType)
            .IsRequired();

        builder.Property(t => t.AssigneeType)
            .IsRequired();

        builder.Property(t => t.AssigneeId);

        builder.Property(t => t.AssigneeRoleId);

        builder.Property(t => t.AssigneeName)
            .HasMaxLength(100);

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.Comment)
            .HasMaxLength(1000);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.CompletedAt);

        // 索引
        builder.HasIndex(t => t.WorkflowInstanceId);
        builder.HasIndex(t => t.AssigneeId);
        builder.HasIndex(t => t.AssigneeRoleId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => new { t.AssigneeId, t.Status });
    }
}
