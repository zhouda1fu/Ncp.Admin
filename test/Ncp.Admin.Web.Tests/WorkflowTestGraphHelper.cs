using System.Text.Json;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;

namespace Ncp.Admin.Web.Tests;

/// <summary>
/// 测试辅助：将旧版设计器树 JSON 转为 GraphSnapshotJson。
/// </summary>
internal static class WorkflowTestGraphHelper
{
    private static readonly JsonSerializerOptions LegacyTreeJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly JsonSerializerOptions GraphJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public static string ConvertLegacyTreeToGraphSnapshot(string definitionJson)
    {
        var root = JsonSerializer.Deserialize<LegacyDesignerTreeNode>(definitionJson, LegacyTreeJsonOptions)
            ?? throw new InvalidOperationException("测试流程定义 JSON 无效");
        var graph = new WorkflowGraph
        {
            StartNodeId = root.NodeKey,
            Nodes = [],
        };
        var visited = new HashSet<string>(StringComparer.Ordinal);
        Visit(root, graph.Nodes, visited);
        return JsonSerializer.Serialize(graph, GraphJsonOptions);
    }

    private static void Visit(LegacyDesignerTreeNode? node, List<WorkflowGraphNode> nodes, HashSet<string> visited)
    {
        if (node == null || string.IsNullOrWhiteSpace(node.NodeKey) || !visited.Add(node.NodeKey))
        {
            return;
        }

        var graphNode = new WorkflowGraphNode
        {
            NodeId = node.NodeKey,
            Name = node.NodeName,
            Type = node.Type switch
            {
                0 => WorkflowGraphNodeType.Start,
                1 => WorkflowGraphNodeType.Approval,
                2 => WorkflowGraphNodeType.CarbonCopy,
                4 => WorkflowGraphNodeType.ConditionRoute,
                _ => WorkflowGraphNodeType.End,
            },
            NextNodeId = node.ChildNode?.NodeKey,
            MergeNodeId = node.Type == 4 ? node.ChildNode?.NodeKey : null,
            ApprovalMode = node.ExamineMode switch
            {
                2 => WorkflowGraphApprovalMode.All,
                3 => WorkflowGraphApprovalMode.Any,
                _ => WorkflowGraphApprovalMode.Sequential,
            },
            ExtensionsJson = node.OfficeTaskParticipantNode
                ? """{"officeTask":{"participantNode":true}}"""
                : "{}",
        };
        graphNode.Branches = (node.ConditionNodes ?? [])
            .Select(branch => new WorkflowGraphConditionBranch
            {
                BranchId = branch.NodeKey,
                Name = branch.NodeName,
                Priority = branch.PriorityLevel,
                ConditionGroups = branch.ConditionList ?? [],
                FirstNodeId = branch.ChildNode?.NodeKey,
                IsFallback = branch.ConditionList == null || branch.ConditionList.Count == 0,
            })
            .ToList();
        nodes.Add(graphNode);

        foreach (var branch in node.ConditionNodes ?? [])
        {
            Visit(branch.ChildNode, nodes, visited);
            LinkBranchTerminalToMerge(branch.ChildNode, node.ChildNode?.NodeKey, nodes);
        }

        Visit(node.ChildNode, nodes, visited);
    }

    private static void LinkBranchTerminalToMerge(
        LegacyDesignerTreeNode? node,
        string? mergeNodeKey,
        List<WorkflowGraphNode> nodes)
    {
        if (node == null || string.IsNullOrWhiteSpace(mergeNodeKey))
        {
            return;
        }

        var current = node;
        while (current.ChildNode != null)
        {
            current = current.ChildNode;
        }

        var graphNode = nodes.FirstOrDefault(n => n.NodeId == current.NodeKey);
        if (graphNode != null && string.IsNullOrWhiteSpace(graphNode.NextNodeId))
        {
            graphNode.NextNodeId = mergeNodeKey;
        }
    }

    private sealed class LegacyDesignerTreeNode
    {
        public string NodeName { get; set; } = string.Empty;

        public string NodeKey { get; set; } = string.Empty;

        public int Type { get; set; }

        public int ExamineMode { get; set; }

        public bool OfficeTaskParticipantNode { get; set; }

        public int PriorityLevel { get; set; }

        public List<List<DesignerConditionRule>>? ConditionList { get; set; }

        public List<LegacyDesignerTreeNode>? ConditionNodes { get; set; }

        public LegacyDesignerTreeNode? ChildNode { get; set; }
    }
}
