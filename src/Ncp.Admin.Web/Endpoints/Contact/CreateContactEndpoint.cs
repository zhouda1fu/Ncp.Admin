using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContactAggregate;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Contact;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contact;

/// <summary>
/// 创建联系人请求
/// </summary>
public class CreateContactRequest
{
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Company { get; set; }
    public Guid? GroupId { get; set; }
}

/// <summary>
/// 创建联系人（当前用户为创建人）
/// </summary>
public class CreateContactEndpoint(IMediator mediator)
    : Endpoint<CreateContactRequest, ResponseData<CreateContactResponse>>
{
    public override void Configure()
    {
        Tags("Contact");
        Post("/api/admin/contacts");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContactCreate);
    }

    public override async Task HandleAsync(CreateContactRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var groupId = req.GroupId.HasValue ? new ContactGroupId(req.GroupId.Value) : (ContactGroupId?)null;
        var cmd = new CreateContactCommand(new UserId(uid), req.Name, req.Phone, req.Email, req.Company, groupId);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateContactResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record CreateContactResponse(ContactId Id);
