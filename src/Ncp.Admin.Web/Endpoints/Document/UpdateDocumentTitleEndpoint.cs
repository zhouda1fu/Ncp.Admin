using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Web.Application.Commands.Document;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Document;

/// <summary>
/// 更新文档标题请求
/// </summary>
/// <param name="Id">文档 ID</param>
/// <param name="Title">标题</param>
public record UpdateDocumentTitleRequest(DocumentId Id, string Title);

/// <summary>
/// 更新文档标题
/// </summary>
public class UpdateDocumentTitleEndpoint(IMediator mediator)
    : Endpoint<UpdateDocumentTitleRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Document");
        Put("/api/admin/documents/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DocumentEdit);
    }

    public override async Task HandleAsync(UpdateDocumentTitleRequest req, CancellationToken ct)
    {
        var cmd = new UpdateDocumentTitleCommand(req.Id, req.Title);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
