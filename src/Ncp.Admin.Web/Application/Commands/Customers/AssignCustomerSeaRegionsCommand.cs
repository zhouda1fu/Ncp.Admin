using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.CustomerSeaRegionAssignmentAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Infrastructure.Services;
using NetCorePal.Context;
using System.Text;

namespace Ncp.Admin.Web.Application.Commands.Customers;

public record AssignCustomerSeaRegionsCommand(
    UserId TargetUserId,
    IReadOnlyList<RegionId>? SelectedRegionIds,
    UserId OperatorUserId) : ICommand<bool>;

public class AssignCustomerSeaRegionsCommandValidator : AbstractValidator<AssignCustomerSeaRegionsCommand>
{
    public AssignCustomerSeaRegionsCommandValidator()
    {
        RuleFor(x => x.TargetUserId).NotEmpty();
        RuleFor(x => x.OperatorUserId).NotEmpty();
    }
}

public class AssignCustomerSeaRegionsCommandHandler(
    ICustomerSeaRegionAssignmentRepository assignmentRepository,
    ICustomerSeaRegionAssignmentAuditRepository auditRepository,
    DeptQuery deptQuery,
    RegionQuery regionQuery,
    UserQuery userQuery,
    ApplicationDbContext dbContext,
    IContextAccessor contextAccessor,
    IMediator mediator)
    : ICommandHandler<AssignCustomerSeaRegionsCommand, bool>
{
    private static void DebugLog(string content)
    {
        try
        {
            var path = Path.Combine(Path.GetTempPath(), "sea-region-assign-permission-debug.log");
            File.AppendAllText(path, content + Environment.NewLine, Encoding.UTF8);
        }
        catch
        {
            // ignore
        }
    }

    public async Task<bool> Handle(AssignCustomerSeaRegionsCommand request, CancellationToken cancellationToken)
    {
        var targetUserId = request.TargetUserId;

        // 数据权限上下文用于调试/展示口径；保存动作不做数据权限越权阻断
        var dataPermission = contextAccessor.GetContext<DataPermissionContext>();

        var marketingCenter = await deptQuery.GetDeptByExactNameAsync("营销中心", cancellationToken)
            ?? throw new KnownException("未找到营销中心部门，请检查部门名称配置/数据", ErrorCodes.CustomerSeaRegionAssignMarketingCenterDeptNotFound);

        var marketingCenterDeptIds = await deptQuery.GetAllChildDeptIdsAsync(marketingCenter.Id, cancellationToken);

        // 越权校验：目标用户必须属于营销中心及其下级部门
        var targetUserDeptId = await dbContext.Users.AsNoTracking()
            .Where(u => u.Id == targetUserId)
            .Select(u => u.Dept != null ? u.Dept.DeptId : new DeptId(0))
            .SingleOrDefaultAsync(cancellationToken);

        if (targetUserDeptId is null)
            throw new KnownException("未找到目标用户", ErrorCodes.UserNotFound);

        if (!marketingCenterDeptIds.Contains(targetUserDeptId))
            throw new KnownException("目标用户不在营销中心及其下级部门范围内", ErrorCodes.CustomerSeaRegionAssignUserOutOfMarketingCenter);

        var operatorScope = dataPermission?.Scope.ToString() ?? "null";
        var operatorAuthorizedDeptIds = (dataPermission?.AuthorizedDeptIds ?? []).Select(d => d.Id).ToList();
        DebugLog(
            $"AssignCustomerSeaRegions: operatorUserId={request.OperatorUserId.Id}, operatorScope={operatorScope}, operatorAuthorizedDeptIds=[{string.Join(",", operatorAuthorizedDeptIds)}], targetUserId={targetUserId.Id}, targetDeptId={(targetUserDeptId?.Id.ToString() ?? "null")}");

        // 越权校验：目标用户必须在当前登录人的数据权限可见范围内。
        // 说明：菜单“片区管理-分配”属于功能权限，允许管理员对任意目标人员进行保存；
        // 数据权限仅用于限制“页面展示的片区数据来源”（抽屉提示/回显），不阻断保存动作。
       
        var allRegions = await dbContext.Regions.AsNoTracking()
            .Select(r => new { r.Id, r.ParentId, r.Name })
            .ToListAsync(cancellationToken);
        var regionMap = allRegions.ToDictionary(x => x.Id, x => x.Name);

        var selected = (request.SelectedRegionIds ?? []).Distinct().ToList();
        if (selected.Count > 0 && selected.Any(id => !regionMap.ContainsKey(id)))
            throw new KnownException("选择的地区不存在", ErrorCodes.RegionNotFound);

        var expanded = ExpandRegionsWithDescendants(selected, allRegions.Select(x => (x.Id, x.ParentId)).ToList());
        var expandedSet = expanded.ToHashSet();

        // 读取当前绑定（含 Regions）
        var assignment = await assignmentRepository.GetByTargetUserIdWithRegionsAsync(targetUserId, cancellationToken)
                          ?? new CustomerSeaRegionAssignment(targetUserId);

        if (assignment.Id == default)
            await assignmentRepository.AddAsync(assignment, cancellationToken);

        // 当前已绑定地区
        var currentSet = assignment.Regions.Select(r => r.RegionId).ToHashSet();

        // 差集：用于审计明细
        var added = expandedSet.Except(currentSet).ToList();
        var removed = currentSet.Except(expandedSet).ToList();

        // 领域更新：仅增删差集，避免 Clear + Add 导致 23505
        assignment.UpdateRegionIds(expandedSet);

        // 审计：仅当新增或删除不为空时写入
        if (added.Count > 0 || removed.Count > 0)
        {
            var opUser = await userQuery.GetUserByIdAsync(request.OperatorUserId, cancellationToken);
            var details = new List<(RegionId RegionId, string RegionName, CustomerSeaRegionAssignmentAuditChangeType ChangeType)>();
            foreach (var id in added)
                details.Add((id, regionMap[id], CustomerSeaRegionAssignmentAuditChangeType.Added));
            foreach (var id in removed)
                details.Add((id, regionMap[id], CustomerSeaRegionAssignmentAuditChangeType.Removed));

            var audit = new CustomerSeaRegionAssignmentAudit(targetUserId, request.OperatorUserId, opUser.RealName ?? opUser.Name, details);
            await auditRepository.AddAsync(audit, cancellationToken);

            await mediator.Send(new RecalculateCustomerSeaVisibilityBatchCommand(), cancellationToken);
        }

        return true;
    }

    private static IReadOnlyList<RegionId> ExpandRegionsWithDescendants(
        IReadOnlyList<RegionId> selected,
        IReadOnlyList<(RegionId Id, RegionId ParentId)> allRegions)
    {
        var childrenMap = allRegions
            .GroupBy(x => x.ParentId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

        var expanded = new HashSet<RegionId>();
        var stack = new Stack<RegionId>();

        foreach (var s in selected)
        {
            if (expanded.Add(s))
                stack.Push(s);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!childrenMap.TryGetValue(current, out var children))
                    continue;

                foreach (var child in children)
                {
                    if (expanded.Add(child))
                        stack.Push(child);
                }
            }
        }

        return expanded.ToList();
    }
}

