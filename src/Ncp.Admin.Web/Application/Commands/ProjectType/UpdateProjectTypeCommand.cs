using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectType;

public record UpdateProjectTypeCommand(ProjectTypeId Id, string Name, int SortOrder) : ICommand<bool>;

public class UpdateProjectTypeCommandValidator : AbstractValidator<UpdateProjectTypeCommand>
{
    public UpdateProjectTypeCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateProjectTypeCommandHandler(IProjectTypeRepository repository) : ICommandHandler<UpdateProjectTypeCommand, bool>
{
    public async Task<bool> Handle(UpdateProjectTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到项目类型", ErrorCodes.ProjectTypeNotFound);
        entity.Update(request.Name, request.SortOrder);
        return true;
    }
}
