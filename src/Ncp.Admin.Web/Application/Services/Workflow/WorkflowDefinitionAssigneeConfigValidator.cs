using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;
using System.Text.Json;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 保存/发布流程定义时校验审批/抄送节点配置。
/// 结构校验委托 <see cref="WorkflowGraphCompiler"/>，此处仅补充需查库的业务校验。
/// </summary>
public class WorkflowDefinitionAssigneeConfigValidator(WorkflowGraphCompiler graphCompiler, UserQuery userQuery)
{
    private static readonly JsonSerializerOptions GraphJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public Task ValidateAsync(string? definitionJson, CancellationToken cancellationToken = default) =>
        ValidateAsync(definitionJson, category: null, cancellationToken);

    public async Task ValidateAsync(string? definitionJson, string? category, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(definitionJson))
        {
            throw InvalidDefinition("流程定义不能为空，请先在设计器中配置流程节点");
        }

        var compileResult = graphCompiler.Compile(definitionJson, category);
        var graph = JsonSerializer.Deserialize<WorkflowGraph>(compileResult.GraphSnapshotJson, GraphJsonOptions)
            ?? throw InvalidDefinition("流程定义未包含有效节点，请检查设计器配置");

        ValidateOfficeTaskParticipantNodes(graph);
        ValidateEmptyApproverPolicies(graph);
        await ValidateRoleMembershipAsync(graph, cancellationToken);
    }

    private static void ValidateOfficeTaskParticipantNodes(WorkflowGraph graph)
    {
        var nodes = graph.Nodes.ToDictionary(n => n.NodeId, StringComparer.Ordinal);
        foreach (var node in graph.Nodes.Where(n => n.OfficeTaskParticipantNode()))
        {
            if (node.Type != WorkflowGraphNodeType.Approval)
            {
                throw InvalidDefinition($"办公任务参与人节点「{DisplayName(node)}」必须是审批节点");
            }

            if (string.IsNullOrWhiteSpace(node.NextNodeId)
                || !nodes.TryGetValue(node.NextNodeId, out var next)
                || next.Type != WorkflowGraphNodeType.CarbonCopy)
            {
                throw InvalidDefinition($"办公任务参与人节点「{DisplayName(node)}」后必须紧跟一个抄送节点");
            }
        }
    }

    private static void ValidateEmptyApproverPolicies(WorkflowGraph graph)
    {
        foreach (var node in graph.Nodes.Where(n => n.Type == WorkflowGraphNodeType.Approval))
        {
            if (node.EmptyApproverPolicy.Mode == WorkflowGraphEmptyApproverPolicyMode.SpecifiedMembers
                && !HasAtLeastOneParsableUserId(node.EmptyApproverPolicy.Users))
            {
                throw new KnownException(
                    $"审批节点「{node.Name}」审批人为空时指定成员缺少有效用户",
                    ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);
            }
        }
    }

    private async Task ValidateRoleMembershipAsync(WorkflowGraph graph, CancellationToken cancellationToken)
    {
        foreach (var node in graph.Nodes)
        {
            var rules = node.Type switch
            {
                WorkflowGraphNodeType.Approval => node.AssigneeRules,
                WorkflowGraphNodeType.CarbonCopy => node.CopyRules,
                _ => [],
            };

            var kind = node.Type == WorkflowGraphNodeType.CarbonCopy ? "抄送" : "审批";
            foreach (var rule in rules.Where(r => r.Source == WorkflowGraphAssigneeSource.Role))
            {
                await EnsureEachRoleHasUsersAsync(rule.Roles, node.Name, kind, cancellationToken);
            }
        }
    }

    private static string DisplayName(WorkflowGraphNode node) =>
        string.IsNullOrWhiteSpace(node.Name) ? node.NodeId : node.Name;

    private static KnownException InvalidDefinition(string message) =>
        new(message, ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);

    private static bool HasAtLeastOneParsableUserId(IReadOnlyList<WorkflowGraphOption>? list)
    {
        if (list == null || list.Count == 0)
        {
            return false;
        }

        return list.Any(item => !string.IsNullOrWhiteSpace(item.Id) && long.TryParse(item.Id, out _));
    }

    private async Task EnsureEachRoleHasUsersAsync(
        IReadOnlyList<WorkflowGraphOption>? list,
        string nodeName,
        string kind,
        CancellationToken cancellationToken)
    {
        if (list == null || list.Count == 0)
        {
            throw new KnownException(
                $"{kind}节点「{nodeName}」请选择角色",
                ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);
        }

        var parsedAny = false;
        foreach (var item in list)
        {
            if (string.IsNullOrWhiteSpace(item.Id) || !Guid.TryParse(item.Id, out var roleGuid))
            {
                continue;
            }

            parsedAny = true;
            var roleId = new RoleId(roleGuid);
            var users = await userQuery.GetUserAssigneesByRoleIdAsync(roleId, cancellationToken);
            if (users.Count == 0)
            {
                var roleLabel = string.IsNullOrWhiteSpace(item.Name) ? item.Id : item.Name;
                throw new KnownException(
                    $"{kind}节点「{nodeName}」中的角色「{roleLabel}」下暂无成员，请分配用户后再保存",
                    ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);
            }
        }

        if (!parsedAny)
        {
            throw new KnownException(
                $"{kind}节点「{nodeName}」请选择有效的角色",
                ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);
        }
    }
}
