using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Projects;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Projects;

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
        Description(b => b.AutoTagOverride("Project").WithSummary("添加项目跟进记录"));
        Post("/api/admin/projects/{projectId}/follow-up-records");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectEdit);
    }

    public override async Task HandleAsync(AddProjectFollowUpRecordRequest req, CancellationToken ct)
    {
        UserId? creatorId = User.GetUserIdOrNull();
        var cmd = new AddProjectFollowUpRecordCommand(
            req.ProjectId, req.Title ?? string.Empty, req.VisitDate, req.ReminderIntervalDays, req.Content ?? string.Empty, creatorId);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new AddProjectFollowUpRecordResponse(id).AsResponseData(), cancellation: ct);
    }
}
