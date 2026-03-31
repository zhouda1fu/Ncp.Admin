using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Domain;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.PositionCommands;

/// <summary>
/// 停用岗位命令
/// </summary>
/// <param name="PositionId">岗位ID</param>
public record DeactivatePositionCommand(PositionId PositionId) : ICommand;

/// <summary>
/// 停用岗位命令验证器
/// </summary>
public class DeactivatePositionCommandValidator : AbstractValidator<DeactivatePositionCommand>
{
    public DeactivatePositionCommandValidator()
    {
        RuleFor(x => x.PositionId).NotEmpty();
    }
}

/// <summary>
/// 停用岗位命令处理器
/// </summary>
public class DeactivatePositionCommandHandler(IPositionRepository positionRepository) : ICommandHandler<DeactivatePositionCommand>
{
    public async Task Handle(DeactivatePositionCommand request, CancellationToken cancellationToken)
    {
        var position = await positionRepository.GetAsync(request.PositionId, cancellationToken)
            ?? throw new KnownException($"未找到岗位，PositionId = {request.PositionId}", ErrorCodes.PositionNotFound);
        position.Deactivate();
    }
}
