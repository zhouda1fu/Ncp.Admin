using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Domain;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.PositionCommands;

/// <summary>
/// 删除岗位命令
/// </summary>
public record DeletePositionCommand(PositionId Id) : ICommand;

/// <summary>
/// 删除岗位命令处理器
/// </summary>
public class DeletePositionCommandHandler(IPositionRepository positionRepository) : ICommandHandler<DeletePositionCommand>
{
    public async Task Handle(DeletePositionCommand request, CancellationToken cancellationToken)
    {
        var position = await positionRepository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException($"未找到岗位，Id = {request.Id}", ErrorCodes.PositionNotFound);

        position.SoftDelete();
    }
}
