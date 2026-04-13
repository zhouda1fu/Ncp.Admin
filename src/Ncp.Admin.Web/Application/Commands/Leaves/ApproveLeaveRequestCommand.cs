using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Leaves;

/// <summary>
/// 审批通过请假申请（工作流完成后由领域事件处理器通过 Mediator 调用）
/// </summary>
public record ApproveLeaveRequestCommand(LeaveRequestId LeaveRequestId) : ICommand;

/// <summary>
/// 审批通过请假申请验证器
/// </summary>
public class ApproveLeaveRequestCommandValidator : AbstractValidator<ApproveLeaveRequestCommand>
{
    public ApproveLeaveRequestCommandValidator()
    {
        RuleFor(c => c.LeaveRequestId).NotNull().WithMessage("请假申请ID不能为空");
    }
}

/// <summary>
/// 审批通过请假申请处理器：更新请假单状态并扣减余额
/// </summary>
public class ApproveLeaveRequestCommandHandler(
    ILeaveRequestRepository leaveRequestRepository,
    ILeaveBalanceRepository leaveBalanceRepository)
    : ICommandHandler<ApproveLeaveRequestCommand>
{
    public async Task Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leave = await leaveRequestRepository.GetAsync(request.LeaveRequestId, cancellationToken)
            ?? throw new KnownException("未找到请假申请", ErrorCodes.LeaveRequestNotFound);

        leave.Approve();

        var year = leave.StartDate.Year;
        var balance = await leaveBalanceRepository.GetByUserYearTypeAsync(leave.ApplicantId, year, leave.LeaveType, cancellationToken);
        if (balance != null)
            balance.Deduct(leave.Days);
    }
}
