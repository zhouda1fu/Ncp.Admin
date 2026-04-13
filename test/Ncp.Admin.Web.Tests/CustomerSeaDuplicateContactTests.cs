using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Web.Endpoints.Customers;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Tests;

[Collection(WebAppTestCollection.Name)]
public class CustomerSeaDuplicateContactTests(WebAppFixture app) : AuthenticatedTestBase<WebAppFixture>(app)
{
    private const string TestNamePrefix = "UT_SeaDup_";

    private static string NewToken() => $"{TestNamePrefix}{Guid.NewGuid():N}";

    private async Task<CustomerSourceId> GetAnySeaCustomerSourceIdAsync()
    {
        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var source = await dbContext.CustomerSources
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(TestContext.Current.CancellationToken);

        return source?.Id ?? throw new InvalidOperationException("未找到客户来源数据");
    }

    private async Task<CustomerId> CreateSeaCustomerAsync(
        HttpClient client,
        CustomerSourceId sourceId,
        string mainContactName,
        string mainContactPhone,
        string contactQq,
        string contactWechat)
    {
        var request = new CreateSeaCustomerRequest(
            sourceId,
            "测试来源",
            mainContactName,
            mainContactPhone,
            contactQq,
            contactWechat,
            "110000",
            "110100",
            "110101",
            "北京市",
            "北京市",
            "东城区",
            "110000",
            "110100",
            "110101",
            "北京市",
            "北京市",
            "东城区",
            $"{TestNamePrefix}咨询内容");

        var (_, result) = await client.POSTAsync<
            CreateSeaCustomerEndpoint,
            CreateSeaCustomerRequest,
            ResponseData<CreateSeaCustomerResponse>>(request);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        return result.Data.Id;
    }

    private async Task CleanupAsync()
    {
        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var customers = await dbContext.Customers
            .Where(x =>
                x.MainContactName.StartsWith(TestNamePrefix)
                || x.ConsultationContent.StartsWith(TestNamePrefix))
            .ToListAsync(TestContext.Current.CancellationToken);

        if (customers.Count == 0) return;
        dbContext.Customers.RemoveRange(customers);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CheckDuplicateContacts_WithMultipleMatchedCustomers_ShouldReturnAllMatches()
    {
        var client = await GetAuthenticatedClientAsync();
        var sourceId = await GetAnySeaCustomerSourceIdAsync();

        try
        {
            var duplicatePhone = "13800138000";
            var duplicateQq = "qq-dup-001";
            var duplicateWechat = "wechat-dup-001";

            await CreateSeaCustomerAsync(
                client,
                sourceId,
                NewToken(),
                duplicatePhone,
                duplicateQq,
                string.Empty);

            await CreateSeaCustomerAsync(
                client,
                sourceId,
                NewToken(),
                duplicatePhone,
                string.Empty,
                duplicateWechat);

            var request = new CheckSeaCustomerContactDuplicatesRequest(
                "  +86 138-0013-8000 ",
                " qq-dup-001 ",
                "WECHAT-DUP-001");

            var (response, result) = await client.POSTAsync<
                CheckSeaCustomerContactDuplicatesEndpoint,
                CheckSeaCustomerContactDuplicatesRequest,
                ResponseData<CheckSeaCustomerContactDuplicatesResponse>>(request);

            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.Items);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.All(result.Data.Items, x => Assert.False(string.IsNullOrWhiteSpace(x.CustomerName)));
            Assert.All(result.Data.Items, x => Assert.Equal("测试来源", x.CustomerSourceName));
            Assert.All(result.Data.Items, x => Assert.True(string.IsNullOrWhiteSpace(x.OwnerName)));

            Assert.Contains(result.Data.Items, x => x.DuplicatePhones.Any(v => v.Contains("138")));
            Assert.Contains(result.Data.Items, x => x.DuplicateQqs.Any(v => v.Contains("qq-dup-001")));
            Assert.Contains(result.Data.Items, x => x.DuplicateWechats.Any(v => v.Contains("wechat-dup-001")));
        }
        finally
        {
            await CleanupAsync();
        }
    }

    [Fact]
    public async Task CheckDuplicateContacts_WithNoMatch_ShouldReturnEmptyItems()
    {
        var client = await GetAuthenticatedClientAsync();

        var request = new CheckSeaCustomerContactDuplicatesRequest(
            "19900001111",
            "qq-no-match",
            "wechat-no-match");

        var (response, result) = await client.POSTAsync<
            CheckSeaCustomerContactDuplicatesEndpoint,
            CheckSeaCustomerContactDuplicatesRequest,
            ResponseData<CheckSeaCustomerContactDuplicatesResponse>>(request);

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        Assert.Empty(result.Data.Items);
    }

    [Fact]
    public async Task CheckDuplicateContacts_WithAllEmptyContacts_ShouldFailValidation()
    {
        var client = await GetAuthenticatedClientAsync();
        var request = new CheckSeaCustomerContactDuplicatesRequest(string.Empty, string.Empty, string.Empty);

        var (response, result) = await client.POSTAsync<
            CheckSeaCustomerContactDuplicatesEndpoint,
            CheckSeaCustomerContactDuplicatesRequest,
            ResponseData<CheckSeaCustomerContactDuplicatesResponse>>(request);

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorData);
    }
}

