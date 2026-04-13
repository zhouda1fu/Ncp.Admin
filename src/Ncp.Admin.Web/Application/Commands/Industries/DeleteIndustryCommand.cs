using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Industries;

public record DeleteIndustryCommand(IndustryId Id) : ICommand<bool>;

public class DeleteIndustryCommandValidator : AbstractValidator<DeleteIndustryCommand>
{
    public DeleteIndustryCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

public class DeleteIndustryCommandHandler(IIndustryRepository repository)
    : ICommandHandler<DeleteIndustryCommand, bool>
{
    public async Task<bool> Handle(DeleteIndustryCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到行业", ErrorCodes.IndustryNotFound);
        await repository.RemoveAsync(request.Id, cancellationToken);
        return true;
    }
}
