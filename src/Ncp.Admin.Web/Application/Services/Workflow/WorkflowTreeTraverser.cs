using System.Text.Json;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 遍历设计器树形 JSON，查找审批节点（用于发起流程与审批通过后流转）。
/// </summary>
public class WorkflowTreeTraverser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// 从流程根节点开始，沿 childNode/conditionNodes 遍历，找到第一个需要创建任务的节点（type=1 审批 或 type=2 抄送）。
    /// </summary>
    public DesignerNodeConfig? FindFirstTaskNode(string? definitionJson, string? variablesJson = null)
    {
        if (string.IsNullOrWhiteSpace(definitionJson)) return null;

        var root = DeserializeNode(definitionJson);
        if (root == null) return null;

        return ResolveToNextTaskNode(root, variablesJson, new HashSet<string>(StringComparer.Ordinal));
    }

    /// <summary>
    /// 从流程根节点开始，找到第一个需要创建任务的审批节点（type=1），跳过抄送节点。
    /// </summary>
    public DesignerNodeConfig? FindFirstApprovalNode(string? definitionJson, string? variablesJson = null)
    {
        var node = FindFirstTaskNode(definitionJson, variablesJson);
        while (node != null && !node.IsApprovalNode)
            node = FindNextTaskNode(definitionJson, node.NodeKey, variablesJson);
        return node;
    }

    /// <summary>
    /// 从当前节点之后查找下一个需要创建任务的节点（type=1 审批 或 type=2 抄送）。
    /// 条件分支场景会先解析“实例当前命中的单一路径”，再从路径中定位下一任务，
    /// 避免分支尾部空壳节点导致无法回到汇聚链。
    /// </summary>
    public DesignerNodeConfig? FindNextTaskNode(string? definitionJson, string currentNodeKey, string? variablesJson = null)
    {
        if (string.IsNullOrWhiteSpace(definitionJson) || string.IsNullOrWhiteSpace(currentNodeKey)) return null;

        var root = DeserializeNode(definitionJson);
        if (root == null) return null;

        var current = FindByKey(root, currentNodeKey);
        if (current == null) return null;

        var next = FindNextTaskNodeByResolvedRoute(root, currentNodeKey, variablesJson);
        return next;
    }

    /// <summary>
    /// 从当前节点之后查找下一个需要创建任务的审批节点（type=1），跳过抄送节点。
    /// </summary>
    public DesignerNodeConfig? FindNextApprovalNode(string? definitionJson, string currentNodeKey, string? variablesJson = null)
    {
        var node = FindNextTaskNode(definitionJson, currentNodeKey, variablesJson);
        while (node != null && !node.IsApprovalNode)
            node = FindNextTaskNode(definitionJson, node.NodeKey, variablesJson);
        return node;
    }

    /// <summary>
    /// 深度优先遍历整棵设计器树（含所有条件分支），不解析条件表达式；用于保存/发布时校验节点配置。
    /// </summary>
    public IReadOnlyList<DesignerNodeConfig> EnumerateAllNodes(string? definitionJson)
    {
        if (string.IsNullOrWhiteSpace(definitionJson))
        {
            return [];
        }

        var root = DeserializeNode(definitionJson);
        if (root == null)
        {
            return [];
        }

        var acc = new List<DesignerNodeConfig>();
        VisitAllNodesDepthFirst(root, acc);
        return acc;
    }

    private static void VisitAllNodesDepthFirst(DesignerNodeConfig node, List<DesignerNodeConfig> acc)
    {
        acc.Add(node);
        if (node.ChildNode != null)
        {
            VisitAllNodesDepthFirst(node.ChildNode, acc);
        }

        if (node.ConditionNodes == null)
        {
            return;
        }

        foreach (var branch in node.ConditionNodes)
        {
            VisitAllNodesDepthFirst(branch, acc);
        }
    }

    /// <summary>
    /// 按实例变量解析条件分支后，收集发起人/审批/抄送步骤（与 <see cref="ResolveToNextTaskNode"/> 分支规则一致）。
    /// </summary>
    public IReadOnlyList<WorkflowProgressStepItem> CollectProgressSteps(string? definitionJson, string? variablesJson)
    {
        if (string.IsNullOrWhiteSpace(definitionJson))
        {
            return [];
        }

        var root = DeserializeNode(definitionJson);
        if (root == null)
        {
            return [];
        }

        var list = new List<WorkflowProgressStepItem>();
        var visited = new HashSet<DesignerNodeConfig>();
        CollectProgressStepsCore(root, variablesJson, list, visited, stopAtMergeKey: null);
        return list;
    }

    /// <param name="stopAtMergeKey">
    /// 条件分支汇入点 nodeKey；反序列化后汇入可能在树中出现不同对象实例，故用 key 判定停在汇入点之前，再由路由统一走入汇聚链。
    /// </param>
    private static void CollectProgressStepsCore(
        DesignerNodeConfig? node,
        string? variablesJson,
        List<WorkflowProgressStepItem> steps,
        HashSet<DesignerNodeConfig> visited,
        string? stopAtMergeKey)
    {
        if (node == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(stopAtMergeKey)
            && !string.IsNullOrWhiteSpace(node.NodeKey)
            && string.Equals(node.NodeKey.Trim(), stopAtMergeKey, StringComparison.Ordinal))
        {
            return;
        }

        if (!visited.Add(node))
        {
            return;
        }

        switch (node.Type)
        {
            case 4:
                var merge = node.ChildNode;
                var mergeKey = string.IsNullOrWhiteSpace(merge?.NodeKey) ? null : merge.NodeKey.Trim();
                var next = ResolveConditionBranch(node, variablesJson);
                if (next is not null)
                {
                    CollectProgressStepsCore(next, variablesJson, steps, visited, mergeKey);
                }

                if (merge is not null)
                {
                    CollectProgressStepsCore(merge, variablesJson, steps, visited, stopAtMergeKey: null);
                }

                return;
            case 3:
                CollectProgressStepsCore(node.ChildNode, variablesJson, steps, visited, stopAtMergeKey);
                return;
            case 1:
            case 2:
            {
                var raw = node.NodeName?.Trim();
                var title = string.IsNullOrEmpty(raw)
                    ? (node.Type == 1 ? "审批" : "抄送")
                    : raw;
                var key = string.IsNullOrWhiteSpace(node.NodeKey) ? null : node.NodeKey.Trim();
                steps.Add(new WorkflowProgressStepItem(title, key));
                CollectProgressStepsCore(node.ChildNode, variablesJson, steps, visited, stopAtMergeKey);
                return;
            }
            case 0:
            {
                var raw = node.NodeName?.Trim();
                if (!string.IsNullOrEmpty(raw))
                {
                    var key = string.IsNullOrWhiteSpace(node.NodeKey) ? null : node.NodeKey.Trim();
                    steps.Add(new WorkflowProgressStepItem(raw, key));
                }

                CollectProgressStepsCore(node.ChildNode, variablesJson, steps, visited, stopAtMergeKey);
                return;
            }
            default:
                CollectProgressStepsCore(node.ChildNode, variablesJson, steps, visited, stopAtMergeKey);
                return;
        }
    }

    /// <summary>
    /// 按 nodeKey 在树中查找节点。
    /// </summary>
    public DesignerNodeConfig? FindNodeByKey(string? definitionJson, string nodeKey)
    {
        if (string.IsNullOrWhiteSpace(definitionJson) || string.IsNullOrWhiteSpace(nodeKey)) return null;

        var root = DeserializeNode(definitionJson);
        if (root == null) return null;

        return FindByKey(root, nodeKey);
    }

    private static DesignerNodeConfig? DeserializeNode(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<DesignerNodeConfig>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 解析到下一个需要创建任务的节点：type=1 审批 或 type=2 抄送。
    /// </summary>
    private static DesignerNodeConfig? ResolveToNextTaskNode(
        DesignerNodeConfig node,
        string? variablesJson,
        HashSet<string> visited)
    {
        if (string.IsNullOrEmpty(node.NodeKey))
        {
            if (node.Type == 4)
            {
                var nextByCondition = ResolveConditionBranch(node, variablesJson);
                if (nextByCondition != null)
                {
                    return ResolveToNextTaskNode(nextByCondition, variablesJson, visited);
                }

                if (node.ChildNode != null)
                {
                    return ResolveToNextTaskNode(node.ChildNode, variablesJson, visited);
                }

                return null;
            }

            if (node.ChildNode != null)
            {
                return ResolveToNextTaskNode(node.ChildNode, variablesJson, visited);
            }

            return null;
        }

        if (visited.Contains(node.NodeKey))
        {
            return null;
        }
        visited.Add(node.NodeKey);

        // type=1 审批节点，type=2 抄送节点，均需创建任务
        if (node.Type == 1 || node.Type == 2)
            return node;

        if (node.Type == 4) // 条件路由
        {
            var next = ResolveConditionBranch(node, variablesJson);
            if (next != null)
                return ResolveToNextTaskNode(next, variablesJson, visited);
            if (node.ChildNode != null)
                return ResolveToNextTaskNode(node.ChildNode, variablesJson, visited);
            return null;
        }

        // type 0 发起人, 3 条件分支项：继续沿 childNode
        if (node.ChildNode != null)
            return ResolveToNextTaskNode(node.ChildNode, variablesJson, visited);

        return null;
    }

    private static DesignerNodeConfig? ResolveConditionBranch(DesignerNodeConfig branchNode, string? variablesJson)
    {
        var conditionNodes = branchNode.ConditionNodes;
        if (conditionNodes == null || conditionNodes.Count == 0) return null;

        // 按 priorityLevel 升序，先匹配到的分支生效
        var ordered = conditionNodes.OrderBy(c => c.PriorityLevel).ToList();

        foreach (var cn in ordered)
        {
            var list = cn.ConditionList;
            // 空条件列表视为“其他情况”兜底
            if (list == null || list.Count == 0)
            {
                return cn.ChildNode;
            }
            var matched = WorkflowConditionEvaluator.EvaluateDesignerConditionList(variablesJson, list);

            if (matched)
            {
                return cn.ChildNode;
            }
        }

        return null;
    }

    private static DesignerNodeConfig? FindNextTaskNodeByResolvedRoute(
        DesignerNodeConfig root,
        string currentNodeKey,
        string? variablesJson)
    {
        // 复用进度路径解析思路：先拿到当前实例真正执行的节点序列，再向后找任务节点。
        var routeNodes = new List<DesignerNodeConfig>();
        var visited = new HashSet<DesignerNodeConfig>();
        CollectResolvedRouteNodes(root, variablesJson, routeNodes, visited, stopAtMergeKey: null);

        var currentIndex = routeNodes.FindIndex(n =>
            string.Equals(n.NodeKey, currentNodeKey, StringComparison.Ordinal));
        if (currentIndex < 0)
        {
            return null;
        }

        for (var i = currentIndex + 1; i < routeNodes.Count; i++)
        {
            var node = routeNodes[i];
            if (node.Type == 1 || node.Type == 2)
            {
                return node;
            }
        }

        return null;
    }

    private static void CollectResolvedRouteNodes(
        DesignerNodeConfig? node,
        string? variablesJson,
        List<DesignerNodeConfig> nodes,
        HashSet<DesignerNodeConfig> visited,
        string? stopAtMergeKey)
    {
        if (node == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(stopAtMergeKey)
            && !string.IsNullOrWhiteSpace(node.NodeKey)
            && string.Equals(node.NodeKey.Trim(), stopAtMergeKey, StringComparison.Ordinal))
        {
            return;
        }

        if (!visited.Add(node))
        {
            return;
        }

        switch (node.Type)
        {
            case 4:
            {
                // 条件路由：先沿命中分支下探到汇聚前，再继续走汇聚后的主链。
                var merge = node.ChildNode;
                var mergeKey = string.IsNullOrWhiteSpace(merge?.NodeKey) ? null : merge.NodeKey.Trim();
                var next = ResolveConditionBranch(node, variablesJson);
                if (next != null)
                {
                    CollectResolvedRouteNodes(next, variablesJson, nodes, visited, mergeKey);
                }

                if (merge != null)
                {
                    CollectResolvedRouteNodes(merge, variablesJson, nodes, visited, stopAtMergeKey: null);
                }

                return;
            }
            case 3:
                CollectResolvedRouteNodes(node.ChildNode, variablesJson, nodes, visited, stopAtMergeKey);
                return;
            default:
                nodes.Add(node);
                CollectResolvedRouteNodes(node.ChildNode, variablesJson, nodes, visited, stopAtMergeKey);
                return;
        }
    }

    private static DesignerNodeConfig? FindByKey(DesignerNodeConfig node, string key)
    {
        if (string.Equals(node.NodeKey, key, StringComparison.Ordinal))
            return node;

        if (node.ChildNode != null)
        {
            var inChild = FindByKey(node.ChildNode, key);
            if (inChild != null) return inChild;
        }

        if (node.ConditionNodes != null)
        {
            foreach (var cn in node.ConditionNodes)
            {
                var inBranch = FindByKey(cn, key);
                if (inBranch != null) return inBranch;
            }
        }

        return null;
    }
}
