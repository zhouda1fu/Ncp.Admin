using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Utils;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 更新用户信息的请求模型
/// </summary>
/// <param name="UserId">要更新的用户ID</param>
/// <param name="Name">用户名</param>
/// <param name="Email">邮箱地址</param>
/// <param name="Phone">电话号码</param>
/// <param name="RealName">真实姓名</param>
/// <param name="Status">用户状态</param>
/// <param name="Gender">性别</param>
/// <param name="Age">年龄</param>
/// <param name="BirthDate">出生日期</param>
/// <param name="DeptId">部门ID</param>
/// <param name="DeptName">部门名称</param>
/// <param name="PositionId">岗位ID（可选，null 表示清除岗位）</param>
/// <param name="PositionName">岗位名称（可选）</param>
/// <param name="Password">密码（可选，为空则不更新）</param>
/// <param name="IdCardNumber">身份证</param>
/// <param name="Address">地址</param>
/// <param name="Education">学历</param>
/// <param name="GraduateSchool">毕业院校</param>
/// <param name="AvatarUrl">头像地址</param>
/// <param name="NotOrderMeal">不订餐：true 为不参与订餐，false 为参与订餐。</param>
/// <param name="OrderMealSort">订餐排序（可选）</param>
/// <param name="AttendanceRequired">是否需要参与考勤计算；false 表示不参与考勤。</param>
/// <param name="WechatGuid">唯一码</param>
/// <param name="IsResigned">是否离职</param>
/// <param name="ResignedTime">离职时间（可选）</param>
/// <param name="SetAsDeptResponsibleUser">是否为所属部门负责人；null 表示不修改负责人关系</param>
/// <param name="SetAsDefaultDeptResponsibleUser">是否为所属部门默认负责人；null 表示不修改默认负责人关系</param>
public record UpdateUserRequest(
    UserId UserId,
    string Name,
    string Email,
    string Phone,
    string RealName,
    int Status,
    string Gender,
    int Age,
    DateTimeOffset BirthDate,
    DeptId DeptId,
    string DeptName,
    PositionId? PositionId,
    string? PositionName,
    string Password,
    string IdCardNumber,
    string Address,
    string Education,
    string GraduateSchool,
    string AvatarUrl,
    bool NotOrderMeal,
    int OrderMealSort,
    bool AttendanceRequired,
    string WechatGuid,
    bool IsResigned,
    DateTimeOffset ResignedTime,
    bool? SetAsDeptResponsibleUser = null,
    bool? SetAsDefaultDeptResponsibleUser = null);

/// <summary>
/// 更新用户信息的响应模型
/// </summary>
/// <param name="UserId">已更新的用户ID</param>
/// <param name="Name">用户名</param>
/// <param name="Email">邮箱地址</param>
public record UpdateUserResponse(UserId UserId, string Name, string Email);

/// <summary>
/// 更新用户
/// </summary>
/// <param name="mediator"></param>
public class UpdateUserEndpoint(IMediator mediator) : Endpoint<UpdateUserRequest, ResponseData<UpdateUserResponse>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users").WithSummary("更新用户"));
        Put("/api/admin/user/update");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.UserEdit);
    }

    public override async Task HandleAsync(UpdateUserRequest request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            throw new KnownException(
                "不允许通过更新用户接口修改密码，请使用密码重置接口。",
                ErrorCodes.UserPasswordMustUseResetEndpoint);
        }

        var modifierId = User.GetUserIdOrNull() ?? UserId.Unassigned;
        var cmd = new UpdateUserCommand(
            request.UserId,
            request.Name,
            request.Email,
            request.Phone,
            request.RealName,
            request.Status,
            request.Gender,
            request.Age,
            request.BirthDate,
            request.DeptId,
            request.DeptName,
            request.PositionId,
            request.PositionName,
            string.Empty,
            request.IdCardNumber,
            request.Address,
            request.Education,
            request.GraduateSchool,
            request.AvatarUrl,
            request.NotOrderMeal,
            request.OrderMealSort,
            request.AttendanceRequired,
            request.WechatGuid,
            request.IsResigned,
            request.ResignedTime,
            request.SetAsDeptResponsibleUser,
            request.SetAsDefaultDeptResponsibleUser,
            modifierId
        );
        var userId = await mediator.Send(cmd, ct);
        var response = new UpdateUserResponse(userId, request.Name, request.Email);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

