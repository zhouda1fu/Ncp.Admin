using System.Text.Json;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;

namespace Ncp.Admin.Web.Tests;

/// <summary>
/// 运行图运行期查询测试。
/// </summary>
public class WorkflowGraphRuntimeServiceTests
{
    [Fact]
    public void FindStartNode_ReturnsPublishedStartNode()
    {
        // 首个审批节点退回发起人时，ReturnTaskCommand 依赖运行图开始节点作为待办目标节点。
        // 这个测试保护 FindStartNode 按 StartNodeId 精确反查，而不是依赖节点集合顺序。
        var graph = new WorkflowGraph
        {
            StartNodeId = "start",
            Nodes =
            [
                new WorkflowGraphNode
                {
                    NodeId = "start",
                    Name = "发起人",
                    Type = WorkflowGraphNodeType.Start,
                    NextNodeId = "approval1"
                },
                new WorkflowGraphNode
                {
                    NodeId = "approval1",
                    Name = "审批",
                    Type = WorkflowGraphNodeType.Approval
                }
            ]
        };
        var json = JsonSerializer.Serialize(graph, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var start = new WorkflowGraphRuntimeService().FindStartNode(json);

        Assert.NotNull(start);
        Assert.Equal("start", start!.NodeId);
        Assert.Equal("发起人", start.Name);
        Assert.Equal(WorkflowGraphNodeType.Start, start.Type);
    }
}
