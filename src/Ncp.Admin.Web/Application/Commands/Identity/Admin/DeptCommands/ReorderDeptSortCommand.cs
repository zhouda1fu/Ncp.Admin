using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.DeptCommands;

/// <summary>
/// 重排同级部门排序命令
/// </summary>
/// <param name="ParentId">父级部门 ID；未分配表示顶级部门</param>
/// <param name="OrderedIds">同级部门按新顺序排列的 ID 列表</param>
public record ReorderDeptSortCommand(DeptId ParentId, IReadOnlyList<DeptId> OrderedIds) : ICommand<bool>;

/// <summary>
/// 重排同级部门排序校验
/// </summary>
public class ReorderDeptSortCommandValidator : AbstractValidator<ReorderDeptSortCommand>
{
    public ReorderDeptSortCommandValidator()
    {
        RuleFor(x => x.OrderedIds).NotEmpty();
    }
}

/// <summary>
/// 重排同级部门排序处理器
/// </summary>
public class ReorderDeptSortCommandHandler(IDeptRepository deptRepository)
    : ICommandHandler<ReorderDeptSortCommand, bool>
{
    public async Task<bool> Handle(ReorderDeptSortCommand request, CancellationToken cancellationToken)
    {
        var ordered = request.OrderedIds;
        if (ordered.Count != ordered.Distinct().Count())
        {
            throw new KnownException("排序列表存在重复项", ErrorCodes.DeptReorderInvalid);
        }

        var parentId = request.ParentId;
        var siblings = await deptRepository.GetActiveSiblingsAsync(parentId, cancellationToken);
        var siblingMap = siblings.ToDictionary(d => d.Id);

        if (ordered.Any(id => !siblingMap.ContainsKey(id)))
        {
            throw new KnownException("排序列表包含非同级部门", ErrorCodes.DeptReorderInvalid);
        }

        const int step = 10;
        for (var i = 0; i < ordered.Count; i++)
        {
            siblingMap[ordered[i]].SetSortOrder((i + 1) * step);
        }

        var nextOrder = (ordered.Count + 1) * step;
        foreach (var dept in siblings
                     .Where(d => !ordered.Contains(d.Id))
                     .OrderBy(d => d.SortOrder)
                     .ThenBy(d => d.CreatedAt))
        {
            dept.SetSortOrder(nextOrder);
            nextOrder += step;
        }

        return true;
    }
}
