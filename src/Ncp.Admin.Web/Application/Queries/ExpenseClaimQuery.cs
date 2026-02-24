using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.ExpenseAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 报销明细查询 DTO
/// </summary>
public record ExpenseItemQueryDto(ExpenseItemId Id, ExpenseType Type, decimal Amount, string Description, string? InvoiceUrl);

/// <summary>
/// 报销单查询 DTO（含明细列表）
/// </summary>
public record ExpenseClaimQueryDto(
    ExpenseClaimId Id,
    UserId ApplicantId,
    string ApplicantName,
    decimal TotalAmount,
    ExpenseClaimStatus Status,
    Guid? WorkflowInstanceId,
    DateTimeOffset CreatedAt,
    List<ExpenseItemQueryDto> Items);

/// <summary>
/// 报销单分页查询入参
/// </summary>
public class ExpenseClaimQueryInput : PageRequest
{
    /// <summary>
    /// 申请人ID筛选
    /// </summary>
    public UserId? ApplicantId { get; set; }
    /// <summary>
    /// 状态筛选
    /// </summary>
    public ExpenseClaimStatus? Status { get; set; }
}

/// <summary>
/// 报销单查询服务
/// </summary>
public class ExpenseClaimQuery(ApplicationDbContext dbContext) : IQuery
{
    /// <summary>
    /// 按 ID 查询报销单（含明细）
    /// </summary>
    public async Task<ExpenseClaimQueryDto?> GetByIdAsync(ExpenseClaimId id, CancellationToken cancellationToken = default)
    {
        var claim = await dbContext.ExpenseClaims
            .AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (claim == null) return null;
        return new ExpenseClaimQueryDto(
            claim.Id,
            claim.ApplicantId,
            claim.ApplicantName,
            claim.TotalAmount,
            claim.Status,
            claim.WorkflowInstanceId?.Id,
            claim.CreatedAt,
            claim.Items.Select(i => new ExpenseItemQueryDto(i.Id, i.Type, i.Amount, i.Description, i.InvoiceUrl)).ToList());
    }

    /// <summary>
    /// 分页查询报销单（含明细）
    /// </summary>
    public async Task<PagedData<ExpenseClaimQueryDto>> GetPagedAsync(ExpenseClaimQueryInput input, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ExpenseClaims.AsNoTracking();
        if (input.ApplicantId != null)
            query = query.Where(c => c.ApplicantId == input.ApplicantId);
        if (input.Status.HasValue)
            query = query.Where(c => c.Status == input.Status.Value);

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ExpenseClaimQueryDto(
                c.Id,
                c.ApplicantId,
                c.ApplicantName,
                c.TotalAmount,
                c.Status,
                c.WorkflowInstanceId != null ? c.WorkflowInstanceId.Id : (Guid?)null,
                c.CreatedAt,
                c.Items.Select(i => new ExpenseItemQueryDto(i.Id, i.Type, i.Amount, i.Description, i.InvoiceUrl)).ToList()))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
