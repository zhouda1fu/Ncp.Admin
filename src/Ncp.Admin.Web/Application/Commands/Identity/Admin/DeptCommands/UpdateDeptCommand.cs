using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.DeptCommands;

/// <summary>
/// 更新部门命令
/// </summary>
public record UpdateDeptCommand(DeptId Id, string Name, string Remark, DeptId ParentId, int Status, UserId ManagerId) : ICommand;

public class UpdateDeptCommandValidator : AbstractValidator<UpdateDeptCommand>
{
    public UpdateDeptCommandValidator(DeptQuery deptQuery)
    {
        RuleFor(d => d.Name).NotEmpty().WithMessage("部门名称不能为空");
        RuleFor(d => d.Status).InclusiveBetween(0, 1).WithMessage("状态值必须为0或1");
        RuleFor(d => d.ManagerId).NotNull().WithMessage("部门主管不能为空");
        RuleFor(d => d.ParentId)
            .MustAsync(async (cmd, parentId, ct) =>
            {
                var forbiddenIds = await deptQuery.GetAllChildDeptIdsAsync(cmd.Id, ct);
                return !forbiddenIds.Contains(parentId);
            })
            .WithMessage("上级部门不能为自己或自己的下级部门");
    }
}

/// <summary>
/// 更新部门命令处理器
/// </summary>
public class UpdateDeptCommandHandler(IDeptRepository deptRepository) : ICommandHandler<UpdateDeptCommand>
{
    public async Task Handle(UpdateDeptCommand request, CancellationToken cancellationToken)
    {
        var dept = await deptRepository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException($"未找到部门，Id = {request.Id}", ErrorCodes.DeptNotFound);

        dept.UpdateInfo(request.Name, request.Remark, request.ParentId, request.Status, request.ManagerId);
    }
}
