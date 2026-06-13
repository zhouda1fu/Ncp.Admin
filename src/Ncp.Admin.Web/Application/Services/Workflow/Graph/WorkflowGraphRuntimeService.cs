using System.Text.Json;

namespace Ncp.Admin.Web.Application.Services.Workflow.Graph;

/// <summary>
/// 基于已发布 GraphSnapshotJson 的运行期路由服务。
/// </summary>
public class WorkflowGraphRuntimeService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// 从开始节点解析首个需要创建任务的节点。
    /// </summary>
    public WorkflowGraphNode? FindFirstTaskNode(string? graphSnapshotJson, string? variablesJson)
    {
        var graph = DeserializeGraph(graphSnapshotJson);
        return graph == null ? null : ResolveToNextGraphTaskNode(graph, graph.StartNodeId, variablesJson, []);
    }

    /// <summary>
    /// 从当前节点之后解析下一个需要创建任务的节点。
    /// </summary>
    public WorkflowGraphNode? FindNextTaskNode(
        string? graphSnapshotJson,
        string currentNodeId,
        string? variablesJson)
    {
        var graph = DeserializeGraph(graphSnapshotJson);
        if (graph == null || string.IsNullOrWhiteSpace(currentNodeId))
        {
            return null;
        }

        var nodes = ToNodeMap(graph);
        return !nodes.TryGetValue(currentNodeId, out var current)
            ? null
            : ResolveToNextGraphTaskNode(graph, current.NextNodeId, variablesJson, []);
    }

    /// <summary>
    /// 查找已发布运行图中的节点。
    /// </summary>
    public WorkflowGraphNode? FindNodeByKey(string? graphSnapshotJson, string nodeId)
    {
        var graph = DeserializeGraph(graphSnapshotJson);
        if (graph == null || string.IsNullOrWhiteSpace(nodeId))
        {
            return null;
        }

        var nodes = ToNodeMap(graph);
        return nodes.TryGetValue(nodeId, out var node) ? node : null;
    }

    /// <summary>
    /// 查找已发布运行图中的开始节点。
    /// </summary>
    /// <remarks>
    /// 开始节点本身通常不会生成待办；首个审批节点退回时需要把待办创建到开始节点，
    /// 让发起人修改业务单据后再按运行图从头推进，所以这里单独暴露开始节点查询。
    /// </remarks>
    public WorkflowGraphNode? FindStartNode(string? graphSnapshotJson)
    {
        var graph = DeserializeGraph(graphSnapshotJson);
        if (graph == null || string.IsNullOrWhiteSpace(graph.StartNodeId))
        {
            return null;
        }

        // 使用 StartNodeId 反查节点对象，而不是假设第一个节点就是开始节点，兼容发布快照中的节点顺序变化。
        var nodes = ToNodeMap(graph);
        return nodes.TryGetValue(graph.StartNodeId, out var node) ? node : null;
    }

    /// <summary>
    /// 在实例变量命中的运行路径上，查找当前节点之前最近的审批节点。
    /// </summary>
    public string? FindPreviousApprovalNodeKey(
        string? graphSnapshotJson,
        string? variablesJson,
        string currentNodeId)
    {
        var graph = DeserializeGraph(graphSnapshotJson);
        if (graph == null || string.IsNullOrWhiteSpace(currentNodeId))
        {
            return null;
        }

        var routeNodes = CollectResolvedRouteNodes(graph, variablesJson);
        var currentIndex = routeNodes.FindIndex(n =>
            string.Equals(n.NodeId, currentNodeId, StringComparison.Ordinal));
        if (currentIndex <= 0)
        {
            return null;
        }

        for (var i = currentIndex - 1; i >= 0; i--)
        {
            if (routeNodes[i].Type == WorkflowGraphNodeType.Approval)
            {
                return routeNodes[i].NodeId;
            }
        }

        return null;
    }

    /// <summary>
    /// 按实例变量解析条件分支后的进度步骤。
    /// </summary>
    public IReadOnlyList<WorkflowProgressStepItem> CollectProgressSteps(
        string? graphSnapshotJson,
        string? variablesJson)
    {
        var graph = DeserializeGraph(graphSnapshotJson);
        if (graph == null)
        {
            return [];
        }

        var steps = new List<WorkflowProgressStepItem>();
        CollectProgressStepsCore(graph, graph.StartNodeId, variablesJson, steps, []);
        return steps;
    }

    /// <summary>
    /// 运行图是否显式允许无任务自动完成。
    /// </summary>
    public bool AllowsAutoCompleteWithoutTasks(string? graphSnapshotJson) =>
        DeserializeGraph(graphSnapshotJson)?.AllowAutoCompleteWithoutTasks == true;

    private static WorkflowGraph? DeserializeGraph(string? graphSnapshotJson)
    {
        if (string.IsNullOrWhiteSpace(graphSnapshotJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<WorkflowGraph>(graphSnapshotJson, JsonOptions);
        }
        catch (JsonException)
        {
            throw new KnownException("流程运行图快照格式不正确", ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);
        }
    }

    private static Dictionary<string, WorkflowGraphNode> ToNodeMap(WorkflowGraph graph) =>
        graph.Nodes
            .Where(n => !string.IsNullOrWhiteSpace(n.NodeId))
            .GroupBy(n => n.NodeId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

    private static WorkflowGraphNode? ResolveToNextGraphTaskNode(
        WorkflowGraph graph,
        string? nodeId,
        string? variablesJson,
        HashSet<string> visited)
    {
        var nodes = ToNodeMap(graph);
        while (!string.IsNullOrWhiteSpace(nodeId) && nodes.TryGetValue(nodeId, out var node))
        {
            if (!visited.Add(node.NodeId))
            {
                throw new KnownException("流程运行图存在循环引用", ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);
            }

            if (node.Type is WorkflowGraphNodeType.Approval or WorkflowGraphNodeType.CarbonCopy)
            {
                return node;
            }

            nodeId = node.Type == WorkflowGraphNodeType.ConditionRoute
                ? ResolveConditionNextNodeId(node, variablesJson)
                : node.NextNodeId;
        }

        return null;
    }

    private static string? ResolveConditionNextNodeId(WorkflowGraphNode node, string? variablesJson)
    {
        foreach (var branch in node.Branches.OrderBy(b => b.Priority))
        {
            if (branch.IsFallback || branch.ConditionGroups.Count == 0)
            {
                return branch.FirstNodeId ?? node.MergeNodeId ?? node.NextNodeId;
            }

            if (WorkflowConditionEvaluator.EvaluateDesignerConditionList(variablesJson, branch.ConditionGroups))
            {
                return branch.FirstNodeId ?? node.MergeNodeId ?? node.NextNodeId;
            }
        }

        return node.MergeNodeId ?? node.NextNodeId;
    }

    private static void CollectProgressStepsCore(
        WorkflowGraph graph,
        string? nodeId,
        string? variablesJson,
        List<WorkflowProgressStepItem> steps,
        HashSet<string> visited)
    {
        var nodes = ToNodeMap(graph);
        while (!string.IsNullOrWhiteSpace(nodeId) && nodes.TryGetValue(nodeId, out var node))
        {
            if (!visited.Add(node.NodeId))
            {
                return;
            }

            if (node.Type is WorkflowGraphNodeType.Start or WorkflowGraphNodeType.Approval or WorkflowGraphNodeType.CarbonCopy)
            {
                var title = string.IsNullOrWhiteSpace(node.Name)
                    ? (node.Type == WorkflowGraphNodeType.Approval ? "审批" : "抄送")
                    : node.Name.Trim();
                steps.Add(new WorkflowProgressStepItem(title, node.NodeId));
            }

            nodeId = node.Type == WorkflowGraphNodeType.ConditionRoute
                ? ResolveConditionNextNodeId(node, variablesJson)
                : node.NextNodeId;
        }
    }

    private static List<WorkflowGraphNode> CollectResolvedRouteNodes(WorkflowGraph graph, string? variablesJson)
    {
        var nodes = ToNodeMap(graph);
        var routeNodes = new List<WorkflowGraphNode>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var nodeId = graph.StartNodeId;
        while (!string.IsNullOrWhiteSpace(nodeId) && nodes.TryGetValue(nodeId, out var node))
        {
            if (!visited.Add(node.NodeId))
            {
                throw new KnownException("流程运行图存在循环引用", ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);
            }

            routeNodes.Add(node);
            nodeId = node.Type == WorkflowGraphNodeType.ConditionRoute
                ? ResolveConditionNextNodeId(node, variablesJson)
                : node.NextNodeId;
        }

        return routeNodes;
    }
}
