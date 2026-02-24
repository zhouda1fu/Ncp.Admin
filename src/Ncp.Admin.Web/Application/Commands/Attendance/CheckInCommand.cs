using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Attendance;

/// <summary>
/// 考勤签到命令（当日未签到则创建记录，重复签到抛异常）
/// </summary>
public record CheckInCommand(UserId UserId, AttendanceSource Source, string? Location = null) : ICommand<AttendanceRecordId>;

/// <summary>
/// 考勤签到命令验证器
/// </summary>
public class CheckInCommandValidator : AbstractValidator<CheckInCommand>
{
    /// <inheritdoc />
    public CheckInCommandValidator()
    {
        RuleFor(c => c.UserId).NotNull();
        RuleFor(c => c.Source).IsInEnum();
    }
}

/// <summary>
/// 考勤签到命令处理器
/// </summary>
public class CheckInCommandHandler(IAttendanceRecordRepository repository) : ICommandHandler<CheckInCommand, AttendanceRecordId>
{
    /// <inheritdoc />
    public async Task<AttendanceRecordId> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        var existing = await repository.GetTodayByUserAsync(request.UserId, today, cancellationToken);
        if (existing != null)
            throw new KnownException("今日已签到，请勿重复打卡", ErrorCodes.AttendanceAlreadyCheckedIn);
        var record = new AttendanceRecord(request.UserId, DateTimeOffset.UtcNow, request.Source, request.Location);
        await repository.AddAsync(record, cancellationToken);
        return record.Id;
    }
}
