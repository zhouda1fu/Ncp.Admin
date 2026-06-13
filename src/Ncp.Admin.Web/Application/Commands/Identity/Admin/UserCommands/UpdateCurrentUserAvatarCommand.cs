using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;

/// <summary>
/// 当前登录用户更新头像命令。
/// </summary>
public record UpdateCurrentUserAvatarCommand(UserId UserId, string AvatarUrl) : ICommand<bool>;

/// <summary>
/// 当前登录用户更新头像命令验证器。
/// </summary>
public class UpdateCurrentUserAvatarCommandValidator : AbstractValidator<UpdateCurrentUserAvatarCommand>
{
    public UpdateCurrentUserAvatarCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
        RuleFor(x => x.AvatarUrl).NotEmpty().WithMessage("头像地址不能为空");
    }
}

/// <summary>
/// 当前登录用户更新头像命令处理器。
/// </summary>
public class UpdateCurrentUserAvatarCommandHandler(
    IUserRepository userRepository,
    IMemoryCache memoryCache) : ICommandHandler<UpdateCurrentUserAvatarCommand, bool>
{
    public async Task<bool> Handle(UpdateCurrentUserAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken)
                   ?? throw new KnownException("当前用户不存在", ErrorCodes.UserNotFound);

        user.UpdateAvatar(request.AvatarUrl, request.UserId);
        memoryCache.Remove(UserQuery.GetUserCacheKey(request.UserId));
        return true;
    }
}
