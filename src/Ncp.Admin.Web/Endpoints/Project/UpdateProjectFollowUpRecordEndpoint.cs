using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Web.Application.Commands.Projects;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Project;

/// <summary>
/// 更新项目跟进记录请求
/// </summary>
public record UpdateProjectFollowUpRecordRequest(
    ProjectId ProjectId,
    ProjectFollowUpRecordId RecordId,
    string Title,
    DateOnly? VisitDate,
    int ReminderIntervalDays,
    string Content);

/// <summary>
/// 更新项目跟进记录
/// </summary>
public class UpdateProjectFollowUpRecordEndpoint(IMediator mediator) : Endpoint<UpdateProjectFollowUpRecordRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Project");
        Put("/api/admin/projects/{projectId}/follow-up-records/{recordId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectEdit);
    }

    public override async Task HandleAsync(UpdateProjectFollowUpRecordRequest req, CancellationToken ct)
    {
        var cmd = new UpdateProjectFollowUpRecordCommand(
            req.ProjectId, req.RecordId, req.Title ?? string.Empty, req.VisitDate, req.ReminderIntervalDays, req.Content ?? string.Empty);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
