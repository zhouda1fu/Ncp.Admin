using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Leave;

/// <summary>
/// 更新请假申请（仅草稿）
/// </summary>
public record UpdateLeaveRequestCommand(
    LeaveRequestId Id,
    LeaveType LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Days,
    string Reason) : ICommand;

public class UpdateLeaveRequestCommandValidator : AbstractValidator<UpdateLeaveRequestCommand>
{
    public UpdateLeaveRequestCommandValidator()
    {
        RuleFor(c => c.Id).NotNull().WithMessage("请假申请ID不能为空");
        RuleFor(c => c.LeaveType).IsInEnum().WithMessage("无效的请假类型");
        RuleFor(c => c.EndDate).GreaterThanOrEqualTo(c => c.StartDate).WithMessage("结束日期不能早于开始日期");
        RuleFor(c => c.Days).GreaterThan(0).WithMessage("请假天数必须大于0");
        RuleFor(c => c.Reason).MaximumLength(500);
    }
}

public class UpdateLeaveRequestCommandHandler(ILeaveRequestRepository repository)
    : ICommandHandler<UpdateLeaveRequestCommand>
{
    public async Task Handle(UpdateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到请假申请", ErrorCodes.LeaveRequestNotFound);
        entity.UpdateDraft(request.LeaveType, request.StartDate, request.EndDate, request.Days, request.Reason ?? string.Empty);
    }
}
