using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ContractTypeOptionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ContractTypeOptions;

public record CreateContractTypeOptionCommand(
    string Name,
    int TypeValue,
    bool OrderSigningCompanyOptionDisplay = false,
    int SortOrder = 0) : ICommand<ContractTypeOptionId>;

public class CreateContractTypeOptionCommandValidator : AbstractValidator<CreateContractTypeOptionCommand>
{
    public CreateContractTypeOptionCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateContractTypeOptionCommandHandler(IContractTypeOptionRepository repository)
    : ICommandHandler<CreateContractTypeOptionCommand, ContractTypeOptionId>
{
    public async Task<ContractTypeOptionId> Handle(CreateContractTypeOptionCommand request, CancellationToken cancellationToken)
    {
        var entity = new ContractTypeOption(
            request.Name,
            request.TypeValue,
            request.OrderSigningCompanyOptionDisplay,
            request.SortOrder);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
