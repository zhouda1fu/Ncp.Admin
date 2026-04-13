using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Application.Commands.Customers;
using NetCorePal.Extensions.Primitives;

namespace Ncp.Admin.Web.Endpoints.Customers;

/// <summary>
/// 为用户保存客户公海片区分配请求
/// </summary>
/// <param name="UserId">用户 ID</param>
/// <param name="SelectedRegionIds">选中的片区 ID 列表</param>
public record AssignCustomerSeaRegionAssignUserRegionsRequest(
    UserId UserId,
    IReadOnlyList<RegionId> SelectedRegionIds);

/// <summary>
/// 为用户保存“客户公海片区分配”
/// </summary>
public class AssignCustomerSeaRegionAssignUserRegionsEndpoint(IMediator mediator)
    : Endpoint<AssignCustomerSeaRegionAssignUserRegionsRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Customer");
        Put("/api/admin/customer-sea-region-assign/users/{userId}/regions");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerSeaRegionAssignEdit);
        Description(b => b.AutoTagOverride("Customer").WithSummary("保存用户的公海片区分配"));
    }

    public override async Task HandleAsync(AssignCustomerSeaRegionAssignUserRegionsRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var operatorUserId))
            throw new KnownException("未登录或用户标识无效");

        var cmd = new AssignCustomerSeaRegionsCommand(
            req.UserId,
            req.SelectedRegionIds,
            operatorUserId);

        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

