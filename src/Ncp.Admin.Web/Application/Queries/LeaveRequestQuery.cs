using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 请假申请查询DTO
/// </summary>
public record LeaveRequestQueryDto(
    LeaveRequestId Id,
    UserId ApplicantId,
    string ApplicantName,
    LeaveType LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Days,
    string Reason,
    LeaveRequestStatus Status,
    Guid? WorkflowInstanceId,
    DateTimeOffset CreatedAt);

/// <summary>
/// 请假申请查询
/// </summary>
public class LeaveRequestQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<LeaveRequestQueryDto?> GetByIdAsync(LeaveRequestId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.LeaveRequests
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new LeaveRequestQueryDto(
                r.Id,
                r.ApplicantId,
                r.ApplicantName,
                r.LeaveType,
                r.StartDate,
                r.EndDate,
                r.Days,
                r.Reason,
                r.Status,
                r.WorkflowInstanceId != null ? r.WorkflowInstanceId.Id : (Guid?)null,
                r.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedData<LeaveRequestQueryDto>> GetPagedAsync(
        LeaveRequestQueryInput input,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.LeaveRequests.AsNoTracking();
        if (input.ApplicantId != null)
            query = query.Where(r => r.ApplicantId == input.ApplicantId);
        if (input.Status.HasValue)
            query = query.Where(r => r.Status == input.Status.Value);
        if (input.LeaveType.HasValue)
            query = query.Where(r => r.LeaveType == input.LeaveType.Value);
        if (input.StartDateFrom.HasValue)
            query = query.Where(r => r.StartDate >= input.StartDateFrom!.Value);
        if (input.StartDateTo.HasValue)
            query = query.Where(r => r.StartDate <= input.StartDateTo!.Value);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new LeaveRequestQueryDto(
                r.Id,
                r.ApplicantId,
                r.ApplicantName,
                r.LeaveType,
                r.StartDate,
                r.EndDate,
                r.Days,
                r.Reason,
                r.Status,
                r.WorkflowInstanceId != null ? r.WorkflowInstanceId.Id : (Guid?)null,
                r.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}

/// <summary>
/// 请假申请查询入参
/// </summary>
public class LeaveRequestQueryInput : PageRequest
{
    public UserId? ApplicantId { get; set; }
    public LeaveRequestStatus? Status { get; set; }
    public LeaveType? LeaveType { get; set; }
    public DateOnly? StartDateFrom { get; set; }
    public DateOnly? StartDateTo { get; set; }
}
