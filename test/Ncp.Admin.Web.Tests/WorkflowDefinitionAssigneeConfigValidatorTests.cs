using Ncp.Admin.Domain;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;
using NetCorePal.Extensions.Primitives;

namespace Ncp.Admin.Web.Tests;

/// <summary>
/// 流程定义发布/保存校验测试：结构合法性与处理人配置。
/// </summary>
public class WorkflowDefinitionAssigneeConfigValidatorTests
{
    private readonly WorkflowDefinitionAssigneeConfigValidator _validator =
        new(new WorkflowGraphCompiler(), userQuery: null!);

    [Fact]
    public async Task ValidateAsync_WithInvalidJson_ShouldThrow()
    {
        var ex = await Assert.ThrowsAsync<KnownException>(() =>
            _validator.ValidateAsync("{ invalid json", TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig, ex.ErrorCode);
        Assert.Contains("JSON", ex.Message);
    }

    [Fact]
    public async Task ValidateAsync_WithDuplicateNodeId_ShouldThrow()
    {
        var json = """
            {
              "startNodeId": "start",
              "nodes": [
                { "nodeId": "start", "name": "发起人", "type": "start", "nextNodeId": "approval1" },
                {
                  "nodeId": "approval1",
                  "name": "审批1",
                  "type": "approval",
                  "assigneeRules": [
                    { "ruleId": "r1", "source": "member", "users": [{ "id": "1", "name": "张三" }] }
                  ]
                },
                {
                  "nodeId": "approval1",
                  "name": "审批2",
                  "type": "approval",
                  "assigneeRules": [
                    { "ruleId": "r2", "source": "member", "users": [{ "id": "2", "name": "李四" }] }
                  ]
                }
              ]
            }
            """;

        var ex = await Assert.ThrowsAsync<KnownException>(() =>
            _validator.ValidateAsync(json, TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig, ex.ErrorCode);
        Assert.Contains("重复", ex.Message);
    }

    [Fact]
    public async Task ValidateAsync_WithConditionRouteWithoutFallback_ShouldThrow()
    {
        var json = """
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
                      "conditionGroups": [[{ "field": "Amount", "operator": ">", "value": "100" }]]
                    }
                  ]
                },
                {
                  "nodeId": "approval1",
                  "name": "审批",
                  "type": "approval",
                  "assigneeRules": [
                    { "ruleId": "r1", "source": "member", "users": [{ "id": "1", "name": "张三" }] }
                  ]
                },
                { "nodeId": "after1", "name": "结束", "type": "end" }
              ]
            }
            """;

        var ex = await Assert.ThrowsAsync<KnownException>(() =>
            _validator.ValidateAsync(json, TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig, ex.ErrorCode);
        Assert.Contains("兜底", ex.Message);
    }

    [Fact]
    public async Task ValidateAsync_WithSimpleApprovalNode_ShouldPass()
    {
        var json = """
            {
              "startNodeId": "start",
              "nodes": [
                { "nodeId": "start", "name": "发起人", "type": "start", "nextNodeId": "approval1" },
                {
                  "nodeId": "approval1",
                  "name": "审批",
                  "type": "approval",
                  "assigneeRules": [
                    { "ruleId": "r1", "source": "member", "users": [{ "id": "1", "name": "张三" }] }
                  ]
                }
              ]
            }
            """;

        await _validator.ValidateAsync(json, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ValidateAsync_CopyNodeDeptResponsibleUser_NoAssigneeList_ShouldPass()
    {
        var json = """
            {
              "startNodeId": "start",
              "nodes": [
                { "nodeId": "start", "name": "发起人", "type": "start", "nextNodeId": "approval1" },
                {
                  "nodeId": "approval1",
                  "name": "审批",
                  "type": "approval",
                  "nextNodeId": "cc1",
                  "assigneeRules": [
                    { "ruleId": "r1", "source": "member", "users": [{ "id": "1", "name": "张三" }] }
                  ]
                },
                {
                  "nodeId": "cc1",
                  "name": "抄送人",
                  "type": "carbonCopy",
                  "copyRules": [
                    { "ruleId": "c1", "source": "deptResponsibleUser", "level": 1 }
                  ]
                }
              ]
            }
            """;

        await _validator.ValidateAsync(json, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ValidateAsync_ApprovalNodeDeptResponsibleUserChain_WithExcludeAndExtraUsers_ShouldPass()
    {
        var json = """
            {
              "startNodeId": "start",
              "nodes": [
                { "nodeId": "start", "name": "发起人", "type": "start", "nextNodeId": "approval1" },
                {
                  "nodeId": "approval1",
                  "name": "部门负责人链审批",
                  "type": "approval",
                  "assigneeRules": [
                    {
                      "ruleId": "r1",
                      "source": "deptResponsibleUserChain",
                      "excludeUsers": [{ "id": "3", "name": "C" }],
                      "extraUsers": [{ "id": "9", "name": "额外审批人" }]
                    }
                  ]
                }
              ]
            }
            """;

        await _validator.ValidateAsync(json, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ValidateAsync_RoleConfigWithSpecifiedInitiatorDeptButNoDept_ShouldThrow()
    {
        var roleId = Guid.NewGuid();
        var json = $$"""
            {
              "startNodeId": "start",
              "nodes": [
                { "nodeId": "start", "name": "发起人", "type": "start", "nextNodeId": "approval1" },
                {
                  "nodeId": "approval1",
                  "name": "审批",
                  "type": "approval",
                  "assigneeRules": [
                    {
                      "ruleId": "r1",
                      "source": "role",
                      "roles": [{ "id": "{{roleId}}", "name": "副总监A" }],
                      "initiatorDeptScope": { "mode": "specifiedDeptAndSub", "depts": [] }
                    }
                  ]
                }
              ]
            }
            """;

        var ex = await Assert.ThrowsAsync<KnownException>(() =>
            _validator.ValidateAsync(json, TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig, ex.ErrorCode);
        Assert.Contains("额外发起部门", ex.Message);
    }
}
