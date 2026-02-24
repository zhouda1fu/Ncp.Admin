using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Leave;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Leave;

public record CreateLeaveRequestRequest(
    LeaveType LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Days,
    string? Reason);

public record CreateLeaveRequestResponse(LeaveRequestId Id);

public class CreateLeaveRequestEndpoint(IMediator mediator, UserQuery userQuery) : Endpoint<CreateLeaveRequestRequest, ResponseData<CreateLeaveRequestResponse>>
{
    public override void Configure()
    {
        Tags("Leave");
        Post("/api/admin/leave/requests");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.LeaveRequestCreate);
    }

    public override async Task HandleAsync(CreateLeaveRequestRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var userIdValue))
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);

        var user = await userQuery.GetUserByIdAsync(new UserId(userIdValue), ct)
            ?? throw new KnownException("未找到当前用户", ErrorCodes.UserNotFound);

        var cmd = new CreateLeaveRequestCommand(
            new UserId(userIdValue),
            user.RealName ?? user.Name,
            req.LeaveType,
            req.StartDate,
            req.EndDate,
            req.Days,
            req.Reason ?? "");
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateLeaveRequestResponse(id).AsResponseData(), cancellation: ct);
    }
}
