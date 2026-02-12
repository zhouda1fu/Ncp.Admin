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
        builder.Property(n => n.Id).UseSnowFlakeValueGenerator();

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(n => n.Type)
            .IsRequired();

        builder.Property(n => n.Level)
            .IsRequired();

        builder.Property(n => n.SenderId);

        builder.Property(n => n.SenderName)
            .HasMaxLength(100);

        builder.Property(n => n.ReceiverId)
            .IsRequired();

        builder.Property(n => n.IsRead)
            .IsRequired();

        builder.Property(n => n.ReadAt);

        builder.Property(n => n.BusinessId)
            .HasMaxLength(100);

        builder.Property(n => n.BusinessType)
            .HasMaxLength(50);

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.IsDeleted)
            .IsRequired();

        builder.Property(n => n.DeletedAt);

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
