using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContactAggregate;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Contacts;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contacts;

/// <summary>
/// 创建联系人请求
/// </summary>
/// <param name="Name">姓名</param>
/// <param name="Phone">电话</param>
/// <param name="Email">邮箱</param>
/// <param name="Company">公司</param>
/// <param name="GroupId">联系组 ID</param>
public record CreateContactRequest(string Name, string? Phone, string? Email, string? Company, ContactGroupId? GroupId);

/// <summary>
/// 创建联系人（当前用户为创建人）
/// </summary>
public class CreateContactEndpoint(IMediator mediator)
    : Endpoint<CreateContactRequest, ResponseData<CreateContactResponse>>
{
    public override void Configure()
    {
        Tags("Contact");
        Description(b => b.AutoTagOverride("Contact").WithSummary("创建联系人（当前用户为创建人）"));
        Post("/api/admin/contacts");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContactCreate);
    }

    public override async Task HandleAsync(CreateContactRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CreateContactCommand(uid, req.Name, req.Phone, req.Email, req.Company, req.GroupId);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateContactResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record CreateContactResponse(ContactId Id);
