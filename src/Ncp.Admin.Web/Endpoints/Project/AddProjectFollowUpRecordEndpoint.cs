using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.ProjectModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Project;

public record AddProjectFollowUpRecordRequest(
    ProjectId ProjectId,
    string Title,
    DateOnly? VisitDate,
    int ReminderIntervalDays,
    string Content);

public record AddProjectFollowUpRecordResponse(ProjectFollowUpRecordId Id);

public class AddProjectFollowUpRecordEndpoint(IMediator mediator) : Endpoint<AddProjectFollowUpRecordRequest, ResponseData<AddProjectFollowUpRecordResponse>>
{
    public override void Configure()
    {
        Tags("Project");
        Post("/api/admin/projects/{projectId}/follow-up-records");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectEdit);
    }

    public override async Task HandleAsync(AddProjectFollowUpRecordRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long? uid = null;
        if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out var parsed))
            uid = parsed;
        var creatorId = uid.HasValue ? new UserId(uid.Value) : (UserId?)null;
        var cmd = new AddProjectFollowUpRecordCommand(
            req.ProjectId, req.Title ?? string.Empty, req.VisitDate, req.ReminderIntervalDays, req.Content ?? string.Empty, creatorId);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new AddProjectFollowUpRecordResponse(id).AsResponseData(), cancellation: ct);
    }
}
