using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 流程定义查询DTO
/// </summary>
public record WorkflowDefinitionQueryDto(
    WorkflowDefinitionId Id,
    string Name,
    string Description,
    int Version,
    string Category,
    WorkflowDefinitionStatus Status,
    UserId CreatedBy,
    DateTimeOffset CreatedAt,
    IEnumerable<WorkflowNodeQueryDto> Nodes);

/// <summary>
/// 流程节点查询DTO
/// </summary>
public record WorkflowNodeQueryDto(
    WorkflowNodeId Id,
    string NodeName,
    WorkflowNodeType NodeType,
    AssigneeType AssigneeType,
    string AssigneeValue,
    int SortOrder,
    string Description,
    ApprovalMode ApprovalMode,
    string ConditionExpression,
    string TrueNextNodeName,
    string FalseNextNodeName);

/// <summary>
/// 流程定义查询输入
/// </summary>
public class WorkflowDefinitionQueryInput : PageRequest
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public WorkflowDefinitionStatus? Status { get; set; }
}

/// <summary>
/// 流程定义查询（与 RoleQuery 模式一致，注入 IMemoryCache 用于缓存）
/// </summary>
public class WorkflowDefinitionQuery(ApplicationDbContext applicationDbContext, IMemoryCache memoryCache) : IQuery
{
    private DbSet<WorkflowDefinition> DefinitionSet { get; } = applicationDbContext.WorkflowDefinitions;
    private static readonly TimeSpan DefinitionCacheExpiry = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan PublishedListCacheExpiry = TimeSpan.FromMinutes(3);

    /// <summary>
    /// 获取流程定义列表（分页）
    /// </summary>
    public async Task<PagedData<WorkflowDefinitionQueryDto>> GetAllDefinitionsAsync(
        WorkflowDefinitionQueryInput query, CancellationToken cancellationToken)
    {
        return await DefinitionSet.AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(query.Name), d => d.Name.Contains(query.Name!))
            .WhereIf(!string.IsNullOrWhiteSpace(query.Category), d => d.Category.Contains(query.Category!))
            .WhereIf(query.Status.HasValue, d => d.Status == query.Status)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new WorkflowDefinitionQueryDto(
                d.Id,
                d.Name,
                d.Description,
                d.Version,
                d.Category,
                d.Status,
                d.CreatedBy,
                d.CreatedAt,
                d.Nodes.OrderBy(n => n.SortOrder).Select(n => new WorkflowNodeQueryDto(
                    n.Id,
                    n.NodeName,
                    n.NodeType,
                    n.AssigneeType,
                    n.AssigneeValue,
                    n.SortOrder,
                    n.Description,
                    n.ApprovalMode,
                    n.ConditionExpression,
                    n.TrueNextNodeName,
                    n.FalseNextNodeName))))
            .ToPagedDataAsync(query, cancellationToken);
    }

    /// <summary>
    /// 根据ID获取流程定义（带缓存，与 RoleQuery.GetRoleByIdAsync 模式一致）
    /// </summary>
    public async Task<WorkflowDefinitionQueryDto?> GetDefinitionByIdAsync(
        WorkflowDefinitionId id, CancellationToken cancellationToken)
    {
        var cacheKey = WorkflowCacheKeys.DefinitionKey(id);

        return await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = DefinitionCacheExpiry;

            return await DefinitionSet.AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new WorkflowDefinitionQueryDto(
                    d.Id,
                    d.Name,
                    d.Description,
                    d.Version,
                    d.Category,
                    d.Status,
                    d.CreatedBy,
                    d.CreatedAt,
                    d.Nodes.OrderBy(n => n.SortOrder).Select(n => new WorkflowNodeQueryDto(
                        n.Id,
                        n.NodeName,
                        n.NodeType,
                        n.AssigneeType,
                        n.AssigneeValue,
                        n.SortOrder,
                        n.Description,
                        n.ApprovalMode,
                        n.ConditionExpression,
                        n.TrueNextNodeName,
                        n.FalseNextNodeName))))
                .FirstOrDefaultAsync(cancellationToken);
        });
    }

    /// <summary>
    /// 获取已发布的流程定义列表（供发起流程时选择），带短期缓存
    /// </summary>
    public async Task<List<WorkflowDefinitionQueryDto>> GetPublishedDefinitionsAsync(
        CancellationToken cancellationToken)
    {
        return (await memoryCache.GetOrCreateAsync(WorkflowCacheKeys.PublishedListKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = PublishedListCacheExpiry;
            return await DefinitionSet.AsNoTracking()
                .Where(d => d.Status == WorkflowDefinitionStatus.Published)
                .OrderBy(d => d.Category)
                .ThenBy(d => d.Name)
                .Select(d => new WorkflowDefinitionQueryDto(
                    d.Id,
                    d.Name,
                    d.Description,
                    d.Version,
                    d.Category,
                    d.Status,
                    d.CreatedBy,
                    d.CreatedAt,
                    d.Nodes.OrderBy(n => n.SortOrder).Select(n => new WorkflowNodeQueryDto(
                        n.Id,
                        n.NodeName,
                        n.NodeType,
                        n.AssigneeType,
                        n.AssigneeValue,
                        n.SortOrder,
                        n.Description,
                        n.ApprovalMode,
                        n.ConditionExpression,
                        n.TrueNextNodeName,
                        n.FalseNextNodeName))))
                .ToListAsync(cancellationToken);
        }))!;
    }
}
