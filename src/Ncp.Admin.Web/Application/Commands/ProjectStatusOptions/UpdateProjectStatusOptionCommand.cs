using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectStatusOptions;

public record UpdateProjectStatusOptionCommand(ProjectStatusOptionId Id, string Name, string Code, int SortOrder) : ICommand<bool>;

public class UpdateProjectStatusOptionCommandValidator : AbstractValidator<UpdateProjectStatusOptionCommand>
{
    public UpdateProjectStatusOptionCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Code).MaximumLength(50);
    }
}

public class UpdateProjectStatusOptionCommandHandler(IProjectStatusOptionRepository repository) : ICommandHandler<UpdateProjectStatusOptionCommand, bool>
{
    public async Task<bool> Handle(UpdateProjectStatusOptionCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到项目状态选项", ErrorCodes.ProjectStatusOptionNotFound);
        entity.Update(request.Name, request.Code ?? string.Empty, request.SortOrder);
        return true;
    }
}
