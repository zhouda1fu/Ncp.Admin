using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.ContactGroups;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ContactsGroups;

/// <summary>
/// 创建联系组请求
/// </summary>
/// <param name="Name">名称</param>
/// <param name="SortOrder">排序</param>
public record CreateContactGroupRequest(string Name, int SortOrder);

/// <summary>
/// 创建联系组（当前用户为创建人）
/// </summary>
public class CreateContactGroupEndpoint(IMediator mediator)
    : Endpoint<CreateContactGroupRequest, ResponseData<CreateContactGroupResponse>>
{
    public override void Configure()
    {
        Tags("ContactGroup");
        Description(b => b.AutoTagOverride("ContactGroup").WithSummary("创建联系组（当前用户为创建人）"));
        Post("/api/admin/contact-groups");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContactGroupCreate);
    }

    public override async Task HandleAsync(CreateContactGroupRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CreateContactGroupCommand(uid, req.Name, req.SortOrder);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateContactGroupResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record CreateContactGroupResponse(ContactGroupId Id);
