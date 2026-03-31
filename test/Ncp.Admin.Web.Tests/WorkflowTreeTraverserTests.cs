using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Tests;

/// <summary>
/// WorkflowTreeTraverser 单元测试：各种树结构下的节点查找（含审批节点与抄送节点）。
/// </summary>
public class WorkflowTreeTraverserTests
{
    private readonly WorkflowTreeTraverser _traverser = new();

    [Fact]
    public void FindFirstTaskNode_EmptyJson_ReturnsNull()
    {
        var result = _traverser.FindFirstTaskNode(null);
        Assert.Null(result);

        result = _traverser.FindFirstTaskNode("");
        Assert.Null(result);
    }

    [Fact]
    public void FindFirstTaskNode_OnlyRootStartNode_ReturnsNull()
    {
        var json = """{"nodeName":"发起人","nodeKey":"root","type":0,"childNode":null}""";
        var result = _traverser.FindFirstTaskNode(json);
        Assert.Null(result);
    }

    [Fact]
    public void FindFirstTaskNode_ApprovalNodeAfterStart_ReturnsApprovalNode()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"部门审批","nodeKey":"approval1","type":1,"childNode":null}}
            """;
        var result = _traverser.FindFirstTaskNode(json);
        Assert.NotNull(result);
        Assert.Equal(1, result.Type);
        Assert.Equal("approval1", result.NodeKey);
        Assert.Equal("部门审批", result.NodeName);
    }

    [Fact]
    public void FindFirstTaskNode_CopyNodeBeforeApproval_ReturnsCopyNode()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"抄送","nodeKey":"cc1","type":2,
               "childNode":{"nodeName":"审批","nodeKey":"approval1","type":1,"childNode":null}}}
            """;
        var result = _traverser.FindFirstTaskNode(json);
        Assert.NotNull(result);
        Assert.Equal(2, result.Type);
        Assert.Equal("cc1", result.NodeKey);
    }

    [Fact]
    public void FindNextTaskNode_AfterApproval_ReturnsNextTaskNode()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"审批1","nodeKey":"a1","type":1,
               "childNode":{"nodeName":"抄送","nodeKey":"cc1","type":2,
                 "childNode":{"nodeName":"审批2","nodeKey":"a2","type":1,"childNode":null}}}}
            """;
        var next = _traverser.FindNextTaskNode(json, "a1", null);
        Assert.NotNull(next);
        Assert.Equal(2, next.Type);
        Assert.Equal("cc1", next.NodeKey);

        next = _traverser.FindNextTaskNode(json, "cc1", null);
        Assert.NotNull(next);
        Assert.Equal(1, next.Type);
        Assert.Equal("a2", next.NodeKey);
    }

    [Fact]
    public void FindNextTaskNode_AfterLastNode_ReturnsNull()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"审批","nodeKey":"a1","type":1,"childNode":null}}
            """;
        var next = _traverser.FindNextTaskNode(json, "a1", null);
        Assert.Null(next);
    }

    [Fact]
    public void FindNodeByKey_ExistingKey_ReturnsNode()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"审批","nodeKey":"a1","type":1,"childNode":null}}
            """;
        var node = _traverser.FindNodeByKey(json, "a1");
        Assert.NotNull(node);
        Assert.Equal("a1", node.NodeKey);
    }

    [Fact]
    public void FindFirstApprovalNode_ReturnsOnlyApprovalType()
    {
        var json = """
            {"nodeName":"发起人","nodeKey":"root","type":0,
             "childNode":{"nodeName":"抄送","nodeKey":"cc1","type":2,
               "childNode":{"nodeName":"审批","nodeKey":"a1","type":1,"childNode":null}}}
            """;
        var result = _traverser.FindFirstApprovalNode(json);
        Assert.NotNull(result);
        Assert.Equal(1, result.Type);
        Assert.Equal("a1", result.NodeKey);
    }
}
