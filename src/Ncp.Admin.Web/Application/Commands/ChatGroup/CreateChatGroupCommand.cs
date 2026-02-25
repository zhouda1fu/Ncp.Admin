using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ChatGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ChatGroup;

/// <summary>
/// 创建单聊命令
/// </summary>
public record CreateSingleChatCommand(UserId CreatorId, UserId OtherUserId) : ICommand<ChatGroupId>;

/// <summary>
/// 创建单聊命令验证器
/// </summary>
public class CreateSingleChatCommandValidator : AbstractValidator<CreateSingleChatCommand>
{
    /// <inheritdoc />
    public CreateSingleChatCommandValidator()
    {
        RuleFor(c => c.CreatorId).NotNull();
        RuleFor(c => c.OtherUserId).NotNull();
        RuleFor(c => c).Must(c => c.CreatorId != c.OtherUserId).WithMessage("不能与自己创建单聊");
    }
}

/// <summary>
/// 创建单聊命令处理器
/// </summary>
public class CreateSingleChatCommandHandler(IChatGroupRepository repository) : ICommandHandler<CreateSingleChatCommand, ChatGroupId>
{
    /// <inheritdoc />
    public async Task<ChatGroupId> Handle(CreateSingleChatCommand request, CancellationToken cancellationToken)
    {
        var group = new Ncp.Admin.Domain.AggregatesModel.ChatGroupAggregate.ChatGroup(request.CreatorId, request.OtherUserId);
        await repository.AddAsync(group, cancellationToken);
        return group.Id;
    }
}

/// <summary>
/// 创建群聊命令
/// </summary>
public record CreateGroupChatCommand(UserId CreatorId, string Name, IReadOnlyList<UserId> MemberIds) : ICommand<ChatGroupId>;

/// <summary>
/// 创建群聊命令验证器
/// </summary>
public class CreateGroupChatCommandValidator : AbstractValidator<CreateGroupChatCommand>
{
    /// <inheritdoc />
    public CreateGroupChatCommandValidator()
    {
        RuleFor(c => c.CreatorId).NotNull();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
    }
}

/// <summary>
/// 创建群聊命令处理器
/// </summary>
public class CreateGroupChatCommandHandler(IChatGroupRepository repository) : ICommandHandler<CreateGroupChatCommand, ChatGroupId>
{
    /// <inheritdoc />
    public async Task<ChatGroupId> Handle(CreateGroupChatCommand request, CancellationToken cancellationToken)
    {
        var group = new Ncp.Admin.Domain.AggregatesModel.ChatGroupAggregate.ChatGroup(
            request.Name, request.CreatorId, request.MemberIds ?? []);
        await repository.AddAsync(group, cancellationToken);
        return group.Id;
    }
}
