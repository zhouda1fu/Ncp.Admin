using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.DeptCommands;

/// <summary>
/// 将用户追加为部门负责人。
/// </summary>
public record AddUserAsDeptResponsibleUserCommand(
    DeptId DeptId,
    UserId UserId,
    bool SetAsDefault) : ICommand;

public class AddUserAsDeptResponsibleUserCommandValidator
    : AbstractValidator<AddUserAsDeptResponsibleUserCommand>
{
    public AddUserAsDeptResponsibleUserCommandValidator()
    {
        RuleFor(x => x.DeptId).NotEmpty().NotEqual(DeptId.Unassigned).WithMessage("部门ID不能为空");
        RuleFor(x => x.UserId).NotEmpty().NotEqual(UserId.Unassigned).WithMessage("用户ID不能为空");
    }
}

public class AddUserAsDeptResponsibleUserCommandHandler(IDeptRepository deptRepository)
    : ICommandHandler<AddUserAsDeptResponsibleUserCommand>
{
    public async Task Handle(AddUserAsDeptResponsibleUserCommand request, CancellationToken cancellationToken)
    {
        var dept = await deptRepository.GetWithResponsibleUsersAsync(request.DeptId, cancellationToken)
            ?? throw new KnownException($"未找到部门，Id = {request.DeptId}", ErrorCodes.DeptNotFound);

        // 部门聚合负责排重、排序和默认负责人约束，命令只传达负责人状态意图。
        dept.SetResponsibleUser(request.UserId, true, request.SetAsDefault);
    }
}
