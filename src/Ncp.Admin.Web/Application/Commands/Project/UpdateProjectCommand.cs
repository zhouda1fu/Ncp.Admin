using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Project;

/// <summary>
/// 更新项目命令
/// </summary>
public record UpdateProjectCommand(ProjectId Id, string Name, string? Description) : ICommand<bool>;

/// <summary>
/// 更新项目命令验证器
/// </summary>
public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    /// <inheritdoc />
    public UpdateProjectCommandValidator()
    {
        RuleFor(c => c.Id).NotNull();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
    }
}

/// <summary>
/// 更新项目命令处理器
/// </summary>
public class UpdateProjectCommandHandler(IProjectRepository repository) : ICommandHandler<UpdateProjectCommand, bool>
{
    /// <inheritdoc />
    public async Task<bool> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到项目", ErrorCodes.ProjectNotFound);
        project.Update(request.Name, request.Description);
        return true;
    }
}
