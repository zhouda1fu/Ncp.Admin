using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ExpenseAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 报销单实体 EF Core 映射配置，表名 expense_claim，含与 expense_item 的一对多关系
/// </summary>
internal class ExpenseClaimEntityTypeConfiguration : IEntityTypeConfiguration<ExpenseClaim>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ExpenseClaim> builder)
    {
        builder.ToTable("expense_claim");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.ApplicantId).IsRequired();
        builder.Property(x => x.ApplicantName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.TotalAmount).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.WorkflowInstanceId);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.HasIndex(x => x.ApplicantId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey("ExpenseClaimId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Items).AutoInclude();
    }
}

/// <summary>
/// 报销明细实体 EF Core 映射配置，表名 expense_item，外键为影子属性 ExpenseClaimId
/// </summary>
internal class ExpenseItemEntityTypeConfiguration : IEntityTypeConfiguration<ExpenseItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ExpenseItem> builder)
    {
        builder.ToTable("expense_item");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property<ExpenseClaimId>("ExpenseClaimId").IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Amount).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.InvoiceUrl).HasMaxLength(500);
        builder.HasIndex("ExpenseClaimId");
    }
}
