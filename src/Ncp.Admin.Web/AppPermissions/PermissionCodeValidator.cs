namespace Ncp.Admin.Web.AppPermissions;

/// <summary>
/// 权限码校验与规范化。
/// </summary>
public static class PermissionCodeValidator
{
    public static IReadOnlyList<string> NormalizeAndValidate(IEnumerable<string> codes)
    {
        var list = (codes ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var invalid = list
            .Where(code => !PermissionDefinitionContext.PermissionsByCode.ContainsKey(code))
            .ToList();
        if (invalid.Count > 0)
        {
            var preview = string.Join("、", invalid.Take(10));
            throw new KnownException(
                invalid.Count > 10
                    ? $"存在无效权限码：{preview} 等 {invalid.Count} 项"
                    : $"存在无效权限码：{preview}");
        }

        return list;
    }
}
