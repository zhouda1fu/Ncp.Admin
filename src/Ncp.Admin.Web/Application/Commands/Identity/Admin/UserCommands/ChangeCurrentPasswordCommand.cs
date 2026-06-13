using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;

/// <summary>
/// 当前登录用户修改自己的密码命令。
/// </summary>
public record ChangeCurrentPasswordCommand(UserId UserId, string OldPassword, string NewPassword) : ICommand<bool>;

/// <summary>
/// 当前登录用户修改自己的密码命令验证器。
/// </summary>
public class ChangeCurrentPasswordCommandValidator : AbstractValidator<ChangeCurrentPasswordCommand>
{
    public ChangeCurrentPasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
        RuleFor(x => x.OldPassword).NotEmpty().WithMessage("请输入当前密码");
        RuleFor(x => x.NewPassword).NotEmpty().WithMessage("请输入新密码")
            .MinimumLength(6).WithMessage("新密码长度不能少于6位");
    }
}

/// <summary>
/// 当前登录用户修改自己的密码命令处理器。
/// </summary>
public class ChangeCurrentPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IMemoryCache memoryCache) : ICommandHandler<ChangeCurrentPasswordCommand, bool>
{
    public async Task<bool> Handle(ChangeCurrentPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken)
                   ?? throw new KnownException("当前用户不存在", ErrorCodes.UserNotFound);

        if (!passwordHasher.Verify(request.OldPassword, user.PasswordHash))
        {
            throw new KnownException("当前密码不正确");
        }

        user.UpdatePassword(passwordHasher.Hash(request.NewPassword));
        memoryCache.Remove(UserQuery.GetUserCacheKey(request.UserId));
        return true;
    }
}
