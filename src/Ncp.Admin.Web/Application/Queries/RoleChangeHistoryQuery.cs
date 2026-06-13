using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.OperationLogAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Infrastructure;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>角色字段变更历史行。</summary>
public record RoleFieldChangeRowDto(
    string FieldKey,
    string OldDisplay,
    string NewDisplay,
    string OperatorUserName,
    DateTimeOffset ChangedAt);

/// <summary>角色字段变更历史（由操作日志相邻版本对比得到）。</summary>
public sealed class RoleChangeHistoryQuery(ApplicationDbContext dbContext) : IQuery
{
    public Task<bool> ExistsAsync(RoleId roleId, CancellationToken cancellationToken = default) =>
        dbContext.Roles.AsNoTracking().AnyAsync(r => r.Id == roleId, cancellationToken);

    public async Task<PagedData<RoleFieldChangeRowDto>> GetFieldChangeHistoryPagedAsync(
        RoleId roleId,
        PageRequest input,
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        var rid = roleId.Id.ToString("D");
        var ridLower = rid.ToLowerInvariant();

        var updates = (await dbContext.OperationLogs.AsNoTracking()
            .Where(o =>
                o.IsSuccess
                && o.OperationType == OperationLogType.Update
                && o.RequestMethod == "PUT"
                && o.RequestPath.ToLower().Contains("/api/admin/roles/update")
                && o.RequestBody != null)
            .OrderBy(o => o.CreatedAt)
            .Select(o => new { o.RequestBody, o.OperatorUserName, o.CreatedAt })
            .ToListAsync(cancellationToken))
            .Where(o => BodyBelongsToRole(o.RequestBody, roleId))
            .ToList();

        var batchPermissionLogs = (await dbContext.OperationLogs.AsNoTracking()
            .Where(o =>
                o.IsSuccess
                && o.OperationType == OperationLogType.Update
                && o.RequestMethod == "PUT"
                && o.RequestPath.ToLower().Contains("/api/admin/roles/permissions/batch")
                && o.RequestBody != null)
            .OrderBy(o => o.CreatedAt)
            .Select(o => new { o.RequestBody, o.OperatorUserName, o.CreatedAt })
            .ToListAsync(cancellationToken))
            .Where(o => BatchBodyContainsRole(o.RequestBody, roleId))
            .ToList();

        var activateLogs = (await dbContext.OperationLogs.AsNoTracking()
            .Where(o =>
                o.IsSuccess
                && o.OperationType == OperationLogType.Update
                && o.RequestMethod == "PUT"
                && o.RequestPath.ToLower().Contains("/api/admin/roles/activate")
                && o.RequestBody != null)
            .OrderBy(o => o.CreatedAt)
            .Select(o => new { o.RequestBody, o.OperatorUserName, o.CreatedAt })
            .ToListAsync(cancellationToken))
            .Where(o => BodyBelongsToRole(o.RequestBody, roleId))
            .ToList();

        var deactivateLogs = (await dbContext.OperationLogs.AsNoTracking()
            .Where(o =>
                o.IsSuccess
                && o.OperationType == OperationLogType.Update
                && o.RequestMethod == "PUT"
                && o.RequestPath.ToLower().Contains("/api/admin/roles/deactivate")
                && o.RequestBody != null)
            .OrderBy(o => o.CreatedAt)
            .Select(o => new { o.RequestBody, o.OperatorUserName, o.CreatedAt })
            .ToListAsync(cancellationToken))
            .Where(o => BodyBelongsToRole(o.RequestBody, roleId))
            .ToList();

        string? createBody = null;
        var createCandidates = await dbContext.OperationLogs.AsNoTracking()
            .Where(o =>
                o.IsSuccess
                && o.OperationType == OperationLogType.Create
                && o.RequestMethod == "POST"
                && o.RequestPath.ToLower().EndsWith("/api/admin/roles"))
            .OrderByDescending(o => o.CreatedAt)
            .Take(80)
            .Select(o => new { o.RequestBody, o.ResponseBody })
            .ToListAsync(cancellationToken);

        foreach (var c in createCandidates)
        {
            if (TryGetRoleIdFromCreateResponse(c.ResponseBody, out var respId)
                && string.Equals(respId, rid, StringComparison.OrdinalIgnoreCase))
            {
                createBody = c.RequestBody;
                break;
            }
        }

        var deptNameById = await LoadDeptNamesAsync(cancellationToken);
        var rows = new List<RoleFieldChangeRowDto>();
        var prevBody = createBody;
        foreach (var u in updates)
        {
            rows.AddRange(DiffRoleSnapshots(prevBody, u.RequestBody!, u.OperatorUserName ?? string.Empty, u.CreatedAt, deptNameById));
            prevBody = u.RequestBody;
        }

        foreach (var u in activateLogs)
        {
            rows.Add(new RoleFieldChangeRowDto(
                "isActive",
                "停用",
                "启用",
                u.OperatorUserName ?? string.Empty,
                u.CreatedAt));
        }

        foreach (var u in deactivateLogs)
        {
            rows.Add(new RoleFieldChangeRowDto(
                "isActive",
                "启用",
                "停用",
                u.OperatorUserName ?? string.Empty,
                u.CreatedAt));
        }

        foreach (var u in batchPermissionLogs)
        {
            var display = FormatBatchPermissionDisplay(u.RequestBody);
            if (display == "—")
                continue;

            rows.Add(new RoleFieldChangeRowDto(
                "permissionCodes",
                "—",
                display,
                u.OperatorUserName ?? string.Empty,
                u.CreatedAt));
        }

        var deleteInfo = await dbContext.OperationLogs.AsNoTracking()
            .Where(o =>
                o.IsSuccess
                && o.OperationType == OperationLogType.Delete
                && o.RequestMethod == "DELETE"
                && o.RequestPath.ToLower().EndsWith("/" + ridLower))
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new { o.OperatorUserName, o.CreatedAt })
            .FirstOrDefaultAsync(cancellationToken);

        if (deleteInfo != null)
        {
            rows.Add(new RoleFieldChangeRowDto(
                "_roleDeleted",
                "—",
                "已删除",
                deleteInfo.OperatorUserName ?? string.Empty,
                deleteInfo.CreatedAt));
        }

        rows.Sort((a, b) => b.ChangedAt.CompareTo(a.ChangedAt));
        rows = await ChangeHistoryOperatorDisplayHelper.HydrateRoleRowsAsync(dbContext, rows, cancellationToken);
        return Paginate(rows, input, keyword);
    }

    private async Task<Dictionary<string, string>> LoadDeptNamesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Depts.AsNoTracking()
            .Select(d => new { Id = d.Id.Id, d.Name })
            .ToDictionaryAsync(
                x => x.Id.ToString(CultureInfo.InvariantCulture),
                x => x.Name,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);
    }

    private static PagedData<RoleFieldChangeRowDto> Paginate(
        List<RoleFieldChangeRowDto> rows,
        PageRequest input,
        string? keyword)
    {
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            rows = rows
                .Where(r =>
                    r.FieldKey.Contains(k, StringComparison.OrdinalIgnoreCase)
                    || r.OldDisplay.Contains(k, StringComparison.OrdinalIgnoreCase)
                    || r.NewDisplay.Contains(k, StringComparison.OrdinalIgnoreCase)
                    || r.OperatorUserName.Contains(k, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var total = rows.Count;
        var pageIndex = input.PageIndex < 1 ? 1 : input.PageIndex;
        var pageSize = input.PageSize < 1 ? 20 : input.PageSize;
        var slice = rows.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        return new PagedData<RoleFieldChangeRowDto>(slice, total, pageIndex, pageSize);
    }

    private static bool TryGetRoleIdFromCreateResponse(string? responseBody, out string id)
    {
        id = string.Empty;
        if (string.IsNullOrWhiteSpace(responseBody))
            return false;
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (!ChangeHistoryJson.TryGetProperty(doc.RootElement, "data", out var data)
                || data.ValueKind != JsonValueKind.Object)
                return false;
            if (!ChangeHistoryJson.TryGetProperty(data, "roleId", out var idEl))
                return false;
            return ChangeHistoryJson.TryReadGuidElement(idEl, out id);
        }
        catch
        {
            return false;
        }
    }

    private static bool BodyBelongsToRole(string? requestBody, RoleId roleId)
    {
        if (TryGetRequestRoleId(requestBody, out var id) && id == roleId)
            return true;

        var rid = roleId.Id.ToString("D");
        return !string.IsNullOrWhiteSpace(requestBody)
            && requestBody.Contains(rid, StringComparison.OrdinalIgnoreCase);
    }

    private static bool BatchBodyContainsRole(string? requestBody, RoleId roleId)
    {
        if (string.IsNullOrWhiteSpace(requestBody))
            return false;

        var rid = roleId.Id.ToString("D");
        if (requestBody.Contains(rid, StringComparison.OrdinalIgnoreCase))
            return true;

        using var doc = ChangeHistoryJson.TryParse(requestBody);
        if (doc == null)
            return false;

        var root = ChangeHistoryJson.GetPropertyBagRoot(doc.RootElement);
        if (!ChangeHistoryJson.TryGetProperty(root, "roleIds", out var roleIdsEl))
            return false;

        return RoleChangeHistory.ExtractGuidSet(roleIdsEl).Contains(rid, StringComparer.OrdinalIgnoreCase);
    }

    private static bool TryGetRequestRoleId(string? requestBody, out RoleId id)
    {
        id = default!;
        if (string.IsNullOrWhiteSpace(requestBody))
            return false;
        try
        {
            using var doc = JsonDocument.Parse(requestBody);
            var root = ChangeHistoryJson.GetPropertyBagRoot(doc.RootElement);
            if (!ChangeHistoryJson.TryGetProperty(root, "roleId", out var el))
                return false;
            if (ChangeHistoryJson.TryReadGuidElement(el, out var guidStr) && Guid.TryParse(guidStr, out var g))
            {
                id = new RoleId(g);
                return true;
            }
        }
        catch
        {
            // 截断/损坏 JSON 时走正则兜底
        }

        return TryExtractRoleIdFromRaw(requestBody, out id);
    }

    private static bool TryExtractRoleIdFromRaw(string? requestBody, out RoleId id)
    {
        id = default!;
        if (string.IsNullOrWhiteSpace(requestBody))
            return false;

        var match = System.Text.RegularExpressions.Regex.Match(
            requestBody,
            "\"roleId\"\\s*:\\s*(?:\"([0-9a-fA-F-]{36})\"|\\{\\s*\"id\"\\s*:\\s*\"([0-9a-fA-F-]{36})\"\\s*\\})",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        if (!match.Success)
            return false;

        var guidStr = !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value;
        if (!Guid.TryParse(guidStr, out var g))
            return false;

        id = new RoleId(g);
        return true;
    }

    private static string FormatBatchPermissionDisplay(string? requestBody)
    {
        using var doc = ChangeHistoryJson.TryParse(requestBody);
        if (doc == null)
            return "—";

        var root = ChangeHistoryJson.GetPropertyBagRoot(doc.RootElement);
        var operation = 0;
        if (ChangeHistoryJson.TryGetProperty(root, "operation", out var opEl)
            && ChangeHistoryJson.TryCoerceInt32(opEl, out var op))
        {
            operation = op;
        }

        var codes = PermissionCodesChangeHistoryHelper.ExtractPermissionCodes(doc, requestBody);
        var names = PermissionCodesChangeHistoryHelper.FormatPermissionCodesDisplay(codes);
        if (names == "—")
            return "—";

        return operation == 1 ? $"移除：{names}" : $"追加：{names}";
    }

    private static IReadOnlyList<RoleFieldChangeRowDto> DiffRoleSnapshots(
        string? previousRequestBody,
        string currentRequestBody,
        string operatorUserName,
        DateTimeOffset changedAt,
        IReadOnlyDictionary<string, string> deptNameById)
    {
        var list = new List<RoleFieldChangeRowDto>();
        JsonDocument? prevDoc = null;
        JsonDocument? currDoc = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(previousRequestBody))
                prevDoc = ChangeHistoryJson.TryParse(previousRequestBody);
            currDoc = ChangeHistoryJson.TryParse(currentRequestBody);

            foreach (var key in RoleChangeHistory.TrackedFieldKeys)
            {
                if (key == "permissionCodes")
                {
                    var (oldDisp, newDisp) = PermissionCodesChangeHistoryHelper.FormatPermissionDiff(
                        prevDoc, previousRequestBody, currDoc, currentRequestBody);
                    if (oldDisp == "—" && newDisp == "—")
                        continue;
                    list.Add(new RoleFieldChangeRowDto(key, oldDisp, newDisp, operatorUserName, changedAt));
                    continue;
                }

                if (key == "customDeptIds")
                {
                    var oldIds = RoleChangeHistory.ExtractDeptIds(prevDoc, previousRequestBody);
                    var newIds = RoleChangeHistory.ExtractDeptIds(currDoc, currentRequestBody);
                    var oldDisp = RoleChangeHistory.FormatDeptNameList(oldIds, deptNameById);
                    var newDisp = RoleChangeHistory.FormatDeptNameList(newIds, deptNameById);
                    if (string.Equals(oldDisp, newDisp, StringComparison.Ordinal))
                        continue;
                    list.Add(new RoleFieldChangeRowDto(key, oldDisp, newDisp, operatorUserName, changedAt));
                    continue;
                }

                var oldNorm = RoleChangeHistory.NormalizeProperty(prevDoc, key, previousRequestBody);
                var newNorm = RoleChangeHistory.NormalizeProperty(currDoc, key, currentRequestBody);
                if (string.Equals(oldNorm, newNorm, StringComparison.Ordinal))
                    continue;
                list.Add(new RoleFieldChangeRowDto(
                    key,
                    RoleChangeHistory.FormatPropertyDisplay(key, prevDoc, key, deptNameById, previousRequestBody),
                    RoleChangeHistory.FormatPropertyDisplay(key, currDoc, key, deptNameById, currentRequestBody),
                    operatorUserName,
                    changedAt));
            }
        }
        catch
        {
            // ignore malformed bodies
        }
        finally
        {
            prevDoc?.Dispose();
            currDoc?.Dispose();
        }

        return list;
    }

    private static class RoleChangeHistory
    {
        internal static readonly string[] TrackedFieldKeys =
        [
            "name",
            "description",
            "dataScope",
            "permissionCodes",
            "customDeptIds",
        ];

        internal static string NormalizeProperty(JsonDocument? doc, string key, string? rawBody = null)
        {
            if (doc != null && doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                var root = ChangeHistoryJson.GetPropertyBagRoot(doc.RootElement);
                if (ChangeHistoryJson.TryGetProperty(root, key, out var el))
                    return NormalizeElement(key, el);
            }

            return ExtractScalarFromRawBody(rawBody, key);
        }

        internal static string FormatPropertyDisplay(
            string fieldKey,
            JsonDocument? doc,
            string propKey,
            IReadOnlyDictionary<string, string> deptNameById,
            string? rawBody = null)
        {
            if (doc != null && doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                var root = ChangeHistoryJson.GetPropertyBagRoot(doc.RootElement);
                if (ChangeHistoryJson.TryGetProperty(root, propKey, out var el))
                    return FormatElement(fieldKey, el, deptNameById);
            }

            var extracted = ExtractScalarFromRawBody(rawBody, propKey);
            if (string.IsNullOrEmpty(extracted))
                return "—";
            if (fieldKey == "dataScope")
                return FormatDataScopeDisplay(extracted);
            if (fieldKey == "permissionCodes")
                return FormatPermissionCountDisplay(extracted);
            return extracted;
        }

        private static string NormalizeElement(string key, JsonElement el)
        {
            if (key == "dataScope" && ChangeHistoryJson.TryCoerceInt32(el, out var scope))
                return scope.ToString(CultureInfo.InvariantCulture);
            if (key == "customDeptIds")
                return string.Join(",", ExtractDeptIdSet(el).OrderBy(x => x, StringComparer.Ordinal));
            if (key == "permissionCodes")
            {
                using var wrapper = JsonDocument.Parse($"{{\"permissionCodes\":{el.GetRawText()}}}");
                return string.Join(
                    ",",
                    PermissionCodesChangeHistoryHelper.ExtractPermissionCodes(wrapper, null)
                        .OrderBy(x => x, StringComparer.Ordinal));
            }
            if (el.ValueKind == JsonValueKind.String)
                return el.GetString() ?? string.Empty;
            return ChangeHistoryJson.NormalizeForCompare(el);
        }

        private static string FormatElement(
            string fieldKey,
            JsonElement el,
            IReadOnlyDictionary<string, string> deptNameById)
        {
            if (fieldKey == "dataScope" && ChangeHistoryJson.TryCoerceInt32(el, out var scope))
                return FormatDataScopeDisplay(scope.ToString(CultureInfo.InvariantCulture));
            if (fieldKey == "permissionCodes")
            {
                using var wrapper = JsonDocument.Parse($"{{\"permissionCodes\":{el.GetRawText()}}}");
                return PermissionCodesChangeHistoryHelper.FormatPermissionCodesDisplay(
                    PermissionCodesChangeHistoryHelper.ExtractPermissionCodes(wrapper, null));
            }
            if (fieldKey == "customDeptIds")
                return FormatDeptNameList(ExtractDeptIdSet(el), deptNameById);
            if (el.ValueKind == JsonValueKind.String)
            {
                var s = el.GetString() ?? string.Empty;
                return string.IsNullOrWhiteSpace(s) ? "—" : s;
            }

            return el.GetRawText();
        }

        internal static HashSet<string> ExtractDeptIds(JsonDocument? doc, string? rawBody)
        {
            if (doc != null && doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                var root = ChangeHistoryJson.GetPropertyBagRoot(doc.RootElement);
                if (ChangeHistoryJson.TryGetProperty(root, "customDeptIds", out var el))
                    return ExtractDeptIdSet(el);
            }

            return ExtractDeptIdsFromRaw(rawBody);
        }

        internal static string FormatDeptNameList(
            IEnumerable<string> deptGuids,
            IReadOnlyDictionary<string, string> deptNameById)
        {
            var names = deptGuids
                .Select(id => deptNameById.TryGetValue(id, out var name) ? name : id)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            return names.Length == 0 ? "—" : string.Join("、", names);
        }

        internal static HashSet<string> ExtractGuidSet(JsonElement el)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (el.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in el.EnumerateArray())
                {
                    if (ChangeHistoryJson.TryReadGuidElement(item, out var guid))
                        ids.Add(guid);
                    else if (item.ValueKind == JsonValueKind.String)
                    {
                        var s = item.GetString();
                        if (!string.IsNullOrWhiteSpace(s) && Guid.TryParse(s, out var g))
                            ids.Add(g.ToString("D"));
                    }
                }

                return ids;
            }

            if (el.ValueKind != JsonValueKind.Object)
                return ids;

            if (ChangeHistoryJson.TryGetProperty(el, "ids", out var idsEl) && idsEl.ValueKind == JsonValueKind.String)
                AddDelimitedValues(idsEl.GetString(), ids);
            else if (ChangeHistoryJson.TryGetProperty(el, "codes", out var codesEl) && codesEl.ValueKind == JsonValueKind.String)
                AddDelimitedValues(codesEl.GetString(), ids);

            return ids;
        }

        private static HashSet<string> ExtractDeptIdSet(JsonElement el)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (el.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in el.EnumerateArray())
                {
                    if (ChangeHistoryJson.TryReadInt64Element(item, out var id))
                        ids.Add(id.ToString(CultureInfo.InvariantCulture));
                    else if (item.ValueKind == JsonValueKind.String)
                    {
                        var s = item.GetString();
                        if (!string.IsNullOrWhiteSpace(s))
                            ids.Add(s.Trim());
                    }
                }

                return ids;
            }

            if (el.ValueKind != JsonValueKind.Object)
                return ids;

            if (ChangeHistoryJson.TryGetProperty(el, "codes", out var codesEl) && codesEl.ValueKind == JsonValueKind.String)
                AddDelimitedValues(codesEl.GetString(), ids);
            else if (ChangeHistoryJson.TryGetProperty(el, "ids", out var idsEl) && idsEl.ValueKind == JsonValueKind.String)
                AddDelimitedValues(idsEl.GetString(), ids);

            return ids;
        }

        private static HashSet<string> ExtractDeptIdsFromRaw(string? rawBody)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(rawBody))
                return ids;

            var objectMatch = System.Text.RegularExpressions.Regex.Match(
                rawBody,
                "\"customDeptIds\"\\s*:\\s*\\{(.*?)\\}",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
                | System.Text.RegularExpressions.RegexOptions.Singleline
                | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
            if (objectMatch.Success)
            {
                var codesMatch = System.Text.RegularExpressions.Regex.Match(
                    objectMatch.Groups[1].Value,
                    "\"(?:ids|codes)\"\\s*:\\s*\"([^\"]+)\"",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
                if (codesMatch.Success)
                    AddDelimitedValues(codesMatch.Groups[1].Value, ids);
                if (ids.Count > 0)
                    return ids;
            }

            var sectionMatch = System.Text.RegularExpressions.Regex.Match(
                rawBody,
                "\"customDeptIds\"\\s*:\\s*\\[(.*?)\\]",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
                | System.Text.RegularExpressions.RegexOptions.Singleline
                | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
            if (!sectionMatch.Success)
                return ids;

            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(
                         sectionMatch.Groups[1].Value,
                         "\\d+",
                         System.Text.RegularExpressions.RegexOptions.CultureInvariant))
            {
                ids.Add(m.Value);
            }

            return ids;
        }

        private static void AddDelimitedValues(string? text, HashSet<string> ids)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            foreach (var part in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!string.IsNullOrWhiteSpace(part))
                    ids.Add(part);
            }
        }

        private static string ExtractScalarFromRawBody(string? rawBody, string key)
        {
            if (string.IsNullOrWhiteSpace(rawBody))
                return string.Empty;

            if (key == "permissionCodes")
                return PermissionCodesChangeHistoryHelper.FormatPermissionCountFromRaw(rawBody);

            if (key == "dataScope")
            {
                var match = System.Text.RegularExpressions.Regex.Match(
                    rawBody,
                    "\"dataScope\"\\s*:\\s*(\\d+)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
                return match.Success ? match.Groups[1].Value : string.Empty;
            }

            var pattern = key switch
            {
                "name" or "description" => $"\"{key}\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"",
                _ => $"\"{key}\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"",
            };
            var textMatch = System.Text.RegularExpressions.Regex.Match(
                rawBody,
                pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
            return textMatch.Success
                ? System.Text.RegularExpressions.Regex.Unescape(textMatch.Groups[1].Value)
                : string.Empty;
        }

        private static string FormatDataScopeDisplay(string normalized)
        {
            if (!int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var scope))
                return string.IsNullOrWhiteSpace(normalized) ? "—" : normalized;

            return scope switch
            {
                0 => "全部数据",
                1 => "本部门",
                2 => "本部门及下级",
                3 => "仅本人",
                4 => "自定义部门及下级",
                _ => normalized,
            };
        }

        private static string FormatPermissionCountDisplay(string normalizedCount)
        {
            if (!int.TryParse(normalizedCount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) || count <= 0)
                return "—";
            return $"{count} 项权限";
        }
    }
}
