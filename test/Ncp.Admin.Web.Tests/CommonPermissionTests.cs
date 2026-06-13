using System.Net;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Endpoints.Identity.Admin.RoleEndpoints;
using Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Tests;

/// <summary>
/// 公共基础数据权限与 AllApiAccess 超级管理员兜底的行为矩阵测试。
/// </summary>
[Collection(WebAppTestCollection.Name)]
public class CommonPermissionTests(WebAppFixture app) : AuthenticatedTestBase<WebAppFixture>(app)
{
    private async Task<DeptId> GetRootDeptIdAsync()
    {
        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dept = await dbContext.Depts.FirstOrDefaultAsync(
            d => d.Name == "总公司",
            TestContext.Current.CancellationToken);
        return dept?.Id ?? throw new InvalidOperationException("找不到根部门");
    }

    private async Task<HttpClient> CreateClientWithRolePermissionsAsync(
        IReadOnlyList<string> permissionCodes,
        CancellationToken cancellationToken = default)
    {
        var adminClient = await GetAuthenticatedClientAsync();
        var roleName = $"权限测试角色_{Guid.NewGuid():N}";
        var userName = $"perm_test_{Guid.NewGuid():N}";
        var password = "123456";
        var deptId = await GetRootDeptIdAsync();

        var createRoleRequest = new CreateRoleRequest(roleName, "权限矩阵测试", permissionCodes);
        var (_, createRoleResult) = await adminClient.POSTAsync<
            CreateRoleEndpoint,
            CreateRoleRequest,
            ResponseData<CreateRoleResponse>>(createRoleRequest);
        var roleId = createRoleResult?.Data?.RoleId
            ?? throw new InvalidOperationException("创建测试角色失败");

        var createUserRequest = new CreateUserRequest(
            userName,
            $"{userName}@test.local",
            password,
            "13800138000",
            "权限测试用户",
            1,
            "男",
            DateTimeOffset.UtcNow.AddYears(-25),
            deptId,
            "总公司",
            null,
            null,
            [roleId],
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            false,
            0,
            string.Empty,
            false,
            DateTimeOffset.MinValue);

        var (_, createUserResult) = await adminClient.POSTAsync<
            CreateUserEndpoint,
            CreateUserRequest,
            ResponseData<CreateUserResponse>>(createUserRequest);
        if (createUserResult?.Data == null)
        {
            throw new InvalidOperationException("创建测试用户失败");
        }

        var client = Fixture.CreateClient();
        var loginRequest = new LoginRequest(userName, password);
        var (_, loginResponse) = await client.POSTAsync<
            LoginEndpoint,
            LoginRequest,
            ResponseData<LoginResponse>>(loginRequest);
        if (loginResponse?.Data == null)
        {
            throw new InvalidOperationException("测试用户登录失败");
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResponse.Data.Token);

        return client;
    }

    private async Task CleanupPermissionTestDataAsync()
    {
        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var testUsers = await dbContext.Users
            .Where(u => u.Name.StartsWith("perm_test_"))
            .ToListAsync(TestContext.Current.CancellationToken);
        foreach (var user in testUsers)
        {
            user.SoftDelete(UserId.Unassigned);
        }

        var testRoles = await dbContext.Roles
            .Where(r => r.Name.StartsWith("权限测试角色_"))
            .ToListAsync(TestContext.Current.CancellationToken);
        foreach (var role in testRoles)
        {
            role.SoftDelete();
        }

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task RoleOptionView_CanListRoles_ButCannotCreateRole()
    {
        try
        {
            var client = await CreateClientWithRolePermissionsAsync([PermissionCodes.RoleOptionView]);

            var listInput = new RoleQueryInput { PageIndex = 1, PageSize = 10, CountTotal = true };
            var (listResponse, listResult) = await client.GETAsync<
                GetAllRolesEndpoint,
                RoleQueryInput,
                ResponseData<PagedData<RoleQueryDto>>>(listInput);

            Assert.True(listResponse.IsSuccessStatusCode);
            Assert.NotNull(listResult);
            Assert.True(listResult.Success);

            var createRequest = new CreateRoleRequest(
                $"权限测试角色_{Guid.NewGuid():N}",
                "应被拒绝",
                [PermissionCodes.UserView]);
            var (createResponse, _) = await client.POSTAsync<
                CreateRoleEndpoint,
                CreateRoleRequest,
                ResponseData<CreateRoleResponse>>(createRequest);

            Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
        }
        finally
        {
            await CleanupPermissionTestDataAsync();
        }
    }

    [Fact]
    public async Task RoleView_CanListRoles_ButCannotCreateWithoutRoleCreate()
    {
        try
        {
            var client = await CreateClientWithRolePermissionsAsync([PermissionCodes.RoleView]);

            var listInput = new RoleQueryInput { PageIndex = 1, PageSize = 10, CountTotal = true };
            var (listResponse, _) = await client.GETAsync<
                GetAllRolesEndpoint,
                RoleQueryInput,
                ResponseData<PagedData<RoleQueryDto>>>(listInput);

            Assert.True(listResponse.IsSuccessStatusCode);

            var createRequest = new CreateRoleRequest(
                $"权限测试角色_{Guid.NewGuid():N}",
                "应被拒绝",
                [PermissionCodes.UserView]);
            var (createResponse, _) = await client.POSTAsync<
                CreateRoleEndpoint,
                CreateRoleRequest,
                ResponseData<CreateRoleResponse>>(createRequest);

            Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
        }
        finally
        {
            await CleanupPermissionTestDataAsync();
        }
    }

    [Fact]
    public async Task WithoutFileAccess_UploadIsForbidden()
    {
        try
        {
            var client = await CreateClientWithRolePermissionsAsync([PermissionCodes.RoleOptionView]);
            using var content = new MultipartFormDataContent();

            var response = await client.PostAsync("/api/admin/files/upload", content);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            await CleanupPermissionTestDataAsync();
        }
    }

    [Fact]
    public async Task AllApiAccess_CanBypassModulePermissionForCreateRole()
    {
        try
        {
            var client = await CreateClientWithRolePermissionsAsync([PermissionCodes.AllApiAccess]);

            var createRequest = new CreateRoleRequest(
                $"权限测试角色_{Guid.NewGuid():N}",
                "AllApiAccess 兜底",
                [PermissionCodes.UserView]);
            var (createResponse, createResult) = await client.POSTAsync<
                CreateRoleEndpoint,
                CreateRoleRequest,
                ResponseData<CreateRoleResponse>>(createRequest);

            Assert.True(createResponse.IsSuccessStatusCode);
            Assert.NotNull(createResult);
            Assert.True(createResult.Success);
        }
        finally
        {
            await CleanupPermissionTestDataAsync();
        }
    }
}
