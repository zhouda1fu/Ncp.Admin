using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 请假申请实体类型配置
/// </summary>
internal class LeaveRequestEntityTypeConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("leave_request");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).UseGuidVersion7ValueGenerator();

        builder.Property(r => r.ApplicantId).IsRequired();
        builder.Property(r => r.ApplicantName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.LeaveType).IsRequired();
        builder.Property(r => r.StartDate).IsRequired();
        builder.Property(r => r.EndDate).IsRequired();
        builder.Property(r => r.Days).IsRequired();
        builder.Property(r => r.Reason).HasMaxLength(500);
        builder.Property(r => r.Status).IsRequired();
        builder.Property(r => r.WorkflowInstanceId);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdateTime);

        builder.HasIndex(r => r.ApplicantId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.WorkflowInstanceId);
        builder.HasIndex(r => r.CreatedAt);
    }
}
