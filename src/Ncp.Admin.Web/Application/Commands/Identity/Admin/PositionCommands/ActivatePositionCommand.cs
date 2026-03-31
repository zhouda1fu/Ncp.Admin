using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Domain;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.PositionCommands;

/// <summary>
/// 激活岗位命令
/// </summary>
/// <param name="PositionId">岗位ID</param>
public record ActivatePositionCommand(PositionId PositionId) : ICommand;

/// <summary>
/// 激活岗位命令验证器
/// </summary>
public class ActivatePositionCommandValidator : AbstractValidator<ActivatePositionCommand>
{
    public ActivatePositionCommandValidator()
    {
        RuleFor(x => x.PositionId).NotEmpty();
    }
}

/// <summary>
/// 激活岗位命令处理器
/// </summary>
public class ActivatePositionCommandHandler(IPositionRepository positionRepository) : ICommandHandler<ActivatePositionCommand>
{
    public async Task Handle(ActivatePositionCommand request, CancellationToken cancellationToken)
    {
        var position = await positionRepository.GetAsync(request.PositionId, cancellationToken)
            ?? throw new KnownException($"未找到岗位，PositionId = {request.PositionId}", ErrorCodes.PositionNotFound);
        position.Activate();
    }
}
