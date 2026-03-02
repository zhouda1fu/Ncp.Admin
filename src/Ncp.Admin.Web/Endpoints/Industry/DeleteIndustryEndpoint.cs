using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Web.Application.Commands.Industry;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Industry;

/// <summary>
/// 删除行业
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class DeleteIndustryEndpoint(IMediator mediator) : EndpointWithoutRequest<ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Industry");
        Delete("/api/admin/industries/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.IndustryDelete);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = new IndustryId(Route<Guid>("id"));
        await mediator.Send(new DeleteIndustryCommand(id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
