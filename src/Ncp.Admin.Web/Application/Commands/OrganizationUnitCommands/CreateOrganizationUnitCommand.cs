using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.OrganizationUnitCommands;

/// <summary>
/// 创建组织架构命令
/// </summary>
/// <param name="Name">组织架构名称</param>
/// <param name="Description">组织架构描述</param>
/// <param name="ParentId">父级组织架构ID</param>
/// <param name="SortOrder">排序顺序</param>
public record CreateOrganizationUnitCommand(string Name, string Description, OrganizationUnitId ParentId, int SortOrder) : ICommand<OrganizationUnitId>;

/// <summary>
/// 创建组织架构命令验证器
/// </summary>
public class CreateOrganizationUnitCommandValidator : AbstractValidator<CreateOrganizationUnitCommand>
{
    public CreateOrganizationUnitCommandValidator(OrganizationUnitQuery organizationUnitQuery)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("组织架构名称不能为空");
        RuleFor(x => x.Description).MaximumLength(200).WithMessage("组织架构描述长度不能超过200个字符");
        RuleFor(x => x.Name).MustAsync(async (n, ct) => !await organizationUnitQuery.DoesOrganizationUnitExist(n, ct))
            .WithMessage(x => $"该组织架构已存在，Name={x.Name}");
        RuleFor(x => x.ParentId).MustAsync(async (parentId, ct) =>
        {
            if (parentId == new OrganizationUnitId(0)) return true; // 根节点
            return await organizationUnitQuery.DoesOrganizationUnitExist(parentId, ct);
        }).WithMessage("父级组织架构不存在");
    }
}

/// <summary>
/// 创建组织架构命令处理器
/// </summary>
public class CreateOrganizationUnitCommandHandler(IOrganizationUnitRepository organizationUnitRepository) : ICommandHandler<CreateOrganizationUnitCommand, OrganizationUnitId>
{
    public async Task<OrganizationUnitId> Handle(CreateOrganizationUnitCommand request, CancellationToken cancellationToken)
    {
        var organizationUnit = new OrganizationUnit(
            request.Name,
            request.Description,
            request.ParentId,
            request.SortOrder);

        await organizationUnitRepository.AddAsync(organizationUnit, cancellationToken);
        return organizationUnit.Id;
    }
}

