using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.DeptCommands;

/// <summary>
/// 清除指定用户在所有部门上的主管关联（将 ManagerId 从该用户改为无主管）
/// </summary>
public record ClearUserAsDeptManagerCommand(UserId UserId) : ICommand;

public class ClearUserAsDeptManagerCommandValidator : AbstractValidator<ClearUserAsDeptManagerCommand>
{
    public ClearUserAsDeptManagerCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
    }
}

public class ClearUserAsDeptManagerCommandHandler(IDeptRepository deptRepository)
    : ICommandHandler<ClearUserAsDeptManagerCommand>
{
    public async Task Handle(ClearUserAsDeptManagerCommand request, CancellationToken cancellationToken)
    {
        var deptIds = await deptRepository.GetDeptIdsWhereManagerIsUserAsync(request.UserId, cancellationToken);
        foreach (var deptId in deptIds)
        {
            var dept = await deptRepository.GetAsync(deptId, cancellationToken);
            if (dept != null && dept.ManagerId == request.UserId)
            {
                dept.SetManagerId(new UserId(0));
            }
        }
    }
}
