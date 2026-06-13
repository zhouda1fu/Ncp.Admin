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
        builder.Property(d => d.Id).UseGuidVersion7ValueGenerator().HasComment("主键");

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("流程名称");

        builder.Property(d => d.Description)
            .HasMaxLength(500)
            .HasComment("流程描述");

        builder.Property(d => d.Version)
            .IsRequired()
            .HasComment("版本号");

        builder.Property(d => d.Category)
            .HasMaxLength(100)
            .HasComment("流程分类");

        builder.Property(d => d.Status)
            .IsRequired()
            .HasComment("流程状态：0草稿 1已发布 2已归档");

        builder.Property(d => d.DesignerSchemaJson)
            .HasColumnType("text")
            .HasComment("设计器 Schema JSON");

        builder.Property(d => d.BasedOnId)
            .IsRequired()
            .HasComment("基于哪条流程定义创建（新版本时指向源定义，发布时据此归档源）");

        builder.Property(d => d.CreatedBy)
            .IsRequired()
            .HasComment("创建人ID");

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasComment("创建时间");

        builder.Property(d => d.UpdateTime)
            .HasComment("更新时间");

        builder.Property(d => d.IsDeleted)
            .IsRequired()
            .HasComment("是否删除");

        builder.Property(d => d.DeletedAt)
            .HasComment("删除时间");

        builder.HasMany(d => d.Versions)
            .WithOne()
            .HasForeignKey(v => v.WorkflowDefinitionId);
        builder.Navigation(d => d.Versions).AutoInclude();

        // 索引
        builder.HasIndex(d => d.Name);
        builder.HasIndex(d => d.Category);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.BasedOnId);
        builder.HasIndex(d => d.IsDeleted);

        // 软删除过滤器
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}

/// <summary>
/// 流程定义版本实体类型配置
/// </summary>
internal class WorkflowDefinitionVersionEntityTypeConfiguration : IEntityTypeConfiguration<WorkflowDefinitionVersion>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinitionVersion> builder)
    {
        builder.ToTable("workflow_definition_version");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).UseGuidVersion7ValueGenerator().HasComment("主键");

        builder.Property(v => v.WorkflowDefinitionId)
            .IsRequired()
            .HasComment("流程定义ID");

        builder.Property(v => v.Version)
            .IsRequired()
            .HasComment("版本号");

        builder.Property(v => v.Status)
            .IsRequired()
            .HasComment("版本状态：0草稿 1已发布 2已归档");

        builder.Property(v => v.DesignerSchemaJson)
            .HasColumnType("text")
            .HasComment("前端设计器 JSON");

        builder.Property(v => v.GraphSnapshotJson)
            .HasColumnType("text")
            .HasComment("发布后的运行图快照 JSON");

        builder.Property(v => v.CreatedAt)
            .IsRequired()
            .HasComment("创建时间");

        builder.Property(v => v.UpdateTime)
            .HasComment("更新时间");

        builder.Property(v => v.PublishedBy)
            .IsRequired()
            .HasComment("发布人ID");

        builder.Property(v => v.PublishedAt)
            .HasComment("发布时间");

        builder.HasIndex(v => v.WorkflowDefinitionId);
        builder.HasIndex(v => new { v.WorkflowDefinitionId, v.Version }).IsUnique();
        builder.HasIndex(v => new { v.WorkflowDefinitionId, v.Status });
    }
}
