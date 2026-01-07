using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.OrganizationUnitCommands;

/// <summary>
/// 更新组织架构命令
/// </summary>
/// <param name="Id">组织架构ID</param>
/// <param name="Name">组织架构名称</param>
/// <param name="Description">组织架构描述</param>
/// <param name="ParentId">父级组织架构ID</param>
/// <param name="SortOrder">排序顺序</param>
public record UpdateOrganizationUnitCommand(OrganizationUnitId Id, string Name, string Description, OrganizationUnitId ParentId, int SortOrder) : ICommand;

/// <summary>
/// 更新组织架构命令验证器
/// </summary>
public class UpdateOrganizationUnitCommandValidator : AbstractValidator<UpdateOrganizationUnitCommand>
{
    public UpdateOrganizationUnitCommandValidator(OrganizationUnitQuery organizationUnitQuery)
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("组织架构ID不能为空");
        RuleFor(x => x.Name).NotEmpty().WithMessage("组织架构名称不能为空");
        RuleFor(x => x.Description).MaximumLength(200).WithMessage("组织架构描述长度不能超过200个字符");
        RuleFor(x => x.ParentId).MustAsync(async (parentId, ct) =>
        {
            if (parentId == new OrganizationUnitId(0)) return true; // 根节点
            return await organizationUnitQuery.DoesOrganizationUnitExist(parentId, ct);
        }).WithMessage("父级组织架构不存在");
        RuleFor(x => x).Must(x => x.ParentId != x.Id).WithMessage("不能将自己设置为父级组织架构");
    }
}

/// <summary>
/// 更新组织架构命令处理器
/// </summary>
public class UpdateOrganizationUnitCommandHandler(IOrganizationUnitRepository organizationUnitRepository) : ICommandHandler<UpdateOrganizationUnitCommand>
{
    public async Task Handle(UpdateOrganizationUnitCommand request, CancellationToken cancellationToken)
    {
        var organizationUnit = await organizationUnitRepository.GetAsync(request.Id, cancellationToken) ??
                               throw new KnownException($"未找到组织架构，Id = {request.Id}");

        organizationUnit.UpdateInfo(
            request.Name,
            request.Description,
            request.ParentId,
            request.SortOrder);
    }
}

