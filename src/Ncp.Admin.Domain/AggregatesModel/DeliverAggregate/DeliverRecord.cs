using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.DeliverAggregate;

public partial record DeliverRecordId : IGuidStronglyTypedId;

public class DeliverRecord : Entity<DeliverRecordId>, IAggregateRoot
{
    protected DeliverRecord() { }


    public DeliverRecord(OrderId orderId)
    {
        this.OrderId = orderId;
    }

    public OrderId OrderId { get; private set; } = default!;
}

