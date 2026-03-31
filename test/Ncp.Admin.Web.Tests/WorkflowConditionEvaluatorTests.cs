using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Tests;

/// <summary>
/// WorkflowConditionEvaluator 单元测试：数值/字符串/布尔比较及 include/notinclude。
/// </summary>
public class WorkflowConditionEvaluatorTests
{
    [Fact]
    public void EvaluateDesignerConditionList_NullOrEmpty_ReturnsFalse()
    {
        Assert.False(WorkflowConditionEvaluator.EvaluateDesignerConditionList(null, null));
        Assert.False(WorkflowConditionEvaluator.EvaluateDesignerConditionList("{}", new List<List<DesignerConditionRule>>()));
    }

    [Fact]
    public void EvaluateDesignerConditionList_NumericEquals_ReturnsTrue()
    {
        var vars = """{"days":3}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "days", "==", "3") }
        };
        Assert.True(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_NumericGreaterThan_ReturnsTrue()
    {
        var vars = """{"amount":100}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "amount", ">", "50") }
        };
        Assert.True(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_StringEquals_ReturnsTrue()
    {
        var vars = """{"type":"leave"}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "type", "==", "leave") }
        };
        Assert.True(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_Include_ReturnsTrue()
    {
        var vars = """{"reason":"事假请假"}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "reason", "include", "事假") }
        };
        Assert.True(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_NotInclude_ReturnsTrue()
    {
        var vars = """{"reason":"年假"}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "reason", "notinclude", "事假") }
        };
        Assert.True(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_NotInclude_WhenContains_ReturnsFalse()
    {
        var vars = """{"reason":"事假请假"}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "reason", "notinclude", "事假") }
        };
        Assert.False(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_BoolEquals_ReturnsTrue()
    {
        var vars = """{"urgent":true}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "urgent", "==", "true") }
        };
        Assert.True(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_GroupAnd_AllMustMatch()
    {
        var vars = """{"days":3,"type":"leave"}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new()
            {
                new DesignerConditionRule(null, "days", ">=", "1"),
                new DesignerConditionRule(null, "type", "==", "leave")
            }
        };
        Assert.True(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_GroupOr_OneMatchSuffices()
    {
        var vars = """{"type":"leave"}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "type", "==", "order") },
            new() { new DesignerConditionRule(null, "type", "==", "leave") }
        };
        Assert.True(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }
}
