using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Services;

namespace Ncp.Admin.Web.Services;

/// <summary>
/// 从当前请求的 JWT claims 解析数据权限上下文（登录时已写入 data_scope、dept_id、authorized_dept_ids）。
/// 应在请求管道早期通过中间件调用 <see cref="LoadAsync"/>。
/// </summary>
public sealed class DataPermissionProvider(IHttpContextAccessor httpContextAccessor) : IDataPermissionProvider
{
    private DataScope _scope = DataScope.All;
    private UserId? _userId;
    private DeptId? _deptId;
    private IReadOnlyList<DeptId> _authorizedDeptIds = [];
    private bool _loaded;

    public DataScope Scope => _scope;
    public UserId? UserId => _userId;
    public DeptId? DeptId => _deptId;
    public IReadOnlyList<DeptId> AuthorizedDeptIds => _authorizedDeptIds;

    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_loaded)
            return Task.CompletedTask;

        var context = httpContextAccessor.HttpContext;
        var userIdClaim = context?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userIdValue))
        {
            _loaded = true;
            return Task.CompletedTask;
        }

        _userId = new UserId(userIdValue);

        var dataScopeClaim = context?.User?.FindFirstValue("data_scope");
        if (!string.IsNullOrEmpty(dataScopeClaim) && int.TryParse(dataScopeClaim, out var scopeValue) &&
            scopeValue >= (int)DataScope.All && scopeValue <= (int)DataScope.Self)
            _scope = (DataScope)scopeValue;

        var deptIdClaim = context?.User?.FindFirstValue("dept_id");
        if (!string.IsNullOrEmpty(deptIdClaim) && long.TryParse(deptIdClaim, out var deptIdValue))
            _deptId = new DeptId(deptIdValue);

        var authorizedDeptIdsClaim = context?.User?.FindFirstValue("authorized_dept_ids");
        if (!string.IsNullOrEmpty(authorizedDeptIdsClaim))
        {
            _authorizedDeptIds = authorizedDeptIdsClaim
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Where(s => long.TryParse(s.Trim(), out _))
                .Select(s => new DeptId(long.Parse(s.Trim())))
                .ToList();
        }
        else if (_deptId != null)
        {
            _authorizedDeptIds = [_deptId];
        }

        _loaded = true;
        return Task.CompletedTask;
    }
}
