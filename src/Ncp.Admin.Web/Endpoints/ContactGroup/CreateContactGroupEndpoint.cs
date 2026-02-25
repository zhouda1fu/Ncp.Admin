using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.ContactGroup;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ContactGroup;

/// <summary>
/// 创建联系组请求
/// </summary>
public class CreateContactGroupRequest
{
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
}

/// <summary>
/// 创建联系组（当前用户为创建人）
/// </summary>
public class CreateContactGroupEndpoint(IMediator mediator)
    : Endpoint<CreateContactGroupRequest, ResponseData<CreateContactGroupResponse>>
{
    public override void Configure()
    {
        Tags("ContactGroup");
        Post("/api/admin/contact-groups");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContactGroupCreate);
    }

    public override async Task HandleAsync(CreateContactGroupRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CreateContactGroupCommand(new UserId(uid), req.Name, req.SortOrder);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateContactGroupResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record CreateContactGroupResponse(ContactGroupId Id);
