using Ncp.Admin.Domain.AggregatesModel.DeliverAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using NetCorePal.Extensions.Primitives;

namespace Ncp.Admin.Web.Application.Commands.Delivers;

public record DeliverGoodsCommand(OrderId OrderId) : ICommand<DeliverRecordId>;

public class DeliverGoodsCommandHandler(IDeliverRecordRepository deliverRecordRepository)
    : ICommandHandler<DeliverGoodsCommand, DeliverRecordId>
{
    public Task<DeliverRecordId> Handle(DeliverGoodsCommand request, CancellationToken cancellationToken)
    {
        var record = new DeliverRecord(request.OrderId);
        deliverRecordRepository.Add(record);
        return Task.FromResult(record.Id);
    }
}