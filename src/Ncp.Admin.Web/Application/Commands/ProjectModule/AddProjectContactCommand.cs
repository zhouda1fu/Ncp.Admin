using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectModule;

public record AddProjectContactCommand(
    ProjectId ProjectId,
    CustomerContactId? CustomerContactId,
    string Name,
    string Position,
    string Mobile,
    string OfficePhone,
    string QQ,
    string Wechat,
    string Email,
    bool IsPrimary,
    string Remark) : ICommand<ProjectContactId>;

public class AddProjectContactCommandValidator : AbstractValidator<AddProjectContactCommand>
{
    public AddProjectContactCommandValidator()
    {
        RuleFor(c => c.ProjectId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(50);
    }
}

public class AddProjectContactCommandHandler(IProjectRepository repository) : ICommandHandler<AddProjectContactCommand, ProjectContactId>
{
    public async Task<ProjectContactId> Handle(AddProjectContactCommand request, CancellationToken cancellationToken)
    {
        var project = await repository.GetAsync(request.ProjectId, cancellationToken)
            ?? throw new KnownException("未找到项目", ErrorCodes.ProjectNotFound);
        var id = project.AddContact(
            request.CustomerContactId, request.Name, request.Position ?? string.Empty, request.Mobile ?? string.Empty,
            request.OfficePhone ?? string.Empty, request.QQ ?? string.Empty, request.Wechat ?? string.Empty,
            request.Email ?? string.Empty, request.IsPrimary, request.Remark ?? string.Empty);
        return id;
    }
}
