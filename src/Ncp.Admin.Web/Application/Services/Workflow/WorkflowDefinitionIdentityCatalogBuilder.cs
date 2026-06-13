using System.Text.Json;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;
using Ncp.Admin.Web.Application.Services.Workflow.Schemas;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 从设计器 Schema JSON 提取身份目录（用户/角色/部门/节点名称映射）。
/// </summary>
public class WorkflowDefinitionIdentityCatalogBuilder(WorkflowGraphCompiler graphCompiler)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public WorkflowDefinitionIdentityCatalog Build(string designerSchemaJson, string? category = null)
    {
        var compileResult = graphCompiler.Compile(designerSchemaJson, category);
        var schema = JsonSerializer.Deserialize<WorkflowDesignerSchema>(compileResult.DesignerSchemaJson, JsonOptions)
            ?? new WorkflowDesignerSchema();

        var users = new Dictionary<string, WorkflowDefinitionIdentityCatalogUserEntry>(StringComparer.Ordinal);
        var roles = new Dictionary<string, WorkflowDefinitionIdentityCatalogRoleEntry>(StringComparer.Ordinal);
        var depts = new Dictionary<string, WorkflowDefinitionIdentityCatalogDeptEntry>(StringComparer.Ordinal);

        foreach (var node in schema.Nodes)
        {
            foreach (var rule in node.AssigneeRules.Concat(node.CopyRules))
            {
                CollectUsers(rule.Users, users);
                CollectUsers(rule.ExcludeUsers, users);
                CollectUsers(rule.ExtraUsers, users);
                CollectRoles(rule.Roles, roles);
                CollectDepts(rule.Depts, depts);
                CollectDepts(rule.InitiatorDeptScope?.Depts, depts);
            }

            CollectUsers(node.EmptyApproverPolicy?.Users, users);
        }

        var nodes = schema.Nodes
            .Where(n => !string.IsNullOrWhiteSpace(n.NodeId))
            .Select(n => new WorkflowDefinitionIdentityCatalogNodeEntry
            {
                NodeId = n.NodeId,
                Name = n.Name ?? string.Empty,
                Type = n.Type ?? string.Empty,
            })
            .OrderBy(n => n.NodeId, StringComparer.Ordinal)
            .ToList();

        return new WorkflowDefinitionIdentityCatalog
        {
            Users = users.Values.OrderBy(x => x.Name, StringComparer.Ordinal).ToList(),
            Roles = roles.Values.OrderBy(x => x.Name, StringComparer.Ordinal).ToList(),
            Depts = depts.Values.OrderBy(x => x.Name, StringComparer.Ordinal).ToList(),
            Nodes = nodes,
        };
    }

    private static void CollectUsers(
        IReadOnlyList<WorkflowDesignerOption>? options,
        Dictionary<string, WorkflowDefinitionIdentityCatalogUserEntry> target)
    {
        if (options == null)
        {
            return;
        }

        foreach (var option in options)
        {
            if (string.IsNullOrWhiteSpace(option.Id) && string.IsNullOrWhiteSpace(option.Name))
            {
                continue;
            }

            var key = $"{option.Id}|{option.Name}";
            if (!target.TryGetValue(key, out var entry))
            {
                entry = new WorkflowDefinitionIdentityCatalogUserEntry
                {
                    ExportedId = option.Id ?? string.Empty,
                    Name = option.Name ?? string.Empty,
                };
                target[key] = entry;
            }
        }
    }

    private static void CollectRoles(
        IReadOnlyList<WorkflowDesignerOption>? options,
        Dictionary<string, WorkflowDefinitionIdentityCatalogRoleEntry> target)
    {
        if (options == null)
        {
            return;
        }

        foreach (var option in options)
        {
            if (string.IsNullOrWhiteSpace(option.Id) && string.IsNullOrWhiteSpace(option.Name))
            {
                continue;
            }

            var key = $"{option.Id}|{option.Name}";
            if (!target.ContainsKey(key))
            {
                target[key] = new WorkflowDefinitionIdentityCatalogRoleEntry
                {
                    ExportedId = option.Id ?? string.Empty,
                    Name = option.Name ?? string.Empty,
                };
            }
        }
    }

    private static void CollectDepts(
        IReadOnlyList<WorkflowDesignerOption>? options,
        Dictionary<string, WorkflowDefinitionIdentityCatalogDeptEntry> target)
    {
        if (options == null)
        {
            return;
        }

        foreach (var option in options)
        {
            if (string.IsNullOrWhiteSpace(option.Id) && string.IsNullOrWhiteSpace(option.Name))
            {
                continue;
            }

            var key = $"{option.Id}|{option.Name}";
            if (!target.ContainsKey(key))
            {
                target[key] = new WorkflowDefinitionIdentityCatalogDeptEntry
                {
                    ExportedId = option.Id ?? string.Empty,
                    Name = option.Name ?? string.Empty,
                };
            }
        }
    }
}
