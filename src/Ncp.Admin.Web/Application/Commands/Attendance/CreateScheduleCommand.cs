using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Attendance;

/// <summary>
/// 创建排班命令
/// </summary>
public record CreateScheduleCommand(UserId UserId, DateOnly WorkDate, TimeOnly StartTime, TimeOnly EndTime, string? ShiftName = null) : ICommand<ScheduleId>;

/// <summary>
/// 创建排班命令验证器
/// </summary>
public class CreateScheduleCommandValidator : AbstractValidator<CreateScheduleCommand>
{
    /// <inheritdoc />
    public CreateScheduleCommandValidator()
    {
        RuleFor(c => c.UserId).NotNull();
        RuleFor(c => c.EndTime).GreaterThan(c => c.StartTime).WithMessage("下班时间须晚于上班时间");
    }
}

/// <summary>
/// 创建排班命令处理器
/// </summary>
public class CreateScheduleCommandHandler(IScheduleRepository repository) : ICommandHandler<CreateScheduleCommand, ScheduleId>
{
    /// <inheritdoc />
    public async Task<ScheduleId> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = new Schedule(request.UserId, request.WorkDate, request.StartTime, request.EndTime, request.ShiftName);
        await repository.AddAsync(schedule, cancellationToken);
        return schedule.Id;
    }
}
