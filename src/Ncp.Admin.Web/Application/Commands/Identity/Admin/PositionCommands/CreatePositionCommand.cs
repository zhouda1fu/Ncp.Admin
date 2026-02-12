using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.PositionCommands;

/// <summary>
/// 创建岗位命令
/// </summary>
public record CreatePositionCommand(string Name, string Code, string Description, DeptId DeptId, int SortOrder, int Status) : ICommand<PositionId>;

public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator(PositionQuery positionQuery)
    {
        RuleFor(p => p.Name).NotEmpty().WithMessage("岗位名称不能为空");
        RuleFor(p => p.Code).NotEmpty().WithMessage("岗位编码不能为空");
        RuleFor(p => p.Code).MustAsync(async (code, ct) => !await positionQuery.DoesPositionCodeExist(code, ct))
            .WithMessage(p => $"该岗位编码已存在，Code={p.Code}");
        RuleFor(p => p.Status).InclusiveBetween(0, 1).WithMessage("状态值必须为0或1");
    }
}

/// <summary>
/// 创建岗位命令处理器
/// </summary>
public class CreatePositionCommandHandler(IPositionRepository positionRepository) : ICommandHandler<CreatePositionCommand, PositionId>
{
    public async Task<PositionId> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
    {
        var position = new Position(request.Name, request.Code, request.Description, request.DeptId, request.SortOrder, request.Status);
        await positionRepository.AddAsync(position, cancellationToken);
        return position.Id;
    }
}
