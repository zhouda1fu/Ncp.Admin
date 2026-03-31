using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.ContractModule;

public record ApproveContractCommand(ContractId Id, UserId ApprovedBy) : ICommand<bool>;

public class ApproveContractCommandHandler(IContractRepository repository) : ICommandHandler<ApproveContractCommand, bool>
{
    public async Task<bool> Handle(ApproveContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到合同", ErrorCodes.ContractNotFound);
        contract.Approve(request.ApprovedBy);
        return true;
    }
}
