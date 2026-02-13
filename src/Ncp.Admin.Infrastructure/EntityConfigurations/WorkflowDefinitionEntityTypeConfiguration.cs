using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 流程定义实体类型配置
/// </summary>
internal class WorkflowDefinitionEntityTypeConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("workflow_definition");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).UseGuidVersion7ValueGenerator();

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.Version)
            .IsRequired();

        builder.Property(d => d.Category)
            .HasMaxLength(100);

        builder.Property(d => d.Status)
            .IsRequired();

        builder.Property(d => d.DefinitionJson)
            .HasColumnType("longtext");

        builder.Property(d => d.CreatedBy)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdateTime);

        builder.Property(d => d.IsDeleted)
            .IsRequired();

        builder.Property(d => d.DeletedAt);

        // 索引
        builder.HasIndex(d => d.Name);
        builder.HasIndex(d => d.Category);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.IsDeleted);

        // 流程节点关系
        builder.HasMany(d => d.Nodes)
            .WithOne()
            .HasForeignKey(n => n.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(d => d.Nodes).AutoInclude();

        // 软删除过滤器
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}

/// <summary>
/// 流程节点实体类型配置
/// </summary>
internal class WorkflowNodeEntityTypeConfiguration : IEntityTypeConfiguration<WorkflowNode>
{
    public void Configure(EntityTypeBuilder<WorkflowNode> builder)
    {
        builder.ToTable("workflow_node");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).UseGuidVersion7ValueGenerator();

        builder.Property(n => n.WorkflowDefinitionId)
            .IsRequired();

        builder.Property(n => n.NodeName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.NodeType)
            .IsRequired();

        builder.Property(n => n.AssigneeType)
            .IsRequired();

        builder.Property(n => n.AssigneeValue)
            .HasMaxLength(500);

        builder.Property(n => n.SortOrder)
            .IsRequired();

        builder.Property(n => n.Description)
            .HasMaxLength(500);

        builder.Property(n => n.ConditionExpression)
            .HasMaxLength(1000);

        builder.Property(n => n.TrueNextNodeName)
            .HasMaxLength(200);

        builder.Property(n => n.FalseNextNodeName)
            .HasMaxLength(200);

        // 索引
        builder.HasIndex(n => n.WorkflowDefinitionId);
        builder.HasIndex(n => new { n.WorkflowDefinitionId, n.SortOrder });
    }
}
