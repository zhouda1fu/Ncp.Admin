using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.Tests;

/// <summary>
/// 客户共享：差量同步行为（保留仍在名单中的共享元数据）。
/// </summary>
public class CustomerShareTests
{
    private static Customer CreateSeaCustomer()
    {
        return new Customer(
            new CustomerSourceId(Guid.NewGuid()),
            "测试来源",
            "联系人",
            "13800138000",
            string.Empty,
            string.Empty,
            "1", "2", "3",
            "电话省", "电话市", "电话区",
            "10", "20", "30",
            "注册省", "注册市", "注册区",
            "咨询内容",
            new UserId(1),
            "创建人");
    }

    [Fact]
    public void ShareToUsers_WhenNarrowingList_ShouldPreserveSharedAtAndSharedByForKeptUsers()
    {
        var customer = CreateSeaCustomer();
        var sharer1 = new UserId(100);
        var u1 = new UserId(1);
        var u2 = new UserId(2);
        customer.ShareToUsers(sharer1, new[] { u1, u2 });
        var u2At = customer.Shares.Single(s => s.SharedToUserId == u2).SharedAt;
        var u2By = customer.Shares.Single(s => s.SharedToUserId == u2).SharedByUserId;

        customer.ShareToUsers(new UserId(200), new[] { u2 });

        Assert.Single(customer.Shares);
        var kept = customer.Shares.Single();
        Assert.Equal(u2, kept.SharedToUserId);
        Assert.Equal(u2At, kept.SharedAt);
        Assert.Equal(u2By, kept.SharedByUserId);
    }

    [Fact]
    public void ShareToUsers_WhenExtendingList_ShouldKeepOriginalMetadataForExistingShares()
    {
        var customer = CreateSeaCustomer();
        var firstSharer = new UserId(10);
        var u1 = new UserId(1);
        var u2 = new UserId(2);
        customer.ShareToUsers(firstSharer, new[] { u1 });
        var u1At = customer.Shares.Single().SharedAt;

        customer.ShareToUsers(new UserId(20), new[] { u1, u2 });

        Assert.Equal(2, customer.Shares.Count);
        var row1 = customer.Shares.Single(s => s.SharedToUserId == u1);
        Assert.Equal(firstSharer, row1.SharedByUserId);
        Assert.Equal(u1At, row1.SharedAt);
        Assert.Equal(new UserId(20), customer.Shares.Single(s => s.SharedToUserId == u2).SharedByUserId);
    }
}
