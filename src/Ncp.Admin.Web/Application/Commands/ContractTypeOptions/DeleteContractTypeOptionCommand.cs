using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ContractTypeOptionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ContractTypeOptions;

public record DeleteContractTypeOptionCommand(ContractTypeOptionId Id) : ICommand<bool>;

public class DeleteContractTypeOptionCommandValidator : AbstractValidator<DeleteContractTypeOptionCommand>
{
    public DeleteContractTypeOptionCommandValidator()
    {
        RuleFor(c => c.Id).NotNull();
    }
}

public class DeleteContractTypeOptionCommandHandler(IContractTypeOptionRepository repository)
    : ICommandHandler<DeleteContractTypeOptionCommand, bool>
{
    public async Task<bool> Handle(DeleteContractTypeOptionCommand request, CancellationToken cancellationToken)
    {
        await repository.RemoveAsync(request.Id, cancellationToken);
        return true;
    }
}
