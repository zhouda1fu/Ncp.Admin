using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Project;

/// <summary>
/// 创建项目命令
/// </summary>
public record CreateProjectCommand(UserId CreatorId, string Name, string? Description) : ICommand<ProjectId>;

/// <summary>
/// 创建项目命令验证器
/// </summary>
public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    /// <inheritdoc />
    public CreateProjectCommandValidator()
    {
        RuleFor(c => c.CreatorId).NotNull();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
    }
}

/// <summary>
/// 创建项目命令处理器
/// </summary>
public class CreateProjectCommandHandler(IProjectRepository repository) : ICommandHandler<CreateProjectCommand, ProjectId>
{
    /// <inheritdoc />
    public async Task<ProjectId> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = new Ncp.Admin.Domain.AggregatesModel.ProjectAggregate.Project(request.Name, request.CreatorId, request.Description);
        await repository.AddAsync(project, cancellationToken);
        return project.Id;
    }
}
