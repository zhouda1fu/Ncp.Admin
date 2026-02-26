using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Contract;

public record SubmitContractCommand(ContractId Id) : ICommand<bool>;

public class SubmitContractCommandHandler(IContractRepository repository) : ICommandHandler<SubmitContractCommand, bool>
{
    public async Task<bool> Handle(SubmitContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到合同", ErrorCodes.ContractNotFound);
        contract.SubmitForApproval();
        return true;
    }
}
