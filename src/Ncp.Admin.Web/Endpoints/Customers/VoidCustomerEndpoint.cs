using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customers;

/// <summary>
/// 发起客户作废审批请求
/// </summary>
/// <param name="Id">客户 ID（公海未领用任意有权限用户；已领用仅领用人）</param>
/// <param name="RoutingRoleId">多角色用户必选：本次用于条件分支的路由角色 ID</param>
public record VoidCustomerRequest(CustomerId Id, RoleId? RoutingRoleId = null);

/// <summary>
/// 发起客户作废审批响应
/// </summary>
/// <param name="WorkflowInstanceId">已创建的工作流实例 ID</param>
/// <param name="Title">流程标题</param>
public record VoidCustomerResponse(WorkflowInstanceId WorkflowInstanceId, string Title);

/// <summary>
/// 发起客户作废审批（审批通过后执行实际作废）
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class VoidCustomerEndpoint(IMediator mediator) : Endpoint<VoidCustomerRequest, ResponseData<VoidCustomerResponse>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Post("/api/admin/customers/{id}/void");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerSeaVoid);
        Description(b => b.AutoTagOverride("Customer").WithSummary("发起客户作废审批"));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(VoidCustomerRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(
            new StartCustomerSeaVoidWorkflowCommand(req.Id, uid, req.RoutingRoleId),
            ct);
        await Send.OkAsync(
            new VoidCustomerResponse(result.WorkflowInstanceId, result.Title).AsResponseData(),
            cancellation: ct);
    }
}
