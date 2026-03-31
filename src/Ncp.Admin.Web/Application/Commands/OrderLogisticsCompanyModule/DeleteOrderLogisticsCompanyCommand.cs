using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.OrderLogisticsCompanyModule;

public record DeleteOrderLogisticsCompanyCommand(OrderLogisticsCompanyId Id) : ICommand<bool>;

public class DeleteOrderLogisticsCompanyCommandHandler(IOrderLogisticsCompanyRepository repository)
    : ICommandHandler<DeleteOrderLogisticsCompanyCommand, bool>
{
    public async Task<bool> Handle(DeleteOrderLogisticsCompanyCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到物流公司", ErrorCodes.OrderNotFound);
        await repository.DeleteAsync(entity);
        return true;
    }
}
