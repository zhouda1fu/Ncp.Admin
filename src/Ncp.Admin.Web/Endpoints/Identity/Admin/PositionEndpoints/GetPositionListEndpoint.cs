using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.PositionEndpoints;

/// <summary>
/// 获取岗位列表的请求模型
/// </summary>
public class GetPositionListRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public DeptId? DeptId { get; set; }
    public int? Status { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 岗位列表项
/// </summary>
public record PositionListItem(
    PositionId Id, string Name, string Code, string Description,
    DeptId DeptId, string? DeptName, int SortOrder, int Status, DateTimeOffset CreatedAt);

/// <summary>
/// 获取岗位列表的响应模型
/// </summary>
public record GetPositionListResponse(IEnumerable<PositionListItem> Items, int Total);

/// <summary>
/// 获取岗位列表
/// </summary>
public class GetPositionListEndpoint(PositionQuery positionQuery) : Endpoint<GetPositionListRequest, ResponseData<GetPositionListResponse>>
{
    public override void Configure()
    {
        Tags("Positions");
        Description(b => b.AutoTagOverride("Positions"));
        Get("/api/admin/position");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.PositionView);
    }

    public override async Task HandleAsync(GetPositionListRequest req, CancellationToken ct)
    {
        var input = new PositionQueryInput
        {
            Name = req.Name,
            Code = req.Code,
            DeptId = req.DeptId,
            Status = req.Status,
            PageIndex = req.PageIndex,
            PageSize = req.PageSize
        };

        var (items, total) = await positionQuery.GetPositionListAsync(input, ct);

        var result = items.Select(p => new PositionListItem(
            p.Id, p.Name, p.Code, p.Description,
            p.DeptId, p.DeptName, p.SortOrder, p.Status, p.CreatedAt));

        await Send.OkAsync(new GetPositionListResponse(result, total).AsResponseData(), cancellation: ct);
    }
}
