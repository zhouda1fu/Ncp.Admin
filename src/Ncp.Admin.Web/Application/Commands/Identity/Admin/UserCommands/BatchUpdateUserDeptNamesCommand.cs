using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;

/// <summary>
/// 批量更新用户部门名称命令（用于部门信息变更时同步更新所有关联用户的部门名称）
/// </summary>
public record BatchUpdateUserDeptNamesCommand(
    DeptId DeptId,
    string NewDeptName) : ICommand;

/// <summary>
/// 批量更新用户部门名称命令验证器
/// </summary>
public class BatchUpdateUserDeptNamesCommandValidator : AbstractValidator<BatchUpdateUserDeptNamesCommand>
{
    public BatchUpdateUserDeptNamesCommandValidator()
    {
        RuleFor(x => x.DeptId).NotEmpty().WithMessage("部门ID不能为空");
        RuleFor(x => x.NewDeptName).NotEmpty().WithMessage("部门名称不能为空");
    }
}

/// <summary>
/// 批量更新用户部门名称命令处理器
/// </summary>
public class BatchUpdateUserDeptNamesCommandHandler(IUserRepository userRepository)
    : ICommandHandler<BatchUpdateUserDeptNamesCommand>
{
    public async Task Handle(BatchUpdateUserDeptNamesCommand request, CancellationToken cancellationToken)
    {
        await userRepository.BulkUpdateUserDeptNamesAsync(
            request.DeptId,
            request.NewDeptName,
            cancellationToken);
    }
}
