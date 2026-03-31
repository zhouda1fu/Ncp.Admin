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
        builder.Property(i => i.Id).UseGuidVersion7ValueGenerator().HasComment("主键");

        builder.Property(i => i.WorkflowDefinitionId)
            .IsRequired()
            .HasComment("流程定义ID");

        builder.Property(i => i.WorkflowDefinitionName)
            .HasMaxLength(200)
            .HasComment("流程定义名称");

        builder.Property(i => i.BusinessKey)
            .HasMaxLength(200)
            .HasComment("业务关联键");

        builder.Property(i => i.BusinessType)
            .HasMaxLength(100)
            .HasComment("业务类型");

        builder.Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("流程标题");

        builder.Property(i => i.InitiatorId)
            .IsRequired()
            .HasComment("发起人ID");

        builder.Property(i => i.InitiatorName)
            .HasMaxLength(100)
            .HasComment("发起人姓名");

        builder.Property(i => i.InitiatorDeptId)
            .IsRequired()
            .HasComment("发起人部门ID");

        builder.Property(i => i.Status)
            .IsRequired()
            .HasComment("流程状态");

        builder.Property(i => i.CurrentNodeKey)
            .HasMaxLength(100)
            .HasComment("当前节点key");

        builder.Property(i => i.CurrentNodeName)
            .HasMaxLength(200)
            .HasComment("当前节点名称");

        builder.Property(i => i.StartedAt)
            .IsRequired()
            .HasComment("开始时间");

        builder.Property(i => i.CompletedAt)
            .HasComment("完成时间");

        builder.Property(i => i.Variables)
            .HasColumnType("text")
            .HasComment("流程变量JSON");

        builder.Property(i => i.Remark)
            .HasMaxLength(1000)
            .HasComment("备注");

        builder.Property(i => i.FailureReason)
            .HasMaxLength(2000)
            .HasComment("业务执行失败原因");

        // 索引
        builder.HasIndex(i => i.WorkflowDefinitionId);
        builder.HasIndex(i => i.BusinessKey);
        builder.HasIndex(i => i.BusinessType);
        builder.HasIndex(i => i.InitiatorId);
        builder.HasIndex(i => i.InitiatorDeptId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => new { i.BusinessType, i.BusinessKey, i.Status });

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
        builder.Property(t => t.Id).UseGuidVersion7ValueGenerator().HasComment("主键");

        builder.Property(t => t.WorkflowInstanceId)
            .IsRequired()
            .HasComment("流程实例ID");

        builder.Property(t => t.NodeKey)
            .HasMaxLength(100)
            .HasComment("节点key");

        builder.Property(t => t.NodeName)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("节点名称");

        builder.Property(t => t.TaskType)
            .IsRequired()
            .HasComment("任务类型：0审批 1通知 2抄送");

        builder.Property(t => t.AssigneeType)
            .IsRequired()
            .HasComment("处理人类型：0用户 1角色");

        builder.Property(t => t.AssigneeId)
            .IsRequired()
            .HasComment("处理人用户ID（按角色任务时为 0）");

        builder.Property(t => t.AssigneeRoleId)
            .IsRequired()
            .HasComment("处理人角色ID（按用户任务时为 Guid.Empty）");

        builder.Property(t => t.AssigneeName)
            .HasMaxLength(100)
            .HasComment("处理人姓名/角色名");

        builder.Property(t => t.Status)
            .IsRequired()
            .HasComment("任务状态");

        builder.Property(t => t.Comment)
            .HasMaxLength(1000)
            .HasComment("审批意见");

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasComment("创建时间");

        builder.Property(t => t.CompletedAt)
            .HasComment("完成时间");

        // 索引
        builder.HasIndex(t => t.WorkflowInstanceId);
        builder.HasIndex(t => t.AssigneeId);
        builder.HasIndex(t => t.AssigneeRoleId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => new { t.AssigneeId, t.Status });
    }
}
