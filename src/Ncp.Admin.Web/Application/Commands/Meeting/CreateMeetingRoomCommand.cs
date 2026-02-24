using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Meeting;

/// <summary>
/// 创建会议室命令
/// </summary>
public record CreateMeetingRoomCommand(string Name, string? Location, int Capacity, string? Equipment) : ICommand<MeetingRoomId>;

/// <summary>
/// 创建会议室命令验证器
/// </summary>
public class CreateMeetingRoomCommandValidator : AbstractValidator<CreateMeetingRoomCommand>
{
    /// <inheritdoc />
    public CreateMeetingRoomCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Capacity).GreaterThan(0);
    }
}

/// <summary>
/// 创建会议室命令处理器
/// </summary>
public class CreateMeetingRoomCommandHandler(IMeetingRoomRepository repository) : ICommandHandler<CreateMeetingRoomCommand, MeetingRoomId>
{
    /// <inheritdoc />
    public async Task<MeetingRoomId> Handle(CreateMeetingRoomCommand request, CancellationToken cancellationToken)
    {
        var room = new MeetingRoom(request.Name, request.Location, request.Capacity, request.Equipment);
        await repository.AddAsync(room, cancellationToken);
        return room.Id;
    }
}
