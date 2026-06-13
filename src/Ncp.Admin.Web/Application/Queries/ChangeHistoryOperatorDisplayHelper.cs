using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>将修改记录中的操作人账号解析为用户姓名（RealName）。</summary>
internal static class ChangeHistoryOperatorDisplayHelper
{
    public static async Task<List<RoleFieldChangeRowDto>> HydrateRoleRowsAsync(
        ApplicationDbContext dbContext,
        List<RoleFieldChangeRowDto> rows,
        CancellationToken cancellationToken = default)
    {
        if (rows.Count == 0)
            return rows;

        var map = await LoadRealNameByAccountsAsync(
            dbContext,
            rows.Select(r => r.OperatorUserName),
            cancellationToken);

        return rows
            .Select(r => map.TryGetValue(r.OperatorUserName?.Trim() ?? string.Empty, out var realName)
                ? r with { OperatorUserName = realName }
                : r)
            .ToList();
    }

    private static async Task<Dictionary<string, string>> LoadRealNameByAccountsAsync(
        ApplicationDbContext dbContext,
        IEnumerable<string?> accounts,
        CancellationToken cancellationToken)
    {
        var names = accounts
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (names.Count == 0)
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var users = await dbContext.Users.AsNoTracking()
            .Where(u => names.Contains(u.Name))
            .Select(u => new { u.Name, u.RealName })
            .ToListAsync(cancellationToken);

        return users
            .Where(u => !string.IsNullOrWhiteSpace(u.RealName))
            .ToDictionary(
                u => u.Name.Trim(),
                u => u.RealName.Trim(),
                StringComparer.OrdinalIgnoreCase);
    }
}
