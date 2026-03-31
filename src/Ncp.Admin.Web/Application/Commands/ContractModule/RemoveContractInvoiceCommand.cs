using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ContractModule;

/// <summary>
/// 删除合同发票命令
/// </summary>
public record RemoveContractInvoiceCommand(ContractId ContractId, ContractInvoiceId InvoiceId) : ICommand<bool>;

public class RemoveContractInvoiceCommandHandler(IContractRepository repository)
    : ICommandHandler<RemoveContractInvoiceCommand, bool>
{
    public async Task<bool> Handle(RemoveContractInvoiceCommand request, CancellationToken cancellationToken)
    {
        var contract = await repository.GetAsync(request.ContractId, cancellationToken)
            ?? throw new KnownException("未找到合同", ErrorCodes.ContractNotFound);
        contract.RemoveInvoice(request.InvoiceId);
        await repository.UpdateAsync(contract, cancellationToken);
        return true;
    }
}
