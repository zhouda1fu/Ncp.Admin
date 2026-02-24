using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Meeting;

/// <summary>
/// 创建会议室预订命令（会校验时段冲突）
/// </summary>
public record CreateMeetingBookingCommand(MeetingRoomId MeetingRoomId, UserId BookerId, string Title, DateTimeOffset StartAt, DateTimeOffset EndAt) : ICommand<MeetingBookingId>;

/// <summary>
/// 创建会议室预订命令验证器
/// </summary>
public class CreateMeetingBookingCommandValidator : AbstractValidator<CreateMeetingBookingCommand>
{
    /// <inheritdoc />
    public CreateMeetingBookingCommandValidator()
    {
        RuleFor(c => c.MeetingRoomId).NotNull();
        RuleFor(c => c.BookerId).NotNull();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.EndAt).GreaterThan(c => c.StartAt);
    }
}

/// <summary>
/// 创建会议室预订命令处理器（校验会议室存在且时段无冲突）
/// </summary>
public class CreateMeetingBookingCommandHandler(IMeetingBookingRepository bookingRepository, IMeetingRoomRepository roomRepository)
    : ICommandHandler<CreateMeetingBookingCommand, MeetingBookingId>
{
    /// <inheritdoc />
    public async Task<MeetingBookingId> Handle(CreateMeetingBookingCommand request, CancellationToken cancellationToken)
    {
        _ = await roomRepository.GetAsync(request.MeetingRoomId, cancellationToken)
            ?? throw new KnownException("未找到会议室", ErrorCodes.MeetingRoomNotFound);
        var hasConflict = await bookingRepository.HasConflictAsync(request.MeetingRoomId, request.StartAt, request.EndAt, null, cancellationToken);
        if (hasConflict)
            throw new KnownException("该时段已被预订", ErrorCodes.MeetingRoomConflict);
        var booking = new MeetingBooking(request.MeetingRoomId, request.BookerId, request.Title, request.StartAt, request.EndAt);
        await bookingRepository.AddAsync(booking, cancellationToken);
        return booking.Id;
    }
}
