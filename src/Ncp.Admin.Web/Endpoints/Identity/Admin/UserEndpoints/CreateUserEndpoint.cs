using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Utils;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 创建用户的请求模型
/// </summary>
/// <param name="Name">用户名</param>
/// <param name="Email">邮箱地址</param>
/// <param name="Password">密码</param>
/// <param name="Phone">电话号码</param>
/// <param name="RealName">真实姓名</param>
/// <param name="Status">用户状态（0=禁用，1=启用）</param>
/// <param name="Gender">性别</param>
/// <param name="BirthDate">出生日期</param>
/// <param name="DeptId">部门ID（可选）</param>
/// <param name="DeptName">部门名称（可选）</param>
/// <param name="IsDeptManager">是否为该部门主管</param>
/// <param name="PositionId">岗位ID（可选）</param>
/// <param name="PositionName">岗位名称（可选）</param>
/// <param name="RoleIds">要分配的角色ID列表</param>
/// <param name="IdCardNumber">身份证</param>
/// <param name="Address">地址</param>
/// <param name="Education">学历</param>
/// <param name="GraduateSchool">毕业院校</param>
/// <param name="AvatarUrl">头像地址</param>
/// <param name="NotOrderMeal">是否订餐（true=不订餐，false=订餐）</param>
/// <param name="OrderMealSort">订餐排序（可选）</param>
/// <param name="WechatGuid">唯一码</param>
/// <param name="IsResigned">是否离职</param>
/// <param name="ResignedTime">离职时间（可选）</param>
public record CreateUserRequest(
    string Name,
    string Email,
    string Password,
    string Phone,
    string RealName,
    int Status,
    string Gender,
    DateTimeOffset BirthDate,
    DeptId? DeptId,
    string? DeptName,
    bool IsDeptManager,
    PositionId? PositionId,
    string? PositionName,
    IEnumerable<RoleId> RoleIds,
    string IdCardNumber,
    string Address,
    string Education,
    string GraduateSchool,
    string AvatarUrl,
    bool NotOrderMeal,
    int OrderMealSort,
    string WechatGuid,
    bool IsResigned,
    DateTimeOffset ResignedTime);

/// <summary>
/// 创建用户的响应模型
/// </summary>
/// <param name="UserId">新创建的用户ID</param>
/// <param name="Name">用户名</param>
/// <param name="Email">邮箱地址</param>
public record CreateUserResponse(UserId UserId, string Name, string Email);

/// <summary>
/// 创建用户
/// </summary>
/// <param name="mediator"></param>
/// <param name="roleQuery"></param>
public class CreateUserEndpoint(IMediator mediator, RoleQuery roleQuery) : Endpoint<CreateUserRequest, ResponseData<CreateUserResponse>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
        Post("/api/admin/users");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserCreate);
    }

    public override async Task HandleAsync(CreateUserRequest request, CancellationToken ct)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var creatorId = !string.IsNullOrWhiteSpace(userIdString) && long.TryParse(userIdString, out var userIdValue)
            ? new UserId(userIdValue)
            : new UserId(0);
        var rolesToBeAssigned = await roleQuery.GetAdminRolesForAssignmentAsync(request.RoleIds, ct);
        var cmd = new CreateUserCommand(
            request.Name,
            request.Email,
            request.Password,
            request.Phone,
            request.RealName,
            request.Status,
            request.Gender,
            request.BirthDate,
            request.DeptId,
            request.DeptName,
            request.IsDeptManager,
            request.PositionId,
            request.PositionName,
            rolesToBeAssigned,
            creatorId,
            request.IdCardNumber,
            request.Address,
            request.Education,
            request.GraduateSchool,
            request.AvatarUrl,
            request.NotOrderMeal,
            request.WechatGuid,
            request.IsResigned,
            request.ResignedTime
        );
        var userId = await mediator.Send(cmd, ct);
        var response = new CreateUserResponse(userId, request.Name, request.Email);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}
