using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.OrderInvoiceTypeOptions;

public record DeleteOrderInvoiceTypeOptionCommand(OrderInvoiceTypeOptionId Id) : ICommand<bool>;

public class DeleteOrderInvoiceTypeOptionCommandHandler(IOrderInvoiceTypeOptionRepository repository)
    : ICommandHandler<DeleteOrderInvoiceTypeOptionCommand, bool>
{
    public async Task<bool> Handle(DeleteOrderInvoiceTypeOptionCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到订单发票类型选项", ErrorCodes.OrderInvoiceTypeOptionNotFound);
        await repository.DeleteAsync(entity);
        return true;
    }
}
