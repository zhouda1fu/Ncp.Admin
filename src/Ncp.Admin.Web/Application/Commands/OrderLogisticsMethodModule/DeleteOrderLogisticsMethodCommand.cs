using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.OrderLogisticsMethodModule;

public record DeleteOrderLogisticsMethodCommand(OrderLogisticsMethodId Id) : ICommand<bool>;

public class DeleteOrderLogisticsMethodCommandHandler(IOrderLogisticsMethodRepository repository)
    : ICommandHandler<DeleteOrderLogisticsMethodCommand, bool>
{
    public async Task<bool> Handle(DeleteOrderLogisticsMethodCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到物流方式", ErrorCodes.OrderNotFound);
        await repository.DeleteAsync(entity);
        return true;
    }
}
