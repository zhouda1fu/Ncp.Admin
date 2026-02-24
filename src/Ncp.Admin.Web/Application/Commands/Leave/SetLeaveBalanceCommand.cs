using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.LeaveBalanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Leave;

/// <summary>
/// 设置请假余额（按用户、年度、类型；不存在则创建）
/// </summary>
public record SetLeaveBalanceCommand(UserId UserId, int Year, LeaveType LeaveType, decimal TotalDays) : ICommand<LeaveBalanceId>;

public class SetLeaveBalanceCommandValidator : AbstractValidator<SetLeaveBalanceCommand>
{
    public SetLeaveBalanceCommandValidator()
    {
        RuleFor(c => c.UserId).NotNull().WithMessage("用户不能为空");
        RuleFor(c => c.Year).InclusiveBetween(2000, 2100).WithMessage("年度无效");
        RuleFor(c => c.LeaveType).IsInEnum().WithMessage("无效的请假类型");
        RuleFor(c => c.TotalDays).GreaterThanOrEqualTo(0).WithMessage("总天数不能为负");
    }
}

public class SetLeaveBalanceCommandHandler(ILeaveBalanceRepository repository)
    : ICommandHandler<SetLeaveBalanceCommand, LeaveBalanceId>
{
    public async Task<LeaveBalanceId> Handle(SetLeaveBalanceCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByUserYearTypeAsync(request.UserId, request.Year, request.LeaveType, cancellationToken);
        if (existing != null)
        {
            existing.SetTotalDays(request.TotalDays);
            return existing.Id;
        }

        var entity = new LeaveBalance(request.UserId, request.Year, request.LeaveType, request.TotalDays);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
