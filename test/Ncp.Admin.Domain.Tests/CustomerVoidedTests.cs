using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.Tests;

/// <summary>
/// 客户作废后的不变式：禁止领用、公海更新等变更。
/// </summary>
public class CustomerVoidedTests
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
    public void Void_WhenAlreadyVoided_ShouldBeIdempotent()
    {
        var customer = CreateSeaCustomer();
        customer.VoidUnclaimedSeaCustomer();
        customer.VoidUnclaimedSeaCustomer();
        Assert.True(customer.IsVoided);
    }

    [Fact]
    public void ClaimFromSea_WhenVoided_ShouldThrow()
    {
        var customer = CreateSeaCustomer();
        customer.VoidUnclaimedSeaCustomer();

        var ex = Assert.Throws<KnownException>(() => customer.ClaimFromSea(new UserId(2), "李四"));

        Assert.Contains("作废", ex.Message);
    }

    [Fact]
    public void UpdateWhenInSea_WhenVoided_ShouldThrow()
    {
        var customer = CreateSeaCustomer();
        customer.VoidUnclaimedSeaCustomer();

        var ex = Assert.Throws<KnownException>(() => customer.UpdateWhenInSea(
            new CustomerSourceId(Guid.NewGuid()),
            "新来源",
            shortName: string.Empty,
            status: null,
            nature: null,
            provinceCode: string.Empty,
            cityCode: string.Empty,
            districtCode: string.Empty,
            provinceName: string.Empty,
            cityName: string.Empty,
            districtName: string.Empty,
            phoneProvinceCode: string.Empty,
            phoneCityCode: string.Empty,
            phoneDistrictCode: string.Empty,
            phoneProvinceName: string.Empty,
            phoneCityName: string.Empty,
            phoneDistrictName: string.Empty,
            consultationContent: string.Empty,
            coverRegion: string.Empty,
            registerAddress: string.Empty,
            employeeCount: 0,
            businessLicense: string.Empty,
            mainContactName: string.Empty,
            mainContactPhone: string.Empty,
            contactQq: string.Empty,
            contactWechat: string.Empty,
            wechatStatus: string.Empty,
            remark: string.Empty,
            isKeyAccount: false,
            industryIds: null));

        Assert.Contains("作废", ex.Message);
    }
}
