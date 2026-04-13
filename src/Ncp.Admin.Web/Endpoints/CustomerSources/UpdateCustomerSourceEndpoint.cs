using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Web.Application.Commands.CustomerSources;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.CustomersSources;

/// <summary>
/// 更新客户来源请求
/// </summary>
/// <param name="Id">客户来源 ID</param>
/// <param name="Name">名称</param>
/// <param name="SortOrder">排序</param>
/// <param name="UsageScene">使用场景（0公海 1客户列表 2通用）</param>
public record UpdateCustomerSourceRequest(CustomerSourceId Id, string Name, int SortOrder, CustomerSourceUsageScene UsageScene);

/// <summary>
/// 更新客户来源
/// </summary>
public class UpdateCustomerSourceEndpoint(IMediator mediator) : Endpoint<UpdateCustomerSourceRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("CustomerSource");
        Description(b => b.AutoTagOverride("CustomerSource").WithSummary("更新客户来源"));
        Put("/api/admin/customer-sources/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerSourceEdit);
    }

    public override async Task HandleAsync(UpdateCustomerSourceRequest req, CancellationToken ct)
    {
        var cmd = new UpdateCustomerSourceCommand(req.Id, req.Name, req.SortOrder, req.UsageScene);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
