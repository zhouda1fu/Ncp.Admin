using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ProjectModule;

public record UpdateProjectContactCommand(
    ProjectId ProjectId,
    ProjectContactId ContactId,
    CustomerContactId? CustomerContactId,
    string Name,
    string Position,
    string Mobile,
    string OfficePhone,
    string QQ,
    string Wechat,
    string Email,
    bool IsPrimary,
    string Remark) : ICommand<bool>;

public class UpdateProjectContactCommandValidator : AbstractValidator<UpdateProjectContactCommand>
{
    public UpdateProjectContactCommandValidator()
    {
        RuleFor(c => c.ProjectId).NotEmpty();
        RuleFor(c => c.ContactId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(50);
    }
}

public class UpdateProjectContactCommandHandler(IProjectRepository repository) : ICommandHandler<UpdateProjectContactCommand, bool>
{
    public async Task<bool> Handle(UpdateProjectContactCommand request, CancellationToken cancellationToken)
    {
        var project = await repository.GetAsync(request.ProjectId, cancellationToken)
            ?? throw new KnownException("未找到项目", ErrorCodes.ProjectNotFound);
        project.UpdateContact(
            request.ContactId, request.CustomerContactId, request.Name, request.Position ?? string.Empty,
            request.Mobile ?? string.Empty, request.OfficePhone ?? string.Empty, request.QQ ?? string.Empty,
            request.Wechat ?? string.Empty, request.Email ?? string.Empty, request.IsPrimary, request.Remark ?? string.Empty);
        return true;
    }
}
