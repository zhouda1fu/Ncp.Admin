using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Contract;

public record UpdateContractCommand(
    ContractId Id,
    string Code,
    string Title,
    string PartyA,
    string PartyB,
    decimal Amount,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string? FileStorageKey = null) : ICommand<bool>;

public class UpdateContractCommandValidator : AbstractValidator<UpdateContractCommand>
{
    public UpdateContractCommandValidator()
    {
        RuleFor(c => c.Code).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.PartyA).NotEmpty().MaximumLength(200);
        RuleFor(c => c.PartyB).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Amount).GreaterThanOrEqualTo(0);
        RuleFor(c => c.EndDate).GreaterThanOrEqualTo(c => c.StartDate);
    }
}

public class UpdateContractCommandHandler(IContractRepository repository) : ICommandHandler<UpdateContractCommand, bool>
{
    public async Task<bool> Handle(UpdateContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到合同", ErrorCodes.ContractNotFound);
        contract.Update(request.Code, request.Title, request.PartyA, request.PartyB,
            request.Amount, request.StartDate, request.EndDate, request.FileStorageKey);
        return true;
    }
}
