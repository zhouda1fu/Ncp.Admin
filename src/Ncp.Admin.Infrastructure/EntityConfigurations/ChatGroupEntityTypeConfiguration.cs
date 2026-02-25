using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ChatGroupAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 聊天组实体 EF Core 映射配置，表名 chat_group
/// </summary>
internal class ChatGroupEntityTypeConfiguration : IEntityTypeConfiguration<ChatGroup>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ChatGroup> builder)
    {
        builder.ToTable("chat_group");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.CreatorId);
        builder.HasIndex(x => x.Type);

        builder.HasMany(x => x.Members)
            .WithOne()
            .HasForeignKey("ChatGroupId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Members).HasField("_members").AutoInclude();
    }
}

/// <summary>
/// 聊天组成员实体 EF Core 映射配置，表名 chat_group_member
/// </summary>
internal class ChatGroupMemberEntityTypeConfiguration : IEntityTypeConfiguration<ChatGroupMember>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ChatGroupMember> builder)
    {
        builder.ToTable("chat_group_member");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property<ChatGroupId>("ChatGroupId").IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.JoinedAt).IsRequired();
        builder.HasIndex("ChatGroupId");
        builder.HasIndex(x => x.UserId);
    }
}
