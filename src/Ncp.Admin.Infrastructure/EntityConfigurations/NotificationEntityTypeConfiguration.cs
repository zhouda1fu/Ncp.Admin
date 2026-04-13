using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 通知实体类型配置
/// </summary>
internal class NotificationEntityTypeConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notification");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).UseSnowFlakeValueGenerator().HasComment("通知标识");

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("标题");

        builder.Property(n => n.Content)
            .IsRequired()
            .HasMaxLength(2000)
            .HasComment("内容");

        builder.Property(n => n.Type)
            .IsRequired()
            .HasComment("类型");

        builder.Property(n => n.Level)
            .IsRequired()
            .HasComment("等级");

        builder.Property(n => n.SenderId).HasComment("发送人用户ID");

        builder.Property(n => n.SenderName)
            .HasMaxLength(100)
            .HasComment("发送人姓名");

        builder.Property(n => n.ReceiverId)
            .IsRequired()
            .HasComment("接收人用户ID");

        builder.Property(n => n.IsRead)
            .IsRequired()
            .HasComment("是否已读");

        builder.Property(n => n.ReadAt).HasComment("已读时间");

        builder.Property(n => n.BusinessId)
            .HasMaxLength(100)
            .HasComment("业务ID（字符串）");

        builder.Property(n => n.BusinessType)
            .HasMaxLength(50)
            .HasComment("业务类型");

        builder.Property(n => n.CreatedAt)
            .IsRequired()
            .HasComment("创建时间");

        builder.Property(n => n.IsDeleted)
            .IsRequired()
            .HasComment("是否软删");

        builder.Property(n => n.DeletedAt).HasComment("删除时间");

        // 索引
        builder.HasIndex(n => n.ReceiverId);
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.Type);
        builder.HasIndex(n => n.IsDeleted);
        builder.HasIndex(n => new { n.ReceiverId, n.IsRead, n.IsDeleted });

        // 软删除过滤器
        builder.HasQueryFilter(n => !n.IsDeleted);
    }
}
