using System.Text.Json;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.Application.Services.Workflow.Schemas;

namespace Ncp.Admin.Web.Application.Services.Workflow.Graph;

public sealed record WorkflowGraphCompileResult(
    string DesignerSchemaJson,
    string GraphSnapshotJson);

/// <summary>
/// 将前端设计器 JSON 编译为后端运行图快照。
/// </summary>
public class WorkflowGraphCompiler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// 编译并校验流程定义。
    /// </summary>
    public WorkflowGraphCompileResult Compile(string definitionJson, string? category = null)
    {
        var schema = DeserializeSchema(definitionJson);
        NormalizeConditionBranchMergeLinks(schema);
        Validate(schema, category);

        var graph = new WorkflowGraph
        {
            StartNodeId = schema.StartNodeId,
            AllowAutoCompleteWithoutTasks = schema.AllowAutoCompleteWithoutTasks,
            Nodes = schema.Nodes.Select(ToGraphNode).ToList()
        };

        return new WorkflowGraphCompileResult(
            JsonSerializer.Serialize(schema, JsonOptions),
            JsonSerializer.Serialize(graph, JsonOptions));
    }

    /// <summary>
    /// 补齐条件分支内部流程到汇聚节点的连接，避免分支尾节点发布后成为断点。
    /// </summary>
    private static void NormalizeConditionBranchMergeLinks(WorkflowDesignerSchema schema)
    {
        var nodes = schema.Nodes
            .Where(n => !string.IsNullOrWhiteSpace(n.NodeId))
            .GroupBy(n => n.NodeId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        foreach (var route in schema.Nodes.Where(n => n.Type == WorkflowDesignerNodeTypes.ConditionRoute))
        {
            var mergeNodeId = route.MergeNodeId ?? route.NextNodeId;
            if (string.IsNullOrWhiteSpace(mergeNodeId))
            {
                continue;
            }

            foreach (var branch in route.Branches.Where(b => !string.IsNullOrWhiteSpace(b.FirstNodeId)))
            {
                // 分支内部可能有多级节点，沿每条路径找到尾节点后接回汇聚节点。
                LinkTerminalNodesToMerge(branch.FirstNodeId, mergeNodeId, nodes, []);
            }
        }
    }

    /// <summary>
    /// 从指定节点开始查找路径尾节点，并在缺少下一节点时接回指定汇聚节点。
    /// </summary>
    private static void LinkTerminalNodesToMerge(
        string? nodeId,
        string mergeNodeId,
        IReadOnlyDictionary<string, WorkflowDesignerNode> nodes,
        HashSet<string> visited)
    {
        if (string.IsNullOrWhiteSpace(nodeId)
            || string.Equals(nodeId, mergeNodeId, StringComparison.Ordinal)
            || !nodes.TryGetValue(nodeId, out var node)
            || !visited.Add(nodeId))
        {
            return;
        }

        if (node.Type == WorkflowDesignerNodeTypes.ConditionRoute)
        {
            var innerMergeNodeId = node.MergeNodeId ?? node.NextNodeId ?? mergeNodeId;
            foreach (var branch in node.Branches.Where(b => !string.IsNullOrWhiteSpace(b.FirstNodeId)))
            {
                LinkTerminalNodesToMerge(branch.FirstNodeId, innerMergeNodeId, nodes, new HashSet<string>(visited, StringComparer.Ordinal));
            }

            if (string.IsNullOrWhiteSpace(node.MergeNodeId) && string.IsNullOrWhiteSpace(node.NextNodeId))
            {
                node.NextNodeId = mergeNodeId;
                return;
            }

            LinkTerminalNodesToMerge(node.MergeNodeId ?? node.NextNodeId, mergeNodeId, nodes, visited);
            return;
        }

        if (string.IsNullOrWhiteSpace(node.NextNodeId))
        {
            node.NextNodeId = mergeNodeId;
            return;
        }

        LinkTerminalNodesToMerge(node.NextNodeId, mergeNodeId, nodes, visited);
    }

    /// <summary>
    /// 反序列化前端提交的设计器 JSON。
    /// </summary>
    private static WorkflowDesignerSchema DeserializeSchema(string definitionJson)
    {
        try
        {
            return JsonSerializer.Deserialize<WorkflowDesignerSchema>(definitionJson, JsonOptions)
                   ?? throw Invalid("流程定义 schema 不能为空");
        }
        catch (JsonException)
        {
            throw Invalid("流程定义 schema JSON 格式不正确");
        }
    }

    /// <summary>
    /// 校验流程定义的结构和关键业务规则。
    /// </summary>
    private static void Validate(WorkflowDesignerSchema schema, string? category)
    {
        if (string.IsNullOrWhiteSpace(schema.StartNodeId))
        {
            throw Invalid("流程缺少开始节点");
        }

        if (schema.Nodes.Count == 0)
        {
            throw Invalid("流程至少需要一个节点");
        }

        var nodeIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var node in schema.Nodes)
        {
            if (string.IsNullOrWhiteSpace(node.NodeId))
            {
                throw Invalid("流程节点缺少 nodeId");
            }

            if (!nodeIds.Add(node.NodeId))
            {
                throw Invalid($"流程定义存在重复节点：{node.NodeId}");
            }

            if (string.IsNullOrWhiteSpace(node.Name) && node.Type is not WorkflowDesignerNodeTypes.Start and not WorkflowDesignerNodeTypes.End)
            {
                throw Invalid($"流程节点「{node.NodeId}」名称不能为空");
            }
        }

        if (!nodeIds.Contains(schema.StartNodeId))
        {
            throw Invalid("开始节点不存在");
        }

        foreach (var node in schema.Nodes)
        {
            ValidateNodeReferences(node, nodeIds);
            ValidateNodeConfig(node, category);
        }

        if (!schema.AllowAutoCompleteWithoutTasks
            && !schema.Nodes.Any(n => n.Type is WorkflowDesignerNodeTypes.Approval or WorkflowDesignerNodeTypes.CarbonCopy))
        {
            throw Invalid("流程定义至少需要一个审批或抄送节点");
        }
    }

    private static void ValidateNodeReferences(WorkflowDesignerNode node, HashSet<string> nodeIds)
    {
        if (!string.IsNullOrWhiteSpace(node.NextNodeId) && !nodeIds.Contains(node.NextNodeId))
        {
            throw Invalid($"节点「{node.Name}」的下一节点不存在");
        }

        if (!string.IsNullOrWhiteSpace(node.MergeNodeId) && !nodeIds.Contains(node.MergeNodeId))
        {
            throw Invalid($"条件节点「{node.Name}」的汇聚节点不存在");
        }

        foreach (var branch in node.Branches)
        {
            if (!string.IsNullOrWhiteSpace(branch.FirstNodeId) && !nodeIds.Contains(branch.FirstNodeId))
            {
                throw Invalid($"条件分支「{branch.Name}」的首节点不存在");
            }
        }
    }

    private static void ValidateNodeConfig(WorkflowDesignerNode node, string? category)
    {
        switch (node.Type)
        {
            case WorkflowDesignerNodeTypes.Start:
            case WorkflowDesignerNodeTypes.End:
            case WorkflowDesignerNodeTypes.BusinessExtension:
                return;
            case WorkflowDesignerNodeTypes.Approval:
                if (node.AssigneeRules.Count == 0)
                {
                    throw Invalid($"审批节点「{node.Name}」请至少配置一个审批人规则");
                }

                ValidateAssigneeRules(node.AssigneeRules, node.Name, "审批");
                return;
            case WorkflowDesignerNodeTypes.CarbonCopy:
                ValidateAssigneeRules(node.CopyRules, node.Name, "抄送", allowEmpty: true);
                if (node.CopyRules.Any(r => r.Source == WorkflowAssigneeSources.OrderContractSigningCompanyResponsibleUser))
                {
                    throw Invalid("当前平台不支持合同签订公司负责人规则");
                }
                return;
            case WorkflowDesignerNodeTypes.ConditionRoute:
                if (node.Branches.Count == 0)
                {
                    throw Invalid($"条件节点「{node.Name}」至少需要一个分支");
                }

                if (node.Branches.All(b => !b.IsFallback && b.ConditionGroups.Count > 0))
                {
                    throw Invalid($"条件节点「{node.Name}」需要配置兜底分支");
                }

                return;
            default:
                throw Invalid($"无法识别流程节点类型：{node.Type}");
        }
    }

    private static void ValidateAssigneeRules(
        IReadOnlyList<WorkflowAssigneeRule> rules,
        string nodeName,
        string kind,
        bool allowEmpty = false)
    {
        if (rules.Count == 0)
        {
            if (allowEmpty)
            {
                return;
            }

            throw Invalid($"{kind}节点「{nodeName}」请至少配置一个人员规则");
        }

        foreach (var rule in rules)
        {
            switch (rule.Source)
            {
                case WorkflowAssigneeSources.Member:
                    if (rule.Users.Count == 0)
                    {
                        throw Invalid($"{kind}节点「{nodeName}」的指定成员规则缺少用户");
                    }
                    break;
                case WorkflowAssigneeSources.Role:
                    if (rule.Roles.Count == 0)
                    {
                        throw Invalid($"{kind}节点「{nodeName}」的角色规则缺少角色");
                    }

                    break;
                case WorkflowAssigneeSources.DeptResponsibleUser:
                    if (rule.Level <= 0)
                    {
                        throw Invalid($"{kind}节点「{nodeName}」的部门负责人层级必须大于0");
                    }
                    break;
                case WorkflowAssigneeSources.DeptResponsibleUserChain:
                    // 部门负责人链允许只配置排除/追加人员，基础链路由运行时按锚点人员解析。
                    break;
                case WorkflowAssigneeSources.Initiator:
                case WorkflowAssigneeSources.BusinessVariable:
                    break;
                case WorkflowAssigneeSources.OrderContractSigningCompanyResponsibleUser:
                    throw Invalid($"{kind}节点「{nodeName}」不支持合同签订公司负责人规则");
                default:
                    throw Invalid($"{kind}节点「{nodeName}」存在不支持的人员来源：{rule.Source}");
            }

            if (rule.Source is WorkflowAssigneeSources.Role
                && rule.InitiatorDeptScope?.Mode == WorkflowInitiatorDeptScopeNames.SpecifiedDeptAndSub
                && rule.InitiatorDeptScope.Depts.Count == 0)
            {
                throw Invalid($"{kind}节点「{nodeName}」选择额外发起部门时，请至少选择一个部门");
            }
        }
    }

    private static WorkflowGraphNode ToGraphNode(WorkflowDesignerNode node)
    {
        return new WorkflowGraphNode
        {
            NodeId = node.NodeId,
            Name = node.Name,
            Type = node.Type switch
            {
                WorkflowDesignerNodeTypes.Start => WorkflowGraphNodeType.Start,
                WorkflowDesignerNodeTypes.Approval => WorkflowGraphNodeType.Approval,
                WorkflowDesignerNodeTypes.CarbonCopy => WorkflowGraphNodeType.CarbonCopy,
                WorkflowDesignerNodeTypes.ConditionRoute => WorkflowGraphNodeType.ConditionRoute,
                WorkflowDesignerNodeTypes.End => WorkflowGraphNodeType.End,
                WorkflowDesignerNodeTypes.BusinessExtension => WorkflowGraphNodeType.BusinessExtension,
                _ => WorkflowGraphNodeType.BusinessExtension,
            },
            NextNodeId = node.NextNodeId,
            ApprovalMode = node.ApprovalMode switch
            {
                WorkflowApprovalModeNames.All => WorkflowGraphApprovalMode.All,
                WorkflowApprovalModeNames.Any => WorkflowGraphApprovalMode.Any,
                _ => WorkflowGraphApprovalMode.Sequential,
            },
            AssigneeRules = node.AssigneeRules.Select(ToGraphAssigneeRule).ToList(),
            CopyRules = node.CopyRules.Select(ToGraphAssigneeRule).ToList(),
            EmptyApproverPolicy = ToGraphEmptyApproverPolicy(node.EmptyApproverPolicy),
            SelfApprovalPolicy = ToGraphSelfApprovalPolicy(node.SelfApprovalPolicy),
            Branches = node.Branches.Select(ToGraphConditionBranch).ToList(),
            MergeNodeId = node.MergeNodeId,
            ExtensionsJson = node.Extensions == null ? "{}" : JsonSerializer.Serialize(node.Extensions, JsonOptions)
        };
    }

    private static WorkflowGraphAssigneeRule ToGraphAssigneeRule(WorkflowAssigneeRule rule)
    {
        return new WorkflowGraphAssigneeRule
        {
            RuleId = string.IsNullOrWhiteSpace(rule.RuleId) ? Guid.NewGuid().ToString("N") : rule.RuleId,
            Source = rule.Source switch
            {
                WorkflowAssigneeSources.Role => WorkflowGraphAssigneeSource.Role,
                WorkflowAssigneeSources.DeptResponsibleUser => WorkflowGraphAssigneeSource.DeptResponsibleUser,
                WorkflowAssigneeSources.DeptResponsibleUserChain => WorkflowGraphAssigneeSource.DeptResponsibleUserChain,
                WorkflowAssigneeSources.Initiator => WorkflowGraphAssigneeSource.Initiator,
                WorkflowAssigneeSources.BusinessVariable => WorkflowGraphAssigneeSource.BusinessVariable,
                WorkflowAssigneeSources.OrderContractSigningCompanyResponsibleUser => WorkflowGraphAssigneeSource.OrderContractSigningCompanyResponsibleUser,
                _ => WorkflowGraphAssigneeSource.Member,
            },
            Users = rule.Users.Select(x => new WorkflowGraphOption(x.Id, x.Name)).ToList(),
            Roles = rule.Roles.Select(x => new WorkflowGraphOption(x.Id, x.Name)).ToList(),
            Depts = rule.Depts.Select(x => new WorkflowGraphOption(x.Id, x.Name)).ToList(),
            // 排除和追加人员目前仅由部门负责人链规则消费，仍随规则快照一起编译到运行图。
            ExcludeUsers = rule.ExcludeUsers.Select(x => new WorkflowGraphOption(x.Id, x.Name)).ToList(),
            ExtraUsers = rule.ExtraUsers.Select(x => new WorkflowGraphOption(x.Id, x.Name)).ToList(),
            Level = rule.Level <= 0 ? 1 : rule.Level,
            InitiatorDeptScopeMode = rule.InitiatorDeptScope?.Mode switch
            {
                WorkflowInitiatorDeptScopeNames.All => WorkflowGraphInitiatorDeptScopeMode.All,
                WorkflowInitiatorDeptScopeNames.SpecifiedDeptAndSub => WorkflowGraphInitiatorDeptScopeMode.SpecifiedDeptAndSub,
                _ => WorkflowGraphInitiatorDeptScopeMode.DataPermission,
            },
            InitiatorDeptScopeDepts = rule.InitiatorDeptScope?.Depts.Select(x => new WorkflowGraphOption(x.Id, x.Name)).ToList() ?? []
        };
    }

    private static WorkflowGraphEmptyApproverPolicy ToGraphEmptyApproverPolicy(WorkflowEmptyApproverPolicySchema? policy)
    {
        return new WorkflowGraphEmptyApproverPolicy
        {
            Mode = policy?.Mode switch
            {
                WorkflowEmptyApproverPolicyNames.SpecifiedMembers => WorkflowGraphEmptyApproverPolicyMode.SpecifiedMembers,
                WorkflowEmptyApproverPolicyNames.WorkflowAdmin => WorkflowGraphEmptyApproverPolicyMode.WorkflowAdmin,
                _ => WorkflowGraphEmptyApproverPolicyMode.AutoPass,
            },
            Users = policy?.Users.Select(x => new WorkflowGraphOption(x.Id, x.Name)).ToList() ?? []
        };
    }

    private static WorkflowGraphSelfApprovalPolicy ToGraphSelfApprovalPolicy(WorkflowSelfApprovalPolicySchema? policy)
    {
        return new WorkflowGraphSelfApprovalPolicy
        {
            Mode = policy?.Mode switch
            {
                WorkflowSelfApprovalPolicyNames.AutoSkip => WorkflowGraphSelfApprovalPolicyMode.AutoSkip,
                WorkflowSelfApprovalPolicyNames.DirectResponsibleUser => WorkflowGraphSelfApprovalPolicyMode.DirectResponsibleUser,
                WorkflowSelfApprovalPolicyNames.DeptResponsibleUser => WorkflowGraphSelfApprovalPolicyMode.DeptResponsibleUser,
                _ => WorkflowGraphSelfApprovalPolicyMode.Allow,
            }
        };
    }

    private static WorkflowGraphConditionBranch ToGraphConditionBranch(WorkflowConditionBranchSchema branch)
    {
        return new WorkflowGraphConditionBranch
        {
            BranchId = branch.BranchId,
            Name = branch.Name,
            Priority = branch.Priority,
            ConditionGroups = branch.ConditionGroups,
            FirstNodeId = branch.FirstNodeId,
            IsFallback = branch.IsFallback
        };
    }

    private static KnownException Invalid(string message)
    {
        return new KnownException(message, ErrorCodes.WorkflowDefinitionInvalidAssigneeConfig);
    }
}
