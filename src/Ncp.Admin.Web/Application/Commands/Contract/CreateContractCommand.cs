using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using ContractEntity = Ncp.Admin.Domain.AggregatesModel.ContractAggregate.Contract;

namespace Ncp.Admin.Web.Application.Commands.Contract;

public record CreateContractCommand(
    string Code,
    string Title,
    string PartyA,
    string PartyB,
    decimal Amount,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    UserId CreatorId,
    string? FileStorageKey = null) : ICommand<ContractId>;

public class CreateContractCommandValidator : AbstractValidator<CreateContractCommand>
{
    public CreateContractCommandValidator()
    {
        RuleFor(c => c.Code).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.PartyA).NotEmpty().MaximumLength(200);
        RuleFor(c => c.PartyB).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Amount).GreaterThanOrEqualTo(0);
        RuleFor(c => c.EndDate).GreaterThanOrEqualTo(c => c.StartDate);
    }
}

public class CreateContractCommandHandler(IContractRepository repository) : ICommandHandler<CreateContractCommand, ContractId>
{
    public async Task<ContractId> Handle(CreateContractCommand request, CancellationToken cancellationToken)
    {
        var contract = new ContractEntity(
            request.Code, request.Title, request.PartyA, request.PartyB,
            request.Amount, request.StartDate, request.EndDate, request.CreatorId, request.FileStorageKey);
        await repository.AddAsync(contract, cancellationToken);
        return contract.Id;
    }
}
