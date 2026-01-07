using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.OrganizationUnitCommands;

/// <summary>
/// 删除组织架构命令
/// </summary>
/// <param name="Id">组织架构ID</param>
public record DeleteOrganizationUnitCommand(OrganizationUnitId Id) : ICommand;

/// <summary>
/// 删除组织架构命令验证器
/// </summary>
public class DeleteOrganizationUnitCommandValidator : AbstractValidator<DeleteOrganizationUnitCommand>
{
    public DeleteOrganizationUnitCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("组织架构ID不能为空");
    }
}

/// <summary>
/// 删除组织架构命令处理器
/// </summary>
public class DeleteOrganizationUnitCommandHandler(IOrganizationUnitRepository organizationUnitRepository) : ICommandHandler<DeleteOrganizationUnitCommand>
{
    public async Task Handle(DeleteOrganizationUnitCommand request, CancellationToken cancellationToken)
    {
        var organizationUnit = await organizationUnitRepository.GetAsync(request.Id, cancellationToken) ??
                               throw new KnownException($"未找到组织架构，Id = {request.Id}");

        organizationUnit.SoftDelete();
    }
}

