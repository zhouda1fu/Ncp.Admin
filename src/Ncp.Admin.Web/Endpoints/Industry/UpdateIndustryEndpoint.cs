using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Web.Application.Commands.Industry;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Industry;

/// <summary>
/// 更新行业请求（Id 来自路由）
/// </summary>
/// <param name="Name">行业名称</param>
/// <param name="ParentId">父级行业 ID，null 或空表示一级</param>
/// <param name="SortOrder">排序</param>
/// <param name="Remark">备注</param>
public record UpdateIndustryRequest(string Name, IndustryId? ParentId, int SortOrder = 0, string? Remark = null);

/// <summary>
/// 更新行业
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class UpdateIndustryEndpoint(IMediator mediator)
    : Endpoint<UpdateIndustryRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Industry");
        Put("/api/admin/industries/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.IndustryEdit);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UpdateIndustryRequest req, CancellationToken ct)
    {
        var id = new IndustryId(Route<Guid>("id"));
        var cmd = new UpdateIndustryCommand(id, req.Name, req.ParentId, req.SortOrder, req.Remark);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
