using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Contract;

public record ArchiveContractCommand(ContractId Id) : ICommand<bool>;

public class ArchiveContractCommandHandler(IContractRepository repository) : ICommandHandler<ArchiveContractCommand, bool>
{
    public async Task<bool> Handle(ArchiveContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到合同", ErrorCodes.ContractNotFound);
        contract.Archive();
        return true;
    }
}
