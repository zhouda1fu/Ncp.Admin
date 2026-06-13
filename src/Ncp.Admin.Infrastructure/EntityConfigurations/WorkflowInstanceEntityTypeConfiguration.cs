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

        builder.Property(i => i.WorkflowDefinitionVersionId)
            .IsRequired()
            .HasComment("流程定义版本ID");

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

        builder.Property(i => i.SuspendedAt)
            .HasComment("最近一次挂起时间");

        builder.Property(i => i.ResumedAt)
            .HasComment("最近一次恢复时间");

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
        builder.HasIndex(i => i.WorkflowDefinitionVersionId);
        builder.HasIndex(i => i.BusinessKey);
        builder.HasIndex(i => i.BusinessType);
        builder.HasIndex(i => i.InitiatorId);
        builder.HasIndex(i => i.InitiatorDeptId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => new { i.BusinessType, i.BusinessKey, i.Status });
        builder.HasIndex(i => new { i.BusinessType, i.BusinessKey })
            .HasDatabaseName("ix_workflow_instance_active_business")
            .HasFilter("\"Status\" IN (0, 1)")
            .IsUnique();

        // 任务关系
        builder.HasMany(i => i.Tasks)
            .WithOne()
            .HasForeignKey(t => t.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
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

        builder.Property(t => t.ExtraDataJson)
            .HasColumnType("text")
            .HasComment("任务扩展数据JSON");

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasComment("创建时间");

        builder.Property(t => t.CompletedAt)
            .HasComment("完成时间");

        builder.Property(t => t.CompletedByUserId)
            .IsRequired()
            .HasComment("审批通过时的实际操作人用户ID（角色任务等用于追溯）");

        // 索引
        builder.HasIndex(t => t.WorkflowInstanceId);
        builder.HasIndex(t => t.AssigneeId);
        builder.HasIndex(t => t.AssigneeRoleId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => new { t.AssigneeId, t.Status });

        builder.HasMany(t => t.AssignmentSnapshots)
            .WithOne()
            .HasForeignKey(s => s.WorkflowTaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// 工作流任务授权快照实体类型配置
/// </summary>
internal class WorkflowTaskAssignmentSnapshotEntityTypeConfiguration : IEntityTypeConfiguration<WorkflowTaskAssignmentSnapshot>
{
    public void Configure(EntityTypeBuilder<WorkflowTaskAssignmentSnapshot> builder)
    {
        builder.ToTable("workflow_task_assignment_snapshot");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).UseGuidVersion7ValueGenerator().HasComment("主键");

        builder.Property(s => s.WorkflowTaskId)
            .IsRequired()
            .HasComment("任务ID");

        builder.Property(s => s.AssigneeType)
            .IsRequired()
            .HasComment("处理人类型：0用户 1角色");

        builder.Property(s => s.AssigneeUserId)
            .IsRequired()
            .HasComment("处理人用户ID");

        builder.Property(s => s.AssigneeRoleId)
            .IsRequired()
            .HasComment("处理人角色ID");

        builder.Property(s => s.AssigneeDisplayName)
            .HasMaxLength(100)
            .HasComment("处理人显示名");

        builder.Property(s => s.AssignmentSource)
            .IsRequired()
            .HasComment("授权来源");

        builder.Property(s => s.SourceRuleId)
            .HasMaxLength(100)
            .HasComment("来源规则ID");

        builder.Property(s => s.VisibilityMode)
            .IsRequired()
            .HasComment("可见性模式");

        builder.Property(s => s.BypassDataPermission)
            .IsRequired()
            .HasComment("是否绕过常规数据权限过滤");

        builder.Property(s => s.InitiatorDeptScopeMode)
            .IsRequired()
            .HasComment("发起部门范围模式");

        builder.Property(s => s.InitiatorDeptScopeDeptIdsJson)
            .HasColumnType("text")
            .HasComment("配置的发起部门范围JSON");

        builder.Property(s => s.CreatedReason)
            .HasMaxLength(500)
            .HasComment("创建原因");

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasComment("创建时间");

        builder.HasIndex(s => s.WorkflowTaskId);
        builder.HasIndex(s => s.AssigneeUserId);
        builder.HasIndex(s => s.AssigneeRoleId);
        builder.HasIndex(s => new { s.AssigneeUserId, s.WorkflowTaskId });
        builder.HasIndex(s => new { s.AssigneeRoleId, s.WorkflowTaskId });
    }
}
