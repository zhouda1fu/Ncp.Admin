using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.DeptCommands;

/// <summary>
/// 清除指定用户在所有部门负责人列表中的关联。
/// </summary>
public record ClearUserAsDeptResponsibleUserCommand(UserId UserId) : ICommand;

public class ClearUserAsDeptResponsibleUserCommandValidator : AbstractValidator<ClearUserAsDeptResponsibleUserCommand>
{
    public ClearUserAsDeptResponsibleUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
    }
}

public class ClearUserAsDeptResponsibleUserCommandHandler(IDeptRepository deptRepository)
    : ICommandHandler<ClearUserAsDeptResponsibleUserCommand>
{
    public async Task Handle(ClearUserAsDeptResponsibleUserCommand request, CancellationToken cancellationToken)
    {
        var depts = await deptRepository.GetDeptsWithResponsibleUserAsync(request.UserId, cancellationToken);
        foreach (var dept in depts)
        {
            dept.SetResponsibleUser(request.UserId, setAsResponsible: false, setAsDefault: false);
        }
    }
}
