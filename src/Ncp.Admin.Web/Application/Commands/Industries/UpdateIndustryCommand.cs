using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Industries;

public record UpdateIndustryCommand(IndustryId Id, string Name, IndustryId? ParentId, int SortOrder, string? Remark = null) : ICommand<bool>;

public class UpdateIndustryCommandValidator : AbstractValidator<UpdateIndustryCommand>
{
    public UpdateIndustryCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
    }
}

public class UpdateIndustryCommandHandler(IIndustryRepository repository)
    : ICommandHandler<UpdateIndustryCommand, bool>
{
    public async Task<bool> Handle(UpdateIndustryCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到行业", ErrorCodes.IndustryNotFound);
        var parentId = request.ParentId ?? Industry.RootParentId;
        entity.Update(request.Name, parentId, request.SortOrder, request.Remark);
        return true;
    }
}
