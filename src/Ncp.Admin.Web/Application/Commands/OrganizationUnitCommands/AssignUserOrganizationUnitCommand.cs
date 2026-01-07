using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.OrganizationUnitCommands;

/// <summary>
/// 分配用户组织架构命令
/// </summary>
/// <param name="UserId">用户ID</param>
/// <param name="OrganizationUnitId">组织架构ID</param>
/// <param name="OrganizationUnitName">组织架构名称</param>
public record AssignUserOrganizationUnitCommand(UserId UserId, OrganizationUnitId OrganizationUnitId, string OrganizationUnitName) : ICommand;

/// <summary>
/// 分配用户组织架构命令验证器
/// </summary>
public class AssignUserOrganizationUnitCommandValidator : AbstractValidator<AssignUserOrganizationUnitCommand>
{
    public AssignUserOrganizationUnitCommandValidator(UserQuery userQuery, OrganizationUnitQuery organizationUnitQuery)
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
        RuleFor(x => x.OrganizationUnitId).NotEmpty().WithMessage("组织架构ID不能为空");
        RuleFor(x => x.OrganizationUnitName).NotEmpty().WithMessage("组织架构名称不能为空");
        RuleFor(x => x.UserId).MustAsync(async (userId, ct) => await userQuery.DoesUserExist(userId, ct))
            .WithMessage("用户不存在");
        RuleFor(x => x.OrganizationUnitId).MustAsync(async (orgId, ct) => await organizationUnitQuery.DoesOrganizationUnitExist(orgId, ct))
            .WithMessage("组织架构不存在");
    }
}

/// <summary>
/// 分配用户组织架构命令处理器
/// </summary>
public class AssignUserOrganizationUnitCommandHandler(IUserRepository userRepository) : ICommandHandler<AssignUserOrganizationUnitCommand>
{
    public async Task Handle(AssignUserOrganizationUnitCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken) ??
                   throw new KnownException($"未找到用户，UserId = {request.UserId}");

        // 创建用户组织架构关联
        var userOrganizationUnit = new UserOrganizationUnit(
            request.UserId,
            request.OrganizationUnitId,
            request.OrganizationUnitName
        );

        // 更新用户的组织架构
        user.AssignOrganizationUnit(userOrganizationUnit);
    }
}

