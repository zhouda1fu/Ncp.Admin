using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Domain;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.PositionCommands;

/// <summary>
/// 更新岗位命令
/// </summary>
public record UpdatePositionCommand(PositionId Id, string Name, string Code, string Description, DeptId DeptId, int SortOrder, int Status) : ICommand;

public class UpdatePositionCommandValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionCommandValidator()
    {
        RuleFor(p => p.Name).NotEmpty().WithMessage("岗位名称不能为空");
        RuleFor(p => p.Code).NotEmpty().WithMessage("岗位编码不能为空");
        RuleFor(p => p.Status).InclusiveBetween(0, 1).WithMessage("状态值必须为0或1");
    }
}

/// <summary>
/// 更新岗位命令处理器
/// </summary>
public class UpdatePositionCommandHandler(IPositionRepository positionRepository) : ICommandHandler<UpdatePositionCommand>
{
    public async Task Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
    {
        var position = await positionRepository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException($"未找到岗位，Id = {request.Id}", ErrorCodes.PositionNotFound);

        position.UpdateInfo(request.Name, request.Code, request.Description, request.DeptId, request.SortOrder, request.Status);
    }
}
