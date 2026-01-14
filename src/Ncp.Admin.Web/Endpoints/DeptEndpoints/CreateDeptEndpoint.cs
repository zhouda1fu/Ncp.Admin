using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Web.Application.Commands.DeptCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.DeptEndpoints;

/// <summary>
/// 创建部门的请求模型
/// </summary>
/// <param name="Name">部门名称</param>
/// <param name="Remark">备注</param>
/// <param name="ParentId">父级部门ID，可为空表示顶级部门</param>
/// <param name="Status">状态（0=禁用，1=启用）</param>
public record CreateDeptRequest(string Name, string Remark, DeptId? ParentId, int Status);

/// <summary>
/// 创建部门的响应模型
/// </summary>
/// <param name="Id">新创建的部门ID</param>
/// <param name="Name">部门名称</param>
/// <param name="Remark">备注</param>
public record CreateDeptResponse(DeptId Id, string Name, string Remark);

/// <summary>
/// 创建部门的API端点
/// 该端点用于在系统中创建新的部门，支持层级结构
/// </summary>
[Tags("Depts")]
public class CreateDeptEndpoint(IMediator mediator) : Endpoint<CreateDeptRequest, ResponseData<CreateDeptResponse>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP POST方法，用于创建新的部门
        Post("/api/dept");

        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

        // 设置权限要求：用户必须同时拥有API访问权限和部门创建权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DeptCreate);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 创建新的部门并返回结果
    /// </summary>
    /// <param name="req">包含部门信息的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(CreateDeptRequest req, CancellationToken ct)
    {
        // 创建部门命令
        var cmd = new CreateDeptCommand(req.Name, req.Remark, req.ParentId, req.Status);

        // 执行命令并获取新创建的部门ID
        var deptId = await mediator.Send(cmd, ct);

        // 构建响应对象
        var response = new CreateDeptResponse(deptId, req.Name, req.Remark);

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}
