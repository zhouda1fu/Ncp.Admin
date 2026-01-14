using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Web.Application.Commands.DeptCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.DeptEndpoints;

/// <summary>
/// 更新部门的请求模型
/// </summary>
/// <param name="Id">部门ID</param>
/// <param name="Name">部门名称</param>
/// <param name="Remark">备注</param>
/// <param name="ParentId">父级部门ID，可为空表示顶级部门</param>
/// <param name="Status">状态（0=禁用，1=启用）</param>
public record UpdateDeptRequest(DeptId Id, string Name, string Remark, DeptId? ParentId, int Status);

/// <summary>
/// 更新部门的API端点
/// 该端点用于更新现有部门的信息
/// </summary>
[Tags("Depts")]
public class UpdateDeptEndpoint(IMediator mediator) : Endpoint<UpdateDeptRequest, ResponseData<bool>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP PUT方法，用于更新部门信息
        Put("/api/dept");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和部门编辑权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DeptEdit);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 更新部门信息
    /// </summary>
    /// <param name="req">包含部门更新信息的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(UpdateDeptRequest req, CancellationToken ct)
    {
        // 将请求转换为领域命令对象
        // 如果父级ID为空，则设置为根部门（ID为0）
        var command = new UpdateDeptCommand(
            req.Id,
            req.Name,
            req.Remark,
            req.ParentId ?? new DeptId(0),
            req.Status
        );

        // 通过中介者发送命令，执行实际的业务逻辑
        await mediator.Send(command, ct);

        // 返回成功响应
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
