using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Web.Application.Commands.CustomerSource;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.CustomerSource;

/// <summary>
/// 创建客户来源请求
/// </summary>
/// <param name="Name">名称</param>
/// <param name="SortOrder">排序</param>
/// <param name="UsageScene">使用场景（0公海 1客户列表 2通用）</param>
public record CreateCustomerSourceRequest(string Name, int SortOrder, CustomerSourceUsageScene UsageScene = CustomerSourceUsageScene.Both);

public record CreateCustomerSourceResponse(CustomerSourceId Id);

public class CreateCustomerSourceEndpoint(IMediator mediator) : Endpoint<CreateCustomerSourceRequest, ResponseData<CreateCustomerSourceResponse>>
{
    public override void Configure()
    {
        Tags("CustomerSource");
        Post("/api/admin/customer-sources");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerSourceCreate);
    }

    public override async Task HandleAsync(CreateCustomerSourceRequest req, CancellationToken ct)
    {
        var cmd = new CreateCustomerSourceCommand(req.Name, req.SortOrder, req.UsageScene);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateCustomerSourceResponse(id).AsResponseData(), cancellation: ct);
    }
}
