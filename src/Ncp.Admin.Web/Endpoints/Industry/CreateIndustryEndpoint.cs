using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Web.Application.Commands.IndustryModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Industry;

/// <summary>
/// 创建行业请求
/// </summary>
/// <param name="Name">行业名称</param>
/// <param name="ParentId">父级行业 ID，null 表示一级</param>
/// <param name="SortOrder">排序</param>
/// <param name="Remark">备注</param>
public record CreateIndustryRequest(
    string Name,
    IndustryId? ParentId,
    int SortOrder = 0,
    string? Remark = null);

/// <summary>
/// 创建行业响应
/// </summary>
/// <param name="Id">行业 ID</param>
public record CreateIndustryResponse(IndustryId Id);

/// <summary>
/// 创建行业
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class CreateIndustryEndpoint(IMediator mediator)
    : Endpoint<CreateIndustryRequest, ResponseData<CreateIndustryResponse>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Industry");
        Post("/api/admin/industries");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.IndustryCreate);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CreateIndustryRequest req, CancellationToken ct)
    {
        var cmd = new CreateIndustryCommand(req.Name, req.ParentId, req.SortOrder, req.Remark);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateIndustryResponse(id).AsResponseData(), cancellation: ct);
    }
}
