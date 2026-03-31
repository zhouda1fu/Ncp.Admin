using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 订单推送实体配置
/// </summary>
internal class OrderPushEntityTypeConfiguration : IEntityTypeConfiguration<OrderPush>
{
    /// <summary>
    /// 配置订单推送实体
    /// </summary>
    /// <param name="builder">实体构建器</param>
    public void Configure(EntityTypeBuilder<OrderPush> builder)
    {
        // 表名
        builder.ToTable("order_push");
        
        // 主键
        builder.HasKey(x => x.Id);
        
        // ID 生成策略：使用 Guid Version 7
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("订单推送标识");
        
        // 订单 ID：必填
        builder.Property(x => x.OrderId).IsRequired().HasComment("订单ID");
        
        // 类别：必填
        builder.Property(x => x.Type).IsRequired().HasComment("推送类型");
        
        // 推送人用户 ID：必填
        builder.Property(x => x.PusherId).IsRequired().HasComment("推送人用户ID");
        
        // 推送人姓名：必填，最大长度 50
        builder.Property(x => x.PusherName).IsRequired().HasMaxLength(50).HasComment("推送人姓名");
        
        // 推送时间：必填
        builder.Property(x => x.PushTime).IsRequired().HasComment("推送时间");
        
        // 进程：必填
        builder.Property(x => x.Process).IsRequired().HasComment("推送进程状态码");
        
        // 进程名称：必填，最大长度 100
        builder.Property(x => x.ProcessName).IsRequired().HasMaxLength(100).HasComment("推送进程状态描述");
        
        // 原因：最大长度 500
        builder.Property(x => x.Reason).HasMaxLength(500).HasComment("推送原因或失败信息");

        // 索引配置
        // 按订单 ID 查询
        builder.HasIndex(x => x.OrderId);
        // 按类别查询
        builder.HasIndex(x => x.Type);
        // 按推送人查询
        builder.HasIndex(x => x.PusherId);
        // 按推送时间查询
        builder.HasIndex(x => x.PushTime);
        // 按进程状态查询
        builder.HasIndex(x => x.Process);
    }
}