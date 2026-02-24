using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Attendance;

/// <summary>
/// 考勤签退命令
/// </summary>
public record CheckOutCommand(AttendanceRecordId AttendanceRecordId) : ICommand;

/// <summary>
/// 考勤签退命令验证器
/// </summary>
public class CheckOutCommandValidator : AbstractValidator<CheckOutCommand>
{
    /// <inheritdoc />
    public CheckOutCommandValidator()
    {
        RuleFor(c => c.AttendanceRecordId).NotNull();
    }
}

/// <summary>
/// 考勤签退命令处理器
/// </summary>
public class CheckOutCommandHandler(IAttendanceRecordRepository repository) : ICommandHandler<CheckOutCommand>
{
    /// <inheritdoc />
    public async Task Handle(CheckOutCommand request, CancellationToken cancellationToken)
    {
        var record = await repository.GetAsync(request.AttendanceRecordId, cancellationToken)
            ?? throw new KnownException("未找到考勤记录", ErrorCodes.AttendanceRecordNotFound);
        record.CheckOut(DateTimeOffset.UtcNow);
    }
}
