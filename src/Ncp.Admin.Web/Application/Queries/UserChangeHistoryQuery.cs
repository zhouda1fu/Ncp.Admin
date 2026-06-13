using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.OperationLogAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>用户字段变更历史行。</summary>
public record UserFieldChangeRowDto(
    string FieldKey,
    string OldDisplay,
    string NewDisplay,
    string OperatorUserName,
    DateTimeOffset ChangedAt);

/// <summary>用户字段变更历史（由操作日志相邻版本对比得到）。</summary>
public sealed class UserChangeHistoryQuery(ApplicationDbContext dbContext) : IQuery
{
    public async Task<PagedData<UserFieldChangeRowDto>> GetUserFieldChangeHistoryPagedAsync(
        UserId userId,
        PageRequest input,
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        var uid = userId.Id.ToString(CultureInfo.InvariantCulture);

        var updates = (await dbContext.OperationLogs.AsNoTracking()
            .Where(o =>
                o.IsSuccess
                && o.OperationType == OperationLogType.Update
                && o.RequestMethod == "PUT"
                && (o.RequestPath.ToLower().Contains("/api/admin/user/")
                    || o.RequestPath.ToLower().Contains("/api/admin/users/"))
                && o.RequestBody != null)
            .OrderBy(o => o.CreatedAt)
            .Select(o => new { o.RequestPath, o.RequestBody, o.OperatorUserName, o.CreatedAt })
            .ToListAsync(cancellationToken))
            .Where(o => BodyBelongsToUser(o.RequestBody, userId))
            .ToList();

        var roleUpdateLogs = (await dbContext.OperationLogs.AsNoTracking()
            .Where(o =>
                o.IsSuccess
                && o.OperationType == OperationLogType.Update
                && o.RequestMethod == "PUT"
                && o.RequestPath.ToLower().Contains("/api/admin/users/update-roles")
                && o.RequestBody != null)
            .OrderBy(o => o.CreatedAt)
            .Select(o => new { o.RequestBody, o.OperatorUserName, o.CreatedAt })
            .ToListAsync(cancellationToken))
            .Where(o => BodyBelongsToUser(o.RequestBody, userId))
            .ToList();

        string? createBody = null;
        var createCandidates = await dbContext.OperationLogs.AsNoTracking()
            .Where(o =>
                o.IsSuccess
                && o.OperationType == OperationLogType.Create
                && o.RequestMethod == "POST"
                && o.RequestPath.ToLower().EndsWith("/api/admin/users"))
            .OrderByDescending(o => o.CreatedAt)
            .Take(80)
            .Select(o => new { o.RequestBody, o.ResponseBody })
            .ToListAsync(cancellationToken);

        foreach (var c in createCandidates)
        {
            if (TryGetUserIdFromCreateResponse(c.ResponseBody, out var respId) && respId == userId.Id)
            {
                createBody = c.RequestBody;
                break;
            }
        }

        var roleNameById = await LoadRoleNamesAsync(cancellationToken);
        var rows = new List<UserFieldChangeRowDto>();
        var prevBody = createBody;
        foreach (var u in updates.Where(u => !u.RequestPath.Contains("update-roles", StringComparison.OrdinalIgnoreCase)))
        {
            rows.AddRange(DiffUserSnapshots(prevBody, u.RequestBody!, u.OperatorUserName ?? string.Empty, u.CreatedAt, roleNameById));
            prevBody = u.RequestBody;
        }

        foreach (var u in roleUpdateLogs)
        {
            using var doc = ChangeHistoryJson.TryParse(u.RequestBody);
            var roleIds = UserChangeHistory.ExtractRoleIds(doc, u.RequestBody);
            var display = UserChangeHistory.FormatRoleNameList(roleIds, roleNameById);
            if (display == "—")
                continue;

            rows.Add(new UserFieldChangeRowDto(
                "roleIds",
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
                && o.RequestPath.ToLower().EndsWith("/" + uid))
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new { o.OperatorUserName, o.CreatedAt })
            .FirstOrDefaultAsync(cancellationToken);

        if (deleteInfo != null)
        {
            rows.Add(new UserFieldChangeRowDto(
                "_userDeleted",
                "—",
                "已删除",
                deleteInfo.OperatorUserName ?? string.Empty,
                deleteInfo.CreatedAt));
        }

        rows.Sort((a, b) => b.ChangedAt.CompareTo(a.ChangedAt));
        rows = await HydrateUserRowsAsync(rows, cancellationToken);
        return Paginate(rows, input, keyword);
    }

    private async Task<List<UserFieldChangeRowDto>> HydrateUserRowsAsync(
        List<UserFieldChangeRowDto> rows,
        CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
            return rows;

        var roleRows = rows
            .Select(r => new RoleFieldChangeRowDto(r.FieldKey, r.OldDisplay, r.NewDisplay, r.OperatorUserName, r.ChangedAt))
            .ToList();
        var hydrated = await ChangeHistoryOperatorDisplayHelper.HydrateRoleRowsAsync(dbContext, roleRows, cancellationToken);
        return hydrated
            .Select(r => new UserFieldChangeRowDto(r.FieldKey, r.OldDisplay, r.NewDisplay, r.OperatorUserName, r.ChangedAt))
            .ToList();
    }

    private async Task<Dictionary<string, string>> LoadRoleNamesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Roles.AsNoTracking()
            .Select(r => new { Id = r.Id.Id, r.Name })
            .ToDictionaryAsync(x => x.Id.ToString("D"), x => x.Name, StringComparer.OrdinalIgnoreCase, cancellationToken);
    }

    private static PagedData<UserFieldChangeRowDto> Paginate(
        List<UserFieldChangeRowDto> rows,
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
        return new PagedData<UserFieldChangeRowDto>(slice, total, pageIndex, pageSize);
    }

    private static bool TryGetUserIdFromCreateResponse(string? responseBody, out long id)
    {
        id = 0;
        if (string.IsNullOrWhiteSpace(responseBody))
            return false;
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (!ChangeHistoryJson.TryGetProperty(doc.RootElement, "data", out var data)
                || data.ValueKind != JsonValueKind.Object)
                return false;
            if (!ChangeHistoryJson.TryGetProperty(data, "userId", out var idEl))
                return false;
            return ChangeHistoryJson.TryReadInt64Element(idEl, out id);
        }
        catch
        {
            return false;
        }
    }

    private static bool BodyBelongsToUser(string? requestBody, UserId userId)
    {
        if (TryGetRequestUserId(requestBody, out var id) && id == userId)
            return true;

        var uid = userId.Id.ToString(CultureInfo.InvariantCulture);
        return !string.IsNullOrWhiteSpace(requestBody)
            && (requestBody.Contains($"\"userId\":{uid}", StringComparison.Ordinal)
                || requestBody.Contains($"\"userId\":\"{uid}\"", StringComparison.OrdinalIgnoreCase)
                || requestBody.Contains($"\"id\":{uid}", StringComparison.Ordinal)
                || requestBody.Contains($"\"id\":\"{uid}\"", StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryGetRequestUserId(string? requestBody, out UserId id)
    {
        id = default!;
        if (string.IsNullOrWhiteSpace(requestBody))
            return false;
        try
        {
            using var doc = JsonDocument.Parse(requestBody);
            var root = ChangeHistoryJson.GetPropertyBagRoot(doc.RootElement);
            if (ChangeHistoryJson.TryGetProperty(root, "userId", out var el)
                && ChangeHistoryJson.TryReadInt64Element(el, out var userId))
            {
                id = new UserId(userId);
                return true;
            }
        }
        catch
        {
            // 截断/损坏 JSON 时走正则兜底
        }

        return TryExtractUserIdFromRaw(requestBody, out id);
    }

    private static bool TryExtractUserIdFromRaw(string? requestBody, out UserId id)
    {
        id = default!;
        if (string.IsNullOrWhiteSpace(requestBody))
            return false;

        var match = System.Text.RegularExpressions.Regex.Match(
            requestBody,
            "\"userId\"\\s*:\\s*(?:\"?(\\d+)\"?|\\{\\s*\"id\"\\s*:\\s*\"?(\\d+)\"?\\s*\\})",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        if (!match.Success)
            return false;

        var idText = !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value;
        if (!long.TryParse(idText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var userId))
            return false;

        id = new UserId(userId);
        return true;
    }

    private static IReadOnlyList<UserFieldChangeRowDto> DiffUserSnapshots(
        string? previousRequestBody,
        string currentRequestBody,
        string operatorUserName,
        DateTimeOffset changedAt,
        IReadOnlyDictionary<string, string> roleNameById)
    {
        var list = new List<UserFieldChangeRowDto>();
        JsonDocument? prevDoc = null;
        JsonDocument? currDoc = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(previousRequestBody))
                prevDoc = ChangeHistoryJson.TryParse(previousRequestBody);
            currDoc = ChangeHistoryJson.TryParse(currentRequestBody);

            foreach (var key in UserChangeHistory.TrackedFieldKeys)
            {
                if (key == "roleIds")
                    continue;

                var oldNorm = UserChangeHistory.NormalizeProperty(prevDoc, key, previousRequestBody);
                var newNorm = UserChangeHistory.NormalizeProperty(currDoc, key, currentRequestBody);
                if (string.Equals(oldNorm, newNorm, StringComparison.Ordinal))
                    continue;
                list.Add(new UserFieldChangeRowDto(
                    key,
                    UserChangeHistory.FormatPropertyDisplay(key, prevDoc, key, roleNameById, previousRequestBody),
                    UserChangeHistory.FormatPropertyDisplay(key, currDoc, key, roleNameById, currentRequestBody),
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

    private static class UserChangeHistory
    {
        internal static readonly string[] TrackedFieldKeys =
        [
            "name",
            "email",
            "phone",
            "realName",
            "status",
            "gender",
            "age",
            "birthDate",
            "deptId",
            "deptName",
            "positionId",
            "positionName",
            "password",
            "idCardNumber",
            "address",
            "education",
            "graduateSchool",
            "avatarUrl",
            "notOrderMeal",
            "attendanceRequired",
            "orderMealSort",
            "wechatGuid",
            "isResigned",
            "resignedTime",
            "roleIds",
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
            IReadOnlyDictionary<string, string> roleNameById,
            string? rawBody = null)
        {
            if (doc != null && doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                var root = ChangeHistoryJson.GetPropertyBagRoot(doc.RootElement);
                if (ChangeHistoryJson.TryGetProperty(root, propKey, out var el))
                    return FormatElement(fieldKey, el, roleNameById);
            }

            var extracted = ExtractScalarFromRawBody(rawBody, propKey);
            if (string.IsNullOrEmpty(extracted))
                return "—";
            return FormatScalarDisplay(fieldKey, extracted);
        }

        internal static HashSet<string> ExtractRoleIds(JsonDocument? doc, string? rawBody)
        {
            if (doc != null && doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                var root = ChangeHistoryJson.GetPropertyBagRoot(doc.RootElement);
                if (ChangeHistoryJson.TryGetProperty(root, "roleIds", out var el))
                    return ExtractRoleGuidSet(el);
            }

            return ExtractRoleGuidsFromRaw(rawBody);
        }

        internal static string FormatRoleNameList(
            IEnumerable<string> roleGuids,
            IReadOnlyDictionary<string, string> roleNameById)
        {
            var names = roleGuids
                .Select(id => roleNameById.TryGetValue(id, out var name) ? name : id)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            return names.Length == 0 ? "—" : string.Join("、", names);
        }

        private static string NormalizeElement(string key, JsonElement el)
        {
            if (key is "status" or "age" or "orderMealSort" && ChangeHistoryJson.TryCoerceInt32(el, out var iv))
                return iv.ToString(CultureInfo.InvariantCulture);
            if (key is "notOrderMeal" or "attendanceRequired" or "isResigned" && ChangeHistoryJson.TryCoerceBoolean(el, out var bv))
                return bv.ToString().ToLowerInvariant();
            if (key is "deptId" or "positionId" && ChangeHistoryJson.TryReadInt64Element(el, out var id))
                return id.ToString(CultureInfo.InvariantCulture);
            if (key == "roleIds")
                return string.Join(",", ExtractRoleGuidSet(el).OrderBy(x => x, StringComparer.Ordinal));
            if (key == "password")
                return string.IsNullOrWhiteSpace(el.GetString()) ? string.Empty : "***";
            if (el.ValueKind == JsonValueKind.String)
                return el.GetString() ?? string.Empty;
            return ChangeHistoryJson.NormalizeForCompare(el);
        }

        private static string FormatElement(
            string fieldKey,
            JsonElement el,
            IReadOnlyDictionary<string, string> roleNameById)
        {
            if (fieldKey is "status" or "age" or "orderMealSort" && ChangeHistoryJson.TryCoerceInt32(el, out var iv))
                return FormatScalarDisplay(fieldKey, iv.ToString(CultureInfo.InvariantCulture));
            if (fieldKey is "notOrderMeal" or "attendanceRequired" or "isResigned" && ChangeHistoryJson.TryCoerceBoolean(el, out var bv))
                return FormatScalarDisplay(fieldKey, bv.ToString().ToLowerInvariant());
            if (fieldKey is "deptId" or "positionId" && ChangeHistoryJson.TryReadInt64Element(el, out var id))
                return id.ToString(CultureInfo.InvariantCulture);
            if (fieldKey == "roleIds")
                return FormatRoleNameList(ExtractRoleGuidSet(el), roleNameById);
            if (fieldKey == "password")
                return string.IsNullOrWhiteSpace(el.GetString()) ? "—" : "***";
            if (el.ValueKind == JsonValueKind.String)
            {
                var s = el.GetString() ?? string.Empty;
                return string.IsNullOrWhiteSpace(s) ? "—" : FormatScalarDisplay(fieldKey, s);
            }

            return FormatScalarDisplay(fieldKey, el.GetRawText());
        }

        private static HashSet<string> ExtractRoleGuidSet(JsonElement el)
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
                AddDelimitedGuids(idsEl.GetString(), ids);
            else if (ChangeHistoryJson.TryGetProperty(el, "codes", out var codesEl) && codesEl.ValueKind == JsonValueKind.String)
                AddDelimitedGuids(codesEl.GetString(), ids);

            return ids;
        }

        private static HashSet<string> ExtractRoleGuidsFromRaw(string? rawBody)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(rawBody))
                return ids;

            var sectionMatch = System.Text.RegularExpressions.Regex.Match(
                rawBody,
                "\"roleIds\"\\s*:\\s*\\[(.*?)\\]",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
                | System.Text.RegularExpressions.RegexOptions.Singleline
                | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
            if (!sectionMatch.Success)
                return ids;

            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(
                         sectionMatch.Groups[1].Value,
                         "[0-9a-fA-F]{8}-(?:[0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}",
                         System.Text.RegularExpressions.RegexOptions.CultureInvariant))
            {
                if (Guid.TryParse(m.Value, out var g))
                    ids.Add(g.ToString("D"));
            }

            return ids;
        }

        private static void AddDelimitedGuids(string? text, HashSet<string> ids)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            foreach (var part in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (Guid.TryParse(part, out var g))
                    ids.Add(g.ToString("D"));
            }
        }

        private static string ExtractScalarFromRawBody(string? rawBody, string key)
        {
            if (string.IsNullOrWhiteSpace(rawBody))
                return string.Empty;

            if (key is "status" or "age" or "orderMealSort" or "deptId" or "positionId")
            {
                var numMatch = System.Text.RegularExpressions.Regex.Match(
                    rawBody,
                    $"\"{key}\"\\s*:\\s*(\\d+)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
                return numMatch.Success ? numMatch.Groups[1].Value : string.Empty;
            }

            if (key is "notOrderMeal" or "attendanceRequired" or "isResigned")
            {
                var boolMatch = System.Text.RegularExpressions.Regex.Match(
                    rawBody,
                    $"\"{key}\"\\s*:\\s*(true|false)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
                return boolMatch.Success ? boolMatch.Groups[1].Value.ToLowerInvariant() : string.Empty;
            }

            if (key == "password")
            {
                var pwdMatch = System.Text.RegularExpressions.Regex.Match(
                    rawBody,
                    "\"password\"\\s*:\\s*\"",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
                return pwdMatch.Success ? "***" : string.Empty;
            }

            var pattern = $"\"{key}\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"";
            var textMatch = System.Text.RegularExpressions.Regex.Match(
                rawBody,
                pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
            return textMatch.Success
                ? System.Text.RegularExpressions.Regex.Unescape(textMatch.Groups[1].Value)
                : string.Empty;
        }

        private static string FormatScalarDisplay(string fieldKey, string normalized)
        {
            if (string.IsNullOrWhiteSpace(normalized))
                return "—";

            return fieldKey switch
            {
                "status" => normalized switch
                {
                    "0" => "禁用",
                    "1" => "启用",
                    _ => normalized,
                },
                "notOrderMeal" or "attendanceRequired" or "isResigned" => normalized.Equals("true", StringComparison.OrdinalIgnoreCase) ? "是" : "否",
                "password" => "***",
                _ => normalized,
            };
        }
    }
}
