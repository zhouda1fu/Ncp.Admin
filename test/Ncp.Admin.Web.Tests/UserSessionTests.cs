using System.Net;
using System.Net.Http.Headers;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Dto;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;
using Ncp.Admin.Web.Services;
using Ncp.Admin.Web.Tests.Fixtures;

namespace Ncp.Admin.Web.Tests;

[Collection(WebAppTestCollection.Name)]
public class UserSessionTests(WebAppFixture fixture) : TestBase<WebAppFixture>
{
    private const string Username = "session-test-user";
    private const string Password = "SessionTest@123";

    [Fact]
    public async Task NewLogin_ShouldInvalidatePreviousSession()
    {
        await EnsureAdminPasswordAsync();
        var firstClient = fixture.CreateClient();
        var secondClient = fixture.CreateClient();
        var firstToken = await LoginAsync(firstClient);
        var secondToken = await LoginAsync(secondClient);

        firstClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", firstToken);
        secondClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secondToken);

        var firstResponse = await firstClient.GetAsync("/api/admin/user/profile", TestContext.Current.CancellationToken);
        var secondResponse = await secondClient.GetAsync("/api/admin/user/profile", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, firstResponse.StatusCode);
        Assert.Equal(
            UserSessionAuthenticationReasons.SessionReplaced,
            firstResponse.Headers.GetValues(UserSessionAuthenticationReasons.HeaderName).Single());
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
    }

    [Fact]
    public async Task PreviousSessionLogout_ShouldNotInvalidateCurrentSession()
    {
        await EnsureAdminPasswordAsync();
        var firstClient = fixture.CreateClient();
        var secondClient = fixture.CreateClient();
        var firstToken = await LoginAsync(firstClient);
        var secondToken = await LoginAsync(secondClient);

        firstClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", firstToken);
        secondClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secondToken);

        await firstClient.PostAsync(
            "/api/admin/auth/logout",
            content: null,
            TestContext.Current.CancellationToken);
        var secondResponse = await secondClient.GetAsync("/api/admin/user/profile", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
    }

    private static async Task<string> LoginAsync(HttpClient client)
    {
        var (httpResponse, response) = await client.POSTAsync<LoginEndpoint, LoginRequest, ResponseData<LoginResponse>>(
            new LoginRequest(Username, Password));
        return response.Data?.Token
               ?? throw new InvalidOperationException(
                   $"登录失败：{httpResponse.StatusCode} {await httpResponse.Content.ReadAsStringAsync()}");
    }

    private async Task EnsureAdminPasswordAsync()
    {
        using var scope = fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = await dbContext.Users.SingleOrDefaultAsync(
            x => x.Name == Username,
            TestContext.Current.CancellationToken);
        if (user is null)
        {
            user = new User(
                Username,
                "13800000000",
                passwordHasher.Hash(Password),
                [],
                "会话测试用户",
                1,
                "session-test@example.com",
                string.Empty,
                DateTimeOffset.MinValue,
                UserId.Unassigned,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);
            await dbContext.Users.AddAsync(user, TestContext.Current.CancellationToken);
        }
        else
        {
            user.PasswordReset(passwordHasher.Hash(Password));
        }
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
