using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectType;

public record DeleteProjectTypeCommand(ProjectTypeId Id) : ICommand<bool>;

public class DeleteProjectTypeCommandValidator : AbstractValidator<DeleteProjectTypeCommand>
{
    public DeleteProjectTypeCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

public class DeleteProjectTypeCommandHandler(IProjectTypeRepository repository) : ICommandHandler<DeleteProjectTypeCommand, bool>
{
    public async Task<bool> Handle(DeleteProjectTypeCommand request, CancellationToken cancellationToken)
    {
        await repository.RemoveAsync(request.Id, cancellationToken);
        return true;
    }
}
