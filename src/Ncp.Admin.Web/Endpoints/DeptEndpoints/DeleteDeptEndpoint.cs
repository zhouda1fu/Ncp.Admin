using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Web.Application.Commands.DeptCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.DeptEndpoints;

/// <summary>
/// 删除部门的请求模型
/// </summary>
/// <param name="Id">部门ID</param>
public record DeleteDeptRequest(DeptId Id);

/// <summary>
/// 删除部门的API端点
/// 该端点用于删除指定的部门（软删除）
/// </summary>
[Tags("Depts")]
public class DeleteDeptEndpoint(IMediator mediator) : Endpoint<DeleteDeptRequest, ResponseData<bool>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP DELETE方法，通过路由参数获取部门ID
        Delete("/api/dept/{id}");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和部门删除权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DeptDelete);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 删除指定的部门
    /// </summary>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(DeleteDeptRequest request,CancellationToken ct)
    {

        // 创建删除部门命令
        var command = new DeleteDeptCommand(request.Id);

        // 通过中介者发送命令，执行实际的业务逻辑
        await mediator.Send(command, ct);

        // 返回成功响应
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
