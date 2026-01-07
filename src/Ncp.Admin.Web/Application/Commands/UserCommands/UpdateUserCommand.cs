using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.UserCommands;

/// <summary>
/// 更新用户命令
/// </summary>
public record UpdateUserCommand(UserId UserId, string Name, string Email, string Phone, string RealName, int Status, string Gender, int Age, DateTimeOffset BirthDate, OrganizationUnitId OrganizationUnitId, string OrganizationUnitName, string PasswordHash) : ICommand<UserId>;

/// <summary>
/// 更新用户命令验证器
/// </summary>
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
        RuleFor(x => x.Name).NotEmpty().WithMessage("用户名不能为空");
    }
}

/// <summary>
/// 更新用户命令处理器
/// </summary>
public class UpdateUserCommandHandler(IUserRepository userRepository) : ICommandHandler<UpdateUserCommand, UserId>
{
    public async Task<UserId> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken) ??
                   throw new KnownException($"未找到用户，UserId = {request.UserId}");

        user.UpdateUserInfo(request.Name, request.Phone, request.RealName, request.Status, request.Email, request.Gender, request.BirthDate);

        // 如果提供了新密码，则更新密码
        if (!string.IsNullOrEmpty(request.PasswordHash))
        {
            user.UpdatePassword(request.PasswordHash);
        }

        // 分配组织架构
        if (request.OrganizationUnitId != new OrganizationUnitId(0) && !string.IsNullOrEmpty(request.OrganizationUnitName))
        {
            var organizationUnit = new UserOrganizationUnit(user.Id, request.OrganizationUnitId, request.OrganizationUnitName);
            user.AssignOrganizationUnit(organizationUnit);
        }

        return user.Id;
    }
}

