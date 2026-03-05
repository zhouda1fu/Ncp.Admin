using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Contract;

public record DeleteContractCommand(ContractId Id) : ICommand<bool>;

public class DeleteContractCommandHandler(IContractRepository repository) : ICommandHandler<DeleteContractCommand, bool>
{
    public async Task<bool> Handle(DeleteContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到合同", ErrorCodes.ContractNotFound);
        contract.MarkDeleted();
        return true;
    }
}
