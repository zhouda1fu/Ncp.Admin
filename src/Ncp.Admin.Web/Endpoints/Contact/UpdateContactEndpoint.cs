using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContactAggregate;
using Ncp.Admin.Domain.AggregatesModel.ContactGroupAggregate;
using Ncp.Admin.Web.Application.Commands.Contact;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contact;

/// <summary>
/// 更新联系人请求
/// </summary>
public class UpdateContactRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Company { get; set; }
    public Guid? GroupId { get; set; }
}

/// <summary>
/// 更新联系人
/// </summary>
public class UpdateContactEndpoint(IMediator mediator) : Endpoint<UpdateContactRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Contact");
        Put("/api/admin/contacts/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContactEdit);
    }

    public override async Task HandleAsync(UpdateContactRequest req, CancellationToken ct)
    {
        var id = new ContactId(req.Id);
        var groupId = req.GroupId.HasValue ? new ContactGroupId(req.GroupId.Value) : (ContactGroupId?)null;
        var cmd = new UpdateContactCommand(id, req.Name, req.Phone, req.Email, req.Company, groupId);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
