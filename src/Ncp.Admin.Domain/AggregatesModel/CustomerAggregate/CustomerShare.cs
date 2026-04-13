using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;

/// <summary>
/// 客户共享关系（子实体）：挂入 <see cref="Customer.Shares"/>，<see cref="CustomerId"/> 由 EF 根据父子关系修复。
/// </summary>
public class CustomerShare
{
    private CustomerShare()
    {
    }

    public CustomerShare(UserId sharedToUserId, UserId sharedByUserId, DateTimeOffset sharedAt)
    {
        SharedToUserId = sharedToUserId;
        SharedByUserId = sharedByUserId;
        SharedAt = sharedAt.ToUniversalTime();
    }

    public CustomerId CustomerId { get; private set; } = default!;
    public UserId SharedToUserId { get; private set; } = default!;
    public UserId SharedByUserId { get; private set; } = default!;
    public DateTimeOffset SharedAt { get; private set; }
}

