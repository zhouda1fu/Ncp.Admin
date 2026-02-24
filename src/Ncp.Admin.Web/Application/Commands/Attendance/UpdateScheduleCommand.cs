using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Attendance;

/// <summary>
/// 更新排班命令（更新班次时间与名称）
/// </summary>
public record UpdateScheduleCommand(ScheduleId ScheduleId, TimeOnly StartTime, TimeOnly EndTime, string? ShiftName = null) : ICommand;

/// <summary>
/// 更新排班命令验证器
/// </summary>
public class UpdateScheduleCommandValidator : AbstractValidator<UpdateScheduleCommand>
{
    /// <inheritdoc />
    public UpdateScheduleCommandValidator()
    {
        RuleFor(c => c.ScheduleId).NotNull();
        RuleFor(c => c.EndTime).GreaterThan(c => c.StartTime).WithMessage("下班时间须晚于上班时间");
    }
}

/// <summary>
/// 更新排班命令处理器
/// </summary>
public class UpdateScheduleCommandHandler(IScheduleRepository repository) : ICommandHandler<UpdateScheduleCommand>
{
    /// <inheritdoc />
    public async Task Handle(UpdateScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await repository.GetAsync(request.ScheduleId, cancellationToken)
            ?? throw new KnownException("未找到排班", ErrorCodes.ScheduleNotFound);
        schedule.UpdateShift(request.StartTime, request.EndTime, request.ShiftName);
    }
}
