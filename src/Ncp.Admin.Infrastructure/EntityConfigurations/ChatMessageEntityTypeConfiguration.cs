using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ChatMessageAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 聊天消息实体 EF Core 映射配置，表名 chat_message
/// </summary>
internal class ChatMessageEntityTypeConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("chat_message");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.ChatGroupId).IsRequired();
        builder.Property(x => x.SenderId).IsRequired();
        builder.Property(x => x.Content).IsRequired().HasMaxLength(4000);
        builder.Property(x => x.ReplyToMessageId);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.ChatGroupId);
        builder.HasIndex(x => x.SenderId);
        builder.HasIndex(x => x.CreatedAt);
    }
}
