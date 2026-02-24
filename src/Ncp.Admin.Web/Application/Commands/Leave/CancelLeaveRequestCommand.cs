using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Leave;

/// <summary>
/// 撤销请假申请
/// </summary>
public record CancelLeaveRequestCommand(LeaveRequestId Id) : ICommand;

public class CancelLeaveRequestCommandValidator : AbstractValidator<CancelLeaveRequestCommand>
{
    public CancelLeaveRequestCommandValidator()
    {
        RuleFor(c => c.Id).NotNull().WithMessage("请假申请ID不能为空");
    }
}

public class CancelLeaveRequestCommandHandler(ILeaveRequestRepository repository)
    : ICommandHandler<CancelLeaveRequestCommand>
{
    public async Task Handle(CancelLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到请假申请", ErrorCodes.LeaveRequestNotFound);
        entity.Cancel();
    }
}
