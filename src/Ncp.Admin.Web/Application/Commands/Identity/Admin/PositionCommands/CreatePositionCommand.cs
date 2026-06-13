using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.PositionCommands;

/// <summary>
/// 创建岗位命令
/// </summary>
public record CreatePositionCommand(string Name, string? Code, string Description, DeptId DeptId, int SortOrder, int Status)
    : ICommand<CreatePositionCommandResult>;

public record CreatePositionCommandResult(PositionId Id, string Code);

public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator(PositionQuery positionQuery)
    {
        RuleFor(p => p.Name).NotEmpty().WithMessage("岗位名称不能为空");
        RuleFor(p => p.Code)
            .MaximumLength(50)
            .WithMessage("岗位编码不能超过50个字符")
            .When(p => !string.IsNullOrWhiteSpace(p.Code));
        RuleFor(p => p.Code!)
            .MustAsync(async (code, ct) => !await positionQuery.DoesPositionCodeExist(code.Trim(), ct))
            .WithMessage(p => $"该岗位编码已存在，Code={p.Code}")
            .When(p => !string.IsNullOrWhiteSpace(p.Code));
        RuleFor(p => p.Status).InclusiveBetween(0, 1).WithMessage("状态值必须为0或1");
    }
}

/// <summary>
/// 创建岗位命令处理器
/// </summary>
public class CreatePositionCommandHandler(IPositionRepository positionRepository, PositionQuery positionQuery)
    : ICommandHandler<CreatePositionCommand, CreatePositionCommandResult>
{
    public async Task<CreatePositionCommandResult> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
    {
        var code = await ResolvePositionCodeAsync(request.Code, cancellationToken);
        var position = new Position(request.Name, code, request.Description, request.DeptId, request.SortOrder, request.Status);
        await positionRepository.AddAsync(position, cancellationToken);
        return new CreatePositionCommandResult(position.Id, code);
    }

    private async Task<string> ResolvePositionCodeAsync(string? requestedCode, CancellationToken cancellationToken)
    {
        var normalized = (requestedCode ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(normalized))
            return normalized;

        for (var i = 0; i < 5; i++)
        {
            var generated = $"POS-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
            if (!await positionQuery.DoesPositionCodeExist(generated, cancellationToken))
                return generated;
        }

        return $"POS-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
    }
}
