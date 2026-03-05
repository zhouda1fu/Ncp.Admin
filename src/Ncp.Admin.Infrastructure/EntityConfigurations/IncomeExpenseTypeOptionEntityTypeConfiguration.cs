using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.IncomeExpenseTypeOptionAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class IncomeExpenseTypeOptionEntityTypeConfiguration : IEntityTypeConfiguration<IncomeExpenseTypeOption>
{
    public void Configure(EntityTypeBuilder<IncomeExpenseTypeOption> builder)
    {
        builder.ToTable("income_expense_type_option");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("收支类型选项标识");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100).HasComment("名称");
        builder.Property(x => x.TypeValue).IsRequired().HasComment("类型值");
        builder.Property(x => x.SortOrder).IsRequired().HasComment("排序");
        builder.HasIndex(x => x.SortOrder);
        builder.HasIndex(x => x.TypeValue);
    }
}
