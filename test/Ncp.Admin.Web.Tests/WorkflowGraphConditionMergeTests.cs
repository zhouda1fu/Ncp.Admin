using System.Text.Json;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;

namespace Ncp.Admin.Web.Tests;

/// <summary>
/// 条件分支汇聚测试：保护分支尾节点接回汇总后流程的运行图结构。
/// </summary>
public class WorkflowGraphConditionMergeTests
{
    [Fact]
    public void Compile_ConditionBranchTerminalNode_ShouldLinkToMergeNode()
    {
        var compiled = new WorkflowGraphCompiler().Compile(DesignerSchemaJson());
        var graph = JsonSerializer.Deserialize<WorkflowGraph>(compiled.GraphSnapshotJson, JsonOptions())!;

        var branchApproval = graph.Nodes.Single(n => n.NodeId == "approval1");
        Assert.Equal("after1", branchApproval.NextNodeId);
    }

    [Fact]
    public void CollectProgressSteps_OldSnapshotWithoutBranchTerminalNext_ShouldContinueToMergeNode()
    {
        var service = new WorkflowGraphRuntimeService();
        var steps = service.CollectProgressSteps(OldGraphSnapshotJson(), """{"amount":200}""");

        Assert.Equal(["发起人", "分支审批", "汇总后审批"], steps.Select(s => s.Title).ToArray());
    }

    [Fact]
    public void FindNextTaskNode_OldSnapshotBranchTerminal_ShouldUseImplicitMergeNode()
    {
        var service = new WorkflowGraphRuntimeService();
        var next = service.FindNextTaskNode(OldGraphSnapshotJson(), "approval1", """{"amount":200}""");

        Assert.NotNull(next);
        Assert.Equal("after1", next!.NodeId);
    }

    private static string DesignerSchemaJson() =>
        """
        {
          "startNodeId": "start",
          "nodes": [
            { "nodeId": "start", "name": "发起人", "type": "start", "nextNodeId": "route1" },
            {
              "nodeId": "route1",
              "name": "条件",
              "type": "conditionRoute",
              "mergeNodeId": "after1",
              "branches": [
                {
                  "branchId": "branch1",
                  "name": "金额大于 100",
                  "priority": 1,
                  "firstNodeId": "approval1",
                  "conditionGroups": [[{ "field": "amount", "operator": ">", "value": "100" }]]
                },
                {
                  "branchId": "branch2",
                  "name": "其他情况",
                  "priority": 2,
                  "conditionGroups": [],
                  "isFallback": true
                }
              ]
            },
            {
              "nodeId": "approval1",
              "name": "分支审批",
              "type": "approval",
              "assigneeRules": [
                { "ruleId": "r1", "source": "member", "users": [{ "id": "1", "name": "张三" }] }
              ]
            },
            {
              "nodeId": "after1",
              "name": "汇总后审批",
              "type": "approval",
              "assigneeRules": [
                { "ruleId": "r2", "source": "member", "users": [{ "id": "2", "name": "李四" }] }
              ]
            }
          ]
        }
        """;

    private static string OldGraphSnapshotJson() =>
        """
        {
          "startNodeId": "start",
          "nodes": [
            { "nodeId": "start", "name": "发起人", "type": 0, "nextNodeId": "route1" },
            {
              "nodeId": "route1",
              "name": "条件",
              "type": 3,
              "mergeNodeId": "after1",
              "branches": [
                {
                  "branchId": "branch1",
                  "name": "金额大于 100",
                  "priority": 1,
                  "firstNodeId": "approval1",
                  "conditionGroups": [[{ "field": "amount", "operator": ">", "value": "100" }]]
                },
                {
                  "branchId": "branch2",
                  "name": "其他情况",
                  "priority": 2,
                  "conditionGroups": [],
                  "isFallback": true
                }
              ]
            },
            { "nodeId": "approval1", "name": "分支审批", "type": 1 },
            { "nodeId": "after1", "name": "汇总后审批", "type": 1 }
          ]
        }
        """;

    private static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
