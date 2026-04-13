using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Web.Application.Commands.ContactGroups;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ContactsGroups;

/// <summary>
/// 更新联系组请求
/// </summary>
/// <param name="Id">联系组 ID</param>
/// <param name="Name">名称</param>
/// <param name="SortOrder">排序</param>
public record UpdateContactGroupRequest(ContactGroupId Id, string Name, int SortOrder);

/// <summary>
/// 更新联系组
/// </summary>
public class UpdateContactGroupEndpoint(IMediator mediator) : Endpoint<UpdateContactGroupRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("ContactGroup");
        Description(b => b.AutoTagOverride("ContactGroup").WithSummary("更新联系组"));
        Put("/api/admin/contact-groups/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContactGroupEdit);
    }

    public override async Task HandleAsync(UpdateContactGroupRequest req, CancellationToken ct)
    {
        var cmd = new UpdateContactGroupCommand(req.Id, req.Name, req.SortOrder);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
