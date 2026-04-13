using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Leaves;

/// <summary>
/// 驳回请假申请（工作流驳回时由领域事件处理器通过 Mediator 调用）
/// </summary>
public record RejectLeaveRequestCommand(LeaveRequestId LeaveRequestId) : ICommand;

/// <summary>
/// 驳回请假申请验证器
/// </summary>
public class RejectLeaveRequestCommandValidator : AbstractValidator<RejectLeaveRequestCommand>
{
    public RejectLeaveRequestCommandValidator()
    {
        RuleFor(c => c.LeaveRequestId).NotNull().WithMessage("请假申请ID不能为空");
    }
}

/// <summary>
/// 驳回请假申请处理器
/// </summary>
public class RejectLeaveRequestCommandHandler(ILeaveRequestRepository leaveRequestRepository)
    : ICommandHandler<RejectLeaveRequestCommand>
{
    public async Task Handle(RejectLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leave = await leaveRequestRepository.GetAsync(request.LeaveRequestId, cancellationToken);
        if (leave != null)
            leave.Reject();
    }
}
