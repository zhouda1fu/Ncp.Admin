using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectStatusOption;

public record DeleteProjectStatusOptionCommand(ProjectStatusOptionId Id) : ICommand<bool>;

public class DeleteProjectStatusOptionCommandValidator : AbstractValidator<DeleteProjectStatusOptionCommand>
{
    public DeleteProjectStatusOptionCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

public class DeleteProjectStatusOptionCommandHandler(IProjectStatusOptionRepository repository) : ICommandHandler<DeleteProjectStatusOptionCommand, bool>
{
    public async Task<bool> Handle(DeleteProjectStatusOptionCommand request, CancellationToken cancellationToken)
    {
        await repository.RemoveAsync(request.Id, cancellationToken);
        return true;
    }
}
