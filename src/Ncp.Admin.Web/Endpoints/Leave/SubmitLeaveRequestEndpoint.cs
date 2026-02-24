using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Web.Application.Commands.Leave;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Leave;

public record SubmitLeaveRequestRequest(LeaveRequestId Id, string? Remark);

public class SubmitLeaveRequestEndpoint(IMediator mediator) : Endpoint<SubmitLeaveRequestRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Leave");
        Post("/api/admin/leave/requests/{id}/submit");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.LeaveRequestSubmit);
    }

    public override async Task HandleAsync(SubmitLeaveRequestRequest req, CancellationToken ct)
    {
        await mediator.Send(new SubmitLeaveRequestCommand(req.Id, req.Remark ?? ""), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
