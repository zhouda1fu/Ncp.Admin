using System.Text.Json;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow.Schemas;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 导入流程定义时，按名称将设计器中的用户/角色/部门 ID 重映射为当前库中的 ID。
/// </summary>
public class WorkflowDefinitionIdentityRemapper(
    WorkflowGraphCompiler graphCompiler,
    UserQuery userQuery,
    RoleQuery roleQuery,
    DeptQuery deptQuery)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<WorkflowDefinitionIdentityRemapResult> RemapAsync(
        string designerSchemaJson,
        string? category,
        CancellationToken cancellationToken)
    {
        var compileResult = graphCompiler.Compile(designerSchemaJson, category);
        var schema = JsonSerializer.Deserialize<WorkflowDesignerSchema>(compileResult.DesignerSchemaJson, JsonOptions)
            ?? throw new KnownException("流程定义 JSON 无效");

        var report = new WorkflowDefinitionIdentityRemapReport();
        var userIndex = await userQuery.BuildWorkflowRemapUserIndexAsync(cancellationToken);
        var roleIndex = await roleQuery.BuildWorkflowRemapRoleIndexAsync(cancellationToken);
        var deptIndex = await deptQuery.BuildWorkflowRemapDeptIndexAsync(cancellationToken);

        foreach (var node in schema.Nodes)
        {
            foreach (var rule in node.AssigneeRules.Concat(node.CopyRules))
            {
                RemapOptionList(rule.Users, RemapUserOption, userIndex, report);
                RemapOptionList(rule.ExcludeUsers, RemapUserOption, userIndex, report);
                RemapOptionList(rule.ExtraUsers, RemapUserOption, userIndex, report);
                RemapOptionList(rule.Roles, RemapRoleOption, roleIndex, report);
                RemapOptionList(rule.Depts, RemapDeptOption, deptIndex, report);
                if (rule.InitiatorDeptScope?.Depts != null)
                {
                    RemapOptionList(rule.InitiatorDeptScope.Depts, RemapDeptOption, deptIndex, report);
                }
            }

            if (node.EmptyApproverPolicy?.Users != null)
            {
                RemapOptionList(node.EmptyApproverPolicy.Users, RemapUserOption, userIndex, report);
            }
        }

        return new WorkflowDefinitionIdentityRemapResult
        {
            DesignerSchemaJson = JsonSerializer.Serialize(schema, JsonOptions),
            Report = report,
        };
    }

    private static void RemapOptionList<TIndex>(
        List<WorkflowDesignerOption> options,
        Func<WorkflowDesignerOption, TIndex, WorkflowDefinitionIdentityRemapReport, (WorkflowDesignerOption Option, bool Remapped, bool Kept, bool Unresolved)> remapOne,
        TIndex index,
        WorkflowDefinitionIdentityRemapReport report)
    {
        for (var i = 0; i < options.Count; i++)
        {
            var (mapped, remapped, kept, unresolved) = remapOne(options[i], index, report);
            options[i] = mapped;

            switch (index)
            {
                case WorkflowRemapUserIndex:
                    if (remapped)
                    {
                        report.UsersRemapped++;
                    }
                    else if (kept)
                    {
                        report.UsersKept++;
                    }
                    else if (unresolved)
                    {
                        report.UsersUnresolved++;
                    }

                    break;
                case WorkflowRemapRoleIndex:
                    if (remapped)
                    {
                        report.RolesRemapped++;
                    }
                    else if (kept)
                    {
                        report.RolesKept++;
                    }
                    else if (unresolved)
                    {
                        report.RolesUnresolved++;
                    }

                    break;
                case WorkflowRemapDeptIndex:
                    if (remapped)
                    {
                        report.DeptsRemapped++;
                    }
                    else if (kept)
                    {
                        report.DeptsKept++;
                    }
                    else if (unresolved)
                    {
                        report.DeptsUnresolved++;
                    }

                    break;
            }
        }
    }

    private static (WorkflowDesignerOption Option, bool Remapped, bool Kept, bool Unresolved) RemapUserOption(
        WorkflowDesignerOption option,
        WorkflowRemapUserIndex index,
        WorkflowDefinitionIdentityRemapReport report)
    {
        if (string.IsNullOrWhiteSpace(option.Name) && string.IsNullOrWhiteSpace(option.Id))
        {
            return (option, false, false, false);
        }

        if (index.TryResolveExisting(option.Id, out var existingId, out var existingName))
        {
            return (option with { Id = existingId, Name = existingName }, false, true, false);
        }

        if (index.TryResolveByName(option.Name, out var resolvedId, out var resolvedName, out var ambiguous))
        {
            if (ambiguous)
            {
                report.Warnings.Add($"用户「{option.Name}」存在重名，未能自动重映射，请导入后在设计器中手动调整");
                return (option, false, false, true);
            }

            return (option with { Id = resolvedId, Name = resolvedName }, true, false, false);
        }

        if (!string.IsNullOrWhiteSpace(option.Name))
        {
            report.Warnings.Add($"未找到用户「{option.Name}」，保留导出文件中的 ID（可能无效）");
        }

        return (option, false, false, true);
    }

    private static (WorkflowDesignerOption Option, bool Remapped, bool Kept, bool Unresolved) RemapRoleOption(
        WorkflowDesignerOption option,
        WorkflowRemapRoleIndex index,
        WorkflowDefinitionIdentityRemapReport report)
    {
        if (string.IsNullOrWhiteSpace(option.Name) && string.IsNullOrWhiteSpace(option.Id))
        {
            return (option, false, false, false);
        }

        if (index.TryResolveExisting(option.Id, out var existingId, out var existingName))
        {
            return (option with { Id = existingId, Name = existingName }, false, true, false);
        }

        var name = option.Name?.Trim() ?? string.Empty;
        if (name.Length > 0 && index.TryResolveByName(name, out var roleId, out var roleName))
        {
            return (option with { Id = roleId, Name = roleName }, true, false, false);
        }

        if (name.Length > 0)
        {
            report.Warnings.Add($"未找到角色「{name}」，保留导出文件中的 ID（可能无效）");
        }

        return (option, false, false, true);
    }

    private static (WorkflowDesignerOption Option, bool Remapped, bool Kept, bool Unresolved) RemapDeptOption(
        WorkflowDesignerOption option,
        WorkflowRemapDeptIndex index,
        WorkflowDefinitionIdentityRemapReport report)
    {
        if (string.IsNullOrWhiteSpace(option.Name) && string.IsNullOrWhiteSpace(option.Id))
        {
            return (option, false, false, false);
        }

        if (index.TryResolveExisting(option.Id, out var existingId, out var existingName))
        {
            return (option with { Id = existingId, Name = existingName }, false, true, false);
        }

        var name = option.Name?.Trim() ?? string.Empty;
        if (name.Length > 0 && index.TryResolveByName(name, out var deptId, out var deptName))
        {
            return (option with { Id = deptId, Name = deptName }, true, false, false);
        }

        if (name.Length > 0)
        {
            report.Warnings.Add($"未找到部门「{name}」或部门名称不唯一，保留导出文件中的 ID（可能无效）");
        }

        return (option, false, false, true);
    }
}

/// <summary>流程导入用户解析索引。</summary>
public sealed class WorkflowRemapUserIndex
{
    private readonly Dictionary<UserId, (string DisplayName, string AccountName)> _byId = [];

    private readonly Dictionary<string, List<(UserId Id, string DisplayName, string AccountName)>> _byDisplayName =
        new(StringComparer.Ordinal);

    private readonly Dictionary<string, List<(UserId Id, string DisplayName, string AccountName)>> _byAccountName =
        new(StringComparer.Ordinal);

    public void Add(UserId id, string displayName, string accountName)
    {
        _byId[id] = (displayName, accountName);
        AddToNameIndex(_byDisplayName, displayName, id, displayName, accountName);
        if (!string.IsNullOrWhiteSpace(accountName))
        {
            AddToNameIndex(_byAccountName, accountName, id, displayName, accountName);
        }
    }

    public bool TryResolveExisting(string? exportedId, out string resolvedId, out string resolvedName)
    {
        resolvedId = string.Empty;
        resolvedName = string.Empty;
        if (string.IsNullOrWhiteSpace(exportedId) || !long.TryParse(exportedId, out var value) || value <= 0)
        {
            return false;
        }

        var userId = new UserId(value);
        if (!_byId.TryGetValue(userId, out var info))
        {
            return false;
        }

        resolvedId = userId.ToString();
        resolvedName = info.DisplayName;
        return true;
    }

    public bool TryResolveByName(string? displayName, out string resolvedId, out string resolvedName, out bool ambiguous)
    {
        resolvedId = string.Empty;
        resolvedName = string.Empty;
        ambiguous = false;
        var trimmed = displayName?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return false;
        }

        if (_byDisplayName.TryGetValue(trimmed, out var byDisplay))
        {
            if (TryPickUnique(byDisplay, out var picked))
            {
                resolvedId = picked.Id.ToString();
                resolvedName = picked.DisplayName;
                return true;
            }

            if (byDisplay.Count > 1)
            {
                ambiguous = true;
                return false;
            }
        }

        if (_byAccountName.TryGetValue(trimmed, out var byAccount))
        {
            if (TryPickUnique(byAccount, out var picked))
            {
                resolvedId = picked.Id.ToString();
                resolvedName = picked.DisplayName;
                return true;
            }

            if (byAccount.Count > 1)
            {
                ambiguous = true;
            }
        }

        return false;
    }

    private static void AddToNameIndex(
        Dictionary<string, List<(UserId Id, string DisplayName, string AccountName)>> index,
        string key,
        UserId id,
        string displayName,
        string accountName)
    {
        if (!index.TryGetValue(key, out var list))
        {
            list = [];
            index[key] = list;
        }

        if (list.All(x => x.Id != id))
        {
            list.Add((id, displayName, accountName));
        }
    }

    private static bool TryPickUnique(
        List<(UserId Id, string DisplayName, string AccountName)> matches,
        out (UserId Id, string DisplayName, string AccountName) picked)
    {
        picked = default;
        if (matches.Count != 1)
        {
            return false;
        }

        picked = matches[0];
        return true;
    }
}

public sealed class WorkflowRemapRoleIndex
{
    private readonly Dictionary<RoleId, string> _byId = [];
    private readonly Dictionary<string, RoleId> _byName = new(StringComparer.Ordinal);

    public void Add(RoleId id, string name)
    {
        _byId[id] = name;
        if (!string.IsNullOrWhiteSpace(name) && !_byName.ContainsKey(name))
        {
            _byName[name] = id;
        }
    }

    public bool TryResolveExisting(string? exportedId, out string resolvedId, out string resolvedName)
    {
        resolvedId = string.Empty;
        resolvedName = string.Empty;
        if (string.IsNullOrWhiteSpace(exportedId) || !Guid.TryParse(exportedId, out var guid))
        {
            return false;
        }

        var roleId = new RoleId(guid);
        if (!_byId.TryGetValue(roleId, out var name))
        {
            return false;
        }

        resolvedId = roleId.ToString();
        resolvedName = name;
        return true;
    }

    public bool TryResolveByName(string name, out string resolvedId, out string resolvedName)
    {
        resolvedId = string.Empty;
        resolvedName = string.Empty;
        if (!_byName.TryGetValue(name, out var roleId))
        {
            return false;
        }

        resolvedId = roleId.ToString();
        resolvedName = _byId[roleId];
        return true;
    }
}

public sealed class WorkflowRemapDeptIndex
{
    private readonly Dictionary<DeptId, string> _byId = [];
    private readonly Dictionary<string, DeptId> _byName = new(StringComparer.Ordinal);

    public void Add(DeptId id, string name)
    {
        _byId[id] = name;
        if (!string.IsNullOrWhiteSpace(name) && !_byName.ContainsKey(name))
        {
            _byName[name] = id;
        }
    }

    public bool TryResolveExisting(string? exportedId, out string resolvedId, out string resolvedName)
    {
        resolvedId = string.Empty;
        resolvedName = string.Empty;
        if (string.IsNullOrWhiteSpace(exportedId) || !long.TryParse(exportedId, out var deptValue) || deptValue <= 0)
        {
            return false;
        }

        var deptId = new DeptId(deptValue);
        if (!_byId.TryGetValue(deptId, out var name))
        {
            return false;
        }

        resolvedId = deptId.ToString();
        resolvedName = name;
        return true;
    }

    public bool TryResolveByName(string name, out string resolvedId, out string resolvedName)
    {
        resolvedId = string.Empty;
        resolvedName = string.Empty;
        if (!_byName.TryGetValue(name, out var deptId))
        {
            return false;
        }

        resolvedId = deptId.ToString();
        resolvedName = _byId[deptId];
        return true;
    }
}
