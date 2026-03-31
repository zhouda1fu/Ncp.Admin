using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.OperationLogAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 操作日志实体 EF Core 映射配置，表名 operation_log
/// </summary>
internal class OperationLogEntityTypeConfiguration : IEntityTypeConfiguration<OperationLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OperationLog> builder)
    {
        builder.ToTable("operation_log");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseSnowFlakeValueGenerator().HasComment("操作日志ID");
        builder.Property(x => x.OperatorUserId).IsRequired().HasComment("操作人用户ID");
        builder.Property(x => x.OperatorUserName).IsRequired().HasMaxLength(100).HasComment("操作人姓名");
        builder.Property(x => x.Module).IsRequired().HasMaxLength(64).HasComment("模块名称");
        builder.Property(x => x.OperationType).IsRequired().HasComment("操作类型");
        builder.Property(x => x.RequestPath).IsRequired().HasMaxLength(512).HasComment("请求路径");
        builder.Property(x => x.RequestMethod).IsRequired().HasMaxLength(16).HasComment("HTTP方法");
        builder.Property(x => x.HttpStatusCode).IsRequired().HasComment("HTTP状态码");
        builder.Property(x => x.IsSuccess).IsRequired().HasComment("是否成功");
        builder.Property(x => x.IpAddress).IsRequired().HasMaxLength(64).HasComment("客户端IP");
        builder.Property(x => x.UserAgent).IsRequired().HasMaxLength(512).HasComment("User-Agent");
        builder.Property(x => x.RequestBody).IsRequired().HasMaxLength(4000).HasComment("请求入参(JSON,脱敏/截断)");
        builder.Property(x => x.ResponseBody).IsRequired().HasMaxLength(4000).HasComment("响应出参(JSON,脱敏/截断)");
        builder.Property(x => x.DurationMs).IsRequired().HasComment("请求耗时(毫秒)");
        builder.Property(x => x.CreatedAt).IsRequired().HasComment("操作时间");

        builder.HasIndex(x => x.OperatorUserId);
        builder.HasIndex(x => x.Module);
        builder.HasIndex(x => x.OperationType);
        builder.HasIndex(x => x.CreatedAt);
    }
}
