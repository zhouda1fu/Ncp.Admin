using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Web.Application.Commands.Projects;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Projects;

/// <summary>
/// 移除项目跟进记录请求
/// </summary>
public record RemoveProjectFollowUpRecordRequest(ProjectId ProjectId, ProjectFollowUpRecordId RecordId);

/// <summary>
/// 移除项目跟进记录
/// </summary>
public class RemoveProjectFollowUpRecordEndpoint(IMediator mediator) : Endpoint<RemoveProjectFollowUpRecordRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Project");
        Description(b => b.AutoTagOverride("Project").WithSummary("移除项目跟进记录"));
        Delete("/api/admin/projects/{projectId}/follow-up-records/{recordId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectEdit);
    }

    public override async Task HandleAsync(RemoveProjectFollowUpRecordRequest req, CancellationToken ct)
    {
        await mediator.Send(new RemoveProjectFollowUpRecordCommand(req.ProjectId, req.RecordId), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
