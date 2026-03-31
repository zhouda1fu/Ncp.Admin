using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.IndustryModule;

public record CreateIndustryCommand(string Name, IndustryId? ParentId, int SortOrder = 0, string? Remark = null)
    : ICommand<IndustryId>;

public class CreateIndustryCommandValidator : AbstractValidator<CreateIndustryCommand>
{
    public CreateIndustryCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
    }
}

public class CreateIndustryCommandHandler(IIndustryRepository repository)
    : ICommandHandler<CreateIndustryCommand, IndustryId>
{
    public async Task<IndustryId> Handle(CreateIndustryCommand request, CancellationToken cancellationToken)
    {
        var entity = new Industry(request.Name, request.ParentId, request.SortOrder, request.Remark);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
