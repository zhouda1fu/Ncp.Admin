using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;

/// <summary>
/// 客户共享关系（子实体）：表示某客户共享给某用户
/// </summary>
public class CustomerShare
{
    private CustomerShare()
    {
    }

    public CustomerShare(CustomerId customerId, UserId sharedToUserId, UserId sharedByUserId, DateTimeOffset sharedAt)
    {
        CustomerId = customerId;
        SharedToUserId = sharedToUserId;
        SharedByUserId = sharedByUserId;
        SharedAt = sharedAt.ToUniversalTime();
    }

    public CustomerId CustomerId { get; private set; } = default!;
    public UserId SharedToUserId { get; private set; } = default!;
    public UserId SharedByUserId { get; private set; } = default!;
    public DateTimeOffset SharedAt { get; private set; }
}

