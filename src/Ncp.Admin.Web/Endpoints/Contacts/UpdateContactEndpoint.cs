using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContactAggregate;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Web.Application.Commands.Contacts;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contacts;

/// <summary>
/// 更新联系人请求
/// </summary>
/// <param name="Id">联系人 ID</param>
/// <param name="Name">姓名</param>
/// <param name="Phone">电话</param>
/// <param name="Email">邮箱</param>
/// <param name="Company">公司</param>
/// <param name="GroupId">联系组 ID</param>
public record UpdateContactRequest(ContactId Id, string Name, string? Phone, string? Email, string? Company, ContactGroupId? GroupId);

/// <summary>
/// 更新联系人
/// </summary>
public class UpdateContactEndpoint(IMediator mediator) : Endpoint<UpdateContactRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Contact");
        Description(b => b.AutoTagOverride("Contact").WithSummary("更新联系人"));
        Put("/api/admin/contacts/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContactEdit);
    }

    public override async Task HandleAsync(UpdateContactRequest req, CancellationToken ct)
    {
        var cmd = new UpdateContactCommand(req.Id, req.Name, req.Phone, req.Email, req.Company, req.GroupId);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
