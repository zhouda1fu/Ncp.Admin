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

    [Fact]
    public void EvaluateDesignerConditionList_StringEqualsAny_WhenCommaSeparated_ReturnsTrue()
    {
        var vars = """{"RoutingRoleId":"role-a"}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "RoutingRoleId", "==", "role-x, role-a, role-b") }
        };
        Assert.True(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_StringNotEqualsAny_WhenCommaSeparated_ReturnsTrue()
    {
        var vars = """{"RoutingRoleId":"role-a"}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "RoutingRoleId", "!=", "role-x, role-b") }
        };
        Assert.True(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_ApplicantDeptId_EqualsAny_WhenCommaSeparated_ReturnsTrue()
    {
        var vars = """{"ApplicantDeptId":"dept-a"}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "ApplicantDeptId", "==", "dept-x, dept-a, dept-b") }
        };
        Assert.True(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_StringEqualsAny_WhenNoneMatch_ReturnsFalse()
    {
        var vars = """{"RoutingRoleId":"role-c"}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "RoutingRoleId", "==", "role-a, role-b") }
        };
        Assert.False(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_CategoryDiscountPointsMissingCategory_TreatedAsZero_ForAndGroup()
    {
        var yang = "11111111-1111-1111-1111-111111111111";
        var wei = "22222222-2222-2222-2222-222222222222";
        var vars = "{\"CategoryDiscountPoints\":{\"" + yang + "\":5}}";
        var rules = new List<List<DesignerConditionRule>>
        {
            new()
            {
                new DesignerConditionRule(null, $"CategoryDiscountPoints.{yang}", ">", "2"),
                new DesignerConditionRule(null, $"CategoryDiscountPoints.{wei}", "==", "0"),
            },
        };
        Assert.True(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_CategoryDiscountPointsMissingCategory_GreaterThanZero_ReturnsFalse()
    {
        var wei = "22222222-2222-2222-2222-222222222222";
        var vars = """{"CategoryDiscountPoints":{}}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, $"CategoryDiscountPoints.{wei}", ">", "0") },
        };
        Assert.False(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_OfficeTaskOrderId_NotEmpty_WhenOrderLinked_ReturnsTrue()
    {
        var vars = """{"OrderId":"11111111-1111-1111-1111-111111111111","OrderNumber":"SO-2026-001"}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "OrderId", "==", "notempty") },
        };
        Assert.True(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_OfficeTaskOrderId_Empty_WhenNoOrder_ReturnsTrue()
    {
        var vars = """{"OrderId":"","OrderNumber":""}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "OrderId", "==", "empty") },
        };
        Assert.True(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_OfficeTaskOrderId_Empty_WhenOrderLinked_ReturnsFalse()
    {
        var vars = """{"OrderId":"11111111-1111-1111-1111-111111111111","OrderNumber":"SO-2026-001"}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "OrderId", "==", "empty") },
        };
        Assert.False(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_OfficeTaskOrderId_NotEmpty_WhenOnlyOrderNumberInVars_ReturnsFalse()
    {
        var vars = """{"OrderId":"","OrderNumber":"SO-2026-001"}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "OrderId", "==", "notempty") },
        };
        Assert.False(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }

    [Fact]
    public void EvaluateDesignerConditionList_OfficeTaskOrderNumberField_IsNotRecognized()
    {
        var vars = """{"OrderId":"11111111-1111-1111-1111-111111111111","OrderNumber":"SO-2026-001"}""";
        var rules = new List<List<DesignerConditionRule>>
        {
            new() { new DesignerConditionRule(null, "OrderNumber", "==", "notempty") },
        };
        Assert.False(WorkflowConditionEvaluator.EvaluateDesignerConditionList(vars, rules));
    }
}
