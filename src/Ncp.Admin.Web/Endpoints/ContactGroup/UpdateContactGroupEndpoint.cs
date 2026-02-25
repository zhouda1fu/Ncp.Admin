using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Web.Application.Commands.ContactGroup;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ContactGroup;

/// <summary>
/// 更新联系组请求
/// </summary>
public class UpdateContactGroupRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
}

/// <summary>
/// 更新联系组
/// </summary>
public class UpdateContactGroupEndpoint(IMediator mediator) : Endpoint<UpdateContactGroupRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("ContactGroup");
        Put("/api/admin/contact-groups/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContactGroupEdit);
    }

    public override async Task HandleAsync(UpdateContactGroupRequest req, CancellationToken ct)
    {
        var id = new ContactGroupId(req.Id);
        var cmd = new UpdateContactGroupCommand(id, req.Name, req.SortOrder);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
