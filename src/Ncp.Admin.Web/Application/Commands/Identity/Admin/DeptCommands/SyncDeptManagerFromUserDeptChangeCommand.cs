using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.DeptCommands;

/// <summary>
/// 根据用户部门主管标识变更，同步更新部门聚合的主管用户 ID
/// </summary>
public record SyncDeptManagerFromUserDeptChangeCommand(
    UserId UserId,
    DeptId DeptId,
    bool IsDeptManager) : ICommand;

public class SyncDeptManagerFromUserDeptChangeCommandValidator : AbstractValidator<SyncDeptManagerFromUserDeptChangeCommand>
{
    public SyncDeptManagerFromUserDeptChangeCommandValidator()
    {
        RuleFor(x => x.DeptId).NotEqual(default(DeptId)).WithMessage("部门ID不能为空");
    }
}

public class SyncDeptManagerFromUserDeptChangeCommandHandler(IDeptRepository deptRepository)
    : ICommandHandler<SyncDeptManagerFromUserDeptChangeCommand>
{
    public async Task Handle(SyncDeptManagerFromUserDeptChangeCommand request, CancellationToken cancellationToken)
    {
        var dept = await deptRepository.GetAsync(request.DeptId, cancellationToken);
        if (dept == null)
        {
            return;
        }

        if (request.IsDeptManager)
        {
            dept.SetManagerId(request.UserId);
        }
        else if (dept.ManagerId == request.UserId)
        {
            dept.SetManagerId(new UserId(0));
        }
    }
}
